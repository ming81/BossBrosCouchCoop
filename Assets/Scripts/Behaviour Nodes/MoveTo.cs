﻿using UnityEngine;
using UnityEngine.AI;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;

[TaskDescription("Will move to one of the set values in the inspector.")]
public class MoveTo : Action
{
    public SharedGameObject myTarget;

    public GameObject myGameObject;
    public Vector3 myWorldPosition;

    public float myStopDistance = 0.1f;

    [BehaviorDesigner.Runtime.Tasks.Tooltip("When reaching target, rotates same directon as gameobject")]
    public bool myShouldApplyGameObjectRotation = false;

    private NavMeshAgent myNavmeshAgent;
    private Animator myAnimator;

    private enum TargetType
    {
        SharedGameObject,
        GameObject,
        WorldPosition
    }
    private TargetType myTargetType;

    public override void OnAwake()
    {
        myNavmeshAgent = GetComponent<NavMeshAgent>();
        myAnimator = GetComponent<Animator>();
    }

    public override void OnStart()
    {
        if (myTarget.Value != null)
            myTargetType = TargetType.SharedGameObject;
        else if (myGameObject)
            myTargetType = TargetType.GameObject;
        else
            myTargetType = TargetType.WorldPosition;

        myNavmeshAgent.isStopped = false;
    }

    public override TaskStatus OnUpdate()
    {
        Vector3 targetPosition = myWorldPosition;
        Quaternion targetRotation = Quaternion.identity;
        if (myTargetType == TargetType.SharedGameObject)
        {
            targetPosition = myTarget.Value.transform.position;
        }
        if (myTargetType == TargetType.GameObject)
        {
            targetPosition = myGameObject.transform.position;
            if (myShouldApplyGameObjectRotation)
                targetRotation = myGameObject.transform.rotation;
        }



        myNavmeshAgent.destination = targetPosition;
        float distanceSqr = (new VectorXZ(targetPosition) - new VectorXZ(transform.position)).sqrMagnitude;
        if (distanceSqr <= myStopDistance * myStopDistance && myNavmeshAgent.remainingDistance <= myStopDistance)
        {
            myNavmeshAgent.destination = transform.position;

            if (myShouldApplyGameObjectRotation)
                transform.rotation = targetRotation;

            return TaskStatus.Success;
        }

        return TaskStatus.Running;
    }
}
