﻿using UnityEngine;
using System.Collections;
using BehaviourMachine;

public class ReturnSuccess : ConditionNode {

	// Use this for initialization
	public override void Start () {

	}
	
	// Update is called once per frame
	public override Status Update () {
		if (onSuccess.id != 0)
			owner.root.SendEvent (onSuccess.id);
		return Status.Success;
	}
}
