﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using BehaviourMachine;

public class NavigateObstacle : ConditionNode {

	private GameObject player;
	private Vector2 selfPosition;
	private Vector2 playerPosition;
	private Vector2 direction;
	private EnemyFollow enemyFollow;
	private float xDiff;
	private float yDiff;
	private float distanceToPlayer;
	private LayerMask layerMask;
	private RaycastHit2D hit;
	private Vector2 deltaPosition;
	private BaseCollision baseCollision;

	override public void Start(){
		enemyFollow = self.GetComponent<EnemyFollow> ();
		enemyFollow.targetType = EnemyFollow.TargetType.Null;
		player = GameObject.Find("Player");
		selfPosition = self.transform.position;
		playerPosition = player.transform.position;
		direction = playerPosition - selfPosition;
		xDiff = Mathf.Abs (playerPosition.x - selfPosition.x);
		yDiff = Mathf.Abs (playerPosition.y - selfPosition.y);
		distanceToPlayer = Mathf.Sqrt (Mathf.Pow(xDiff,2) + Mathf.Pow(yDiff,2));
		layerMask = LayerMask.GetMask("Environment");
		baseCollision = self.GetComponent<BaseCollision> ();

		// Fire 2 raycasts again and see which ones hit the obstacle
		if (xDiff > yDiff) {
			// Fire a ray above
			RaycastHit2D topHit = Physics2D.Raycast (selfPosition + new Vector2 (0, 1), direction, distanceToPlayer, layerMask);
			Debug.DrawRay (selfPosition + new Vector2 (0, 1), direction);
			// Fire a ray below
			RaycastHit2D bottomHit = Physics2D.Raycast (selfPosition + new Vector2 (0, -1), direction, distanceToPlayer, layerMask);
			Debug.DrawRay (selfPosition + new Vector2 (0, -1), direction);

			// If only the bottom one hits, veer up
			if (bottomHit && bottomHit.collider.tag=="Obstacle") {
				if (!(topHit && topHit.collider.tag == "Obstacle")) {
					veerUp ();
					return;
				} else navigate ('y'); // If they both hit, navigate around the obstacle
			// If only the top one hits, veer down
			} else {
				if (topHit && topHit.collider.tag=="Obstacle") {
					veerDown ();
					return;
				} else navigate('y'); // Should be clear but let's do this anyway
			}
		} else {
			// Fire a ray to the right
			RaycastHit2D rightHit = Physics2D.Raycast (selfPosition + new Vector2 (1, 0), direction, distanceToPlayer, layerMask);
			Debug.DrawRay (selfPosition + new Vector2 (1, 0), direction);
			// Fire a ray to the left
			RaycastHit2D leftHit = Physics2D.Raycast (selfPosition + new Vector2 (-1, 0), direction, distanceToPlayer, layerMask);
			Debug.DrawRay (selfPosition + new Vector2 (-1, 0), direction);

			// If only the left one hits, veer right
			if (leftHit && leftHit.collider.tag=="Obstacle") {
				if (!(rightHit && rightHit.collider.tag=="Obstacle")) {
					veerRight ();
					return;
				} else navigate('x'); // If they both hit, navigate around the obstacle
			// If only the right one hits, veer left
			} else {
				if (rightHit && rightHit.collider.tag=="Obstacle") {
					veerLeft ();
					return;
				} else navigate('x');
			}
		}
	}

	public override Status Update () {	
		if (onSuccess.id != 0)
			owner.root.SendEvent (onSuccess.id);
		return Status.Success;
	}

	/*
	 * Navigate around an obstacle using the specified axis
	 */ 
	public void navigate(char axis){
		int i = 1;
		if (axis == 'y') {
			while (i < 20) {
				if (checkUp (i)) {
					deltaPosition = Vector3.ClampMagnitude((Vector3) direction + new Vector3 (0, i + 1, 0), 0.07f);
					self.transform.position = selfPosition + deltaPosition;
					baseCollision.Move (deltaPosition);
					return;
				}
				if (checkDown (i)) {
					deltaPosition = Vector3.ClampMagnitude((Vector3) direction + new Vector3 (0, -i - 1, 0), 0.07f);
					self.transform.position = selfPosition + deltaPosition;
					return;
				}
				i++;
			}
		} else {
			while (i < 20) {
				if (checkRight (i)) {
					deltaPosition = Vector3.ClampMagnitude((Vector3) direction + new Vector3 (i + 1, 0, 0), 0.07f);
					self.transform.position = selfPosition + deltaPosition;
					return;
				}
				if (checkLeft (i)) {
					deltaPosition = Vector3.ClampMagnitude((Vector3) direction + new Vector3 (-i - 1, 0, 0), 0.07f);
					self.transform.position = selfPosition + deltaPosition;
					return;
				}
				i++;
			}
		}
	}

	public void veerRight(){
		if (playerPosition.y > selfPosition.y) {
			deltaPosition = new Vector3 (0.07f, 0.07f, 0);
		} else {
			deltaPosition = new Vector3 (0.07f, -0.07f, 0);
		}
		self.GetComponent<BaseCollision> ().Move (deltaPosition);
		//self.transform.position = (Vector2) selfPosition + deltaPosition;
	}

	public void veerLeft(){
		if (playerPosition.y > selfPosition.y)
			deltaPosition = new Vector3 (-0.07f, 0.07f, 0);
		else
			deltaPosition = new Vector3 (-0.07f, -0.07f, 0);
		//self.transform.position = (Vector2) selfPosition + deltaPosition;
		self.GetComponent<BaseCollision> ().Move (deltaPosition);

	}

	public void veerUp(){
		if (selfPosition.y + 0.07f < 0) { // because they have to stay on the ground
			if (playerPosition.x > selfPosition.x) {
				deltaPosition = new Vector3 (0.07f, 0.07f, 0);
			} else {
				deltaPosition = new Vector3 (-0.07f, 0.07f, 0);
			}
			self.GetComponent<BaseCollision> ().Move (deltaPosition);
			//self.transform.position = (Vector2) selfPosition + deltaPosition;
		} else {
			veerDown ();
		}
	}

	public void veerDown(){
		if (playerPosition.x > selfPosition.x)
			deltaPosition = new Vector3 (0.07f, -0.07f, 0);
		else
			deltaPosition = new Vector3 (-0.07f, -0.07f, 0);
		//self.transform.position = (Vector2) selfPosition + deltaPosition;
		self.GetComponent<BaseCollision> ().Move (deltaPosition);
	}

	/* Fires the uppermost ray one step (of size i) up from the original direction. 
	 * If it hits, fires another ray 2 up from that. Returns true if it does NOT hit.
	 */ 
	public bool checkUp(int i){
		if (selfPosition.y + i + 1 >= 0) // can't go above the ground
			return false;
		hit = Physics2D.Raycast (selfPosition + new Vector2 (0, 1), direction + new Vector2 (0, i), 2, layerMask); //length 2
		Debug.DrawRay (selfPosition + new Vector2 (0, 1), direction + new Vector2 (0, i)); //length of direction
		if (hit && hit.collider.tag=="Obstacle"){
			return false;
		}
		// Imagine this as the new bottom ray, and fire the corresponding top ray.
		hit = Physics2D.Raycast(selfPosition + new Vector2(0, 3), direction + new Vector2 (0, i), 2, layerMask);
		if (hit && hit.collider.tag == "Obstacle") {
			return false;
		}
		return true;
	}

	/* Fires the bottommost ray one step (of size i) down from the original direction.
	 * If it hits, fires another ray 2 down form that. Returns true if it does NOT hit.
	 */ 
	public bool checkDown(int i){
		hit = Physics2D.Raycast (selfPosition + new Vector2 (0, -1), direction + new Vector2 (0, -i), 2, layerMask);
		Debug.DrawRay (selfPosition + new Vector2 (0, -1), direction + new Vector2 (0, -i));
		if (hit && hit.collider.tag=="Obstacle"){
			return false;
		}
		hit = Physics2D.Raycast(selfPosition + new Vector2(0, -3), direction + new Vector2(0, -i), 2, layerMask);
		if (hit && hit.collider.tag == "Obstacle") {
			return false;
		}
		return true;
	}

	/* Fires the rightmost ray one step (of size i) to the right of the original direction.
	 * If it hits, fires another ray 2 to the right of that. Returns true if it does NOT hit.
	 */ 
	public bool checkRight(int i){
		hit = Physics2D.Raycast (selfPosition + new Vector2 (1, 0), direction + new Vector2 (i, 0), 2, layerMask);
		Debug.DrawRay (selfPosition + new Vector2 (1, 0), direction + new Vector2 (i, 0));
		if (hit && hit.collider.tag=="Obstacle"){
			return false;
		}
		hit = Physics2D.Raycast (selfPosition + new Vector2 (3, 0), direction + new Vector2 (i, 0), 2, layerMask);
		if (hit && hit.collider.tag=="Obstacle"){
			return false;
		}
		return true;
	}

	/* Fires the leftmost ray one step (of size i) to the left of the original direction.
	 * If it hits, fires another ray 2 to the left of that. Returns true if it does NOT hit.
	 */ 
	public bool checkLeft(int i){
		hit = Physics2D.Raycast (selfPosition + new Vector2 (-1, 0), direction + new Vector2 (-i, 0), 2, layerMask);
		Debug.DrawRay (selfPosition + new Vector2 (-1, 0), direction + new Vector2 (-i, 0));
		if (hit && hit.collider.tag=="Obstacle"){
			return false;
		}
		hit = Physics2D.Raycast (selfPosition + new Vector2 (-3, 0), direction + new Vector2 (-i, 0), 2, layerMask);
		if (hit && hit.collider.tag=="Obstacle"){
			return false;
		}
		return true;
	}
}
