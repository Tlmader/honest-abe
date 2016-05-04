﻿using UnityEngine;
using System.Collections;
using System;

public class Stun : MonoBehaviour
{
    public enum State { Null, Stunned }
    public enum Direction { Left, Right }
    public enum Power { Light, Heavy, Shoot }

    public State state;
    public float stunDuration = 0.0f;

    private Vector2 velocity = Vector2.zero;
    private float stunTimer = 0;
    private CharacterState _characterState;
	private KnockDown _knockdown;
	private Jump _jump;
	private BaseCollision _collision;
    private Animator _animator;
    private float knockbackAmount;
    private Vector2 previousPos, currentPos;
    private GenericAnimation genericAnimation;
    private System.Random random;
    private float stunTransition = 0.1f;

    private void Awake()
    {
        _collision = GetComponent<BaseCollision>();
        _characterState = GetComponent<CharacterState>();
		_knockdown = GetComponent<KnockDown>();
		_jump = GetComponent<Jump>();
		_animator = GetComponent<Animator>();
        genericAnimation = GetComponent<GenericAnimation>();
        random = new System.Random();
    }

    private void OnEnable()
    {
        _collision.OnCollisionEnter += OnCollision;
    }

    private void OnDisable()
    {
        state = State.Null;
        _collision.OnCollisionEnter -= OnCollision;
    }

    private void OnCollision(Collider2D collider)
	{
		if (!_characterState.CanBeStunned()) // || _knockdown.state != KnockDown.State.Null || (tag == "Player" && _jump.state != Jump.State.Null))
			return;

		AttackArea attackArea = collider.GetComponent<AttackArea>();
        if (attackArea && attackArea.IsShootType())
            return;

        if (collider.tag == "Damage")
        {
            if (tag != "Player" || ShouldStunPlayer(attackArea))
            { // only 50% chance to stun if it's Abe				
                Attack attack = collider.GetComponentInParent<Attack>();
                float directionMod = (collider.GetComponentInParent<Movement>().direction == Movement.Direction.Right ? 1f : -1f);
				
                if (attack.attackState == Attack.State.Heavy)
                {
                    if (attack.getAttackHand() == Attack.Hand.Right)
                        GetStunned(attack.GetStunAmount(), attack.GetKnockbackAmount(), directionMod, Direction.Right, Power.Heavy);
                    else
                        GetStunned(attack.GetStunAmount(), attack.GetKnockbackAmount(), directionMod, Direction.Left, Power.Heavy);
                }
                else // if (attack.attackState == Attack.State.Light)
                {
                    if (attack.getAttackHand() == Attack.Hand.Right)
                        GetStunned(attack.GetStunAmount(), attack.GetKnockbackAmount(), directionMod, Direction.Right, Power.Light);
                    else
                        GetStunned(attack.GetStunAmount(), attack.GetKnockbackAmount(), directionMod, Direction.Left, Power.Light);
                }
            }

            if (tag == "Player")
                GetComponent<Attack>().SetState(Attack.State.Null);
        }
    }

    private bool ShouldStunPlayer(AttackArea attackArea)
    {
		bool chanceToRandomlyHurtPlayer = attackArea.GetComponentInParent<Transform>().tag == "Boss" || random.Next() > 0.5;
        bool isAttackKnife = attackArea && attackArea.GetAttackType() == Weapon.AttackType.Knife;
        return chanceToRandomlyHurtPlayer && !isAttackKnife;
    }

    private void Update()
    {
        if (state != State.Stunned)
        {
            previousPos = transform.position;
            return;
        }

        currentPos = transform.position;
        if (Math.Abs(previousPos.x - currentPos.x) >= knockbackAmount / 4)
        {
            velocity = Vector2.zero;
        }

        stunDuration -= Time.deltaTime;
        _collision.Move(velocity * Time.deltaTime);
        if (stunDuration <= 0f)
            FinishStun();
    }

    public void GetStunned(float stunAmount = 1f, float knockbackAmount = 0.1f, float directionModifier = 1f,
        Direction direction = Direction.Left, Power power = Power.Light)
    {
        if (_characterState.state == CharacterState.State.Grabbed)
        {
            _animator.TransitionPlay("Grabbed Damage", stunTransition, 0.15f);
            Invoke("FinishGrabbedStun", 0.5f);
            return;
        }
        this.knockbackAmount = knockbackAmount;
        if (power == Power.Shoot)
        {
            _animator.Play("Gun Shot Damage Reaction");
        }
        else if (power == Power.Heavy)
        {
            if (direction == Direction.Right)
                _animator.TransitionPlay("Heavy Damage Reaction Right", stunTransition, 0.25f);
            else // if (direction == Direction Left)
                _animator.TransitionPlay("Heavy Damage Reaction Left", stunTransition, 0.25f);
        }
        else // if (power == Power.Light)
        {
            if (direction == Direction.Right)
                _animator.TransitionPlay("Light Damage Reaction Right", stunTransition, 0.25f);
            else // if (direction == Direction Left)
                _animator.TransitionPlay("Light Damage Reaction Left", stunTransition, 0.25f);
        }
        state = State.Stunned;
        velocity = new Vector2((directionModifier), 0).normalized * knockbackAmount * 2;
        _characterState.SetState(CharacterState.State.Stun);
        if (tag == "Player")
            stunDuration = stunAmount;
        else
            stunDuration = stunAmount * 2;
    }

    private void FinishStun()
    {
        state = State.Null;
        velocity = Vector2.zero;
        _characterState.SetState(CharacterState.State.Idle);
    }

    private void FinishGrabbedStun()
    {
        if (genericAnimation) genericAnimation.UpdateState();
        if (_characterState.state == CharacterState.State.Grabbed)
            _animator.TransitionPlay("Grabbed");
    }
}
