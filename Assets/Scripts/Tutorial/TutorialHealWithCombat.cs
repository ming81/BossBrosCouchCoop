﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BehaviorDesigner.Runtime;

public class TutorialHealWithCombat : TutorialCompletion
{
    [Header("The Totem")]
    [SerializeField]
    private GameObject myTotemBoss = null;
    [SerializeField]
    private List<ParticleSystem> myBurningEyes = new List<ParticleSystem>();

    [SerializeField]
    private Collider myStartFightCollider = null;

    private Subscriber mySubscriber;

    protected override bool StartTutorial()
    {
        if (!base.StartTutorial())
            return false;

        return true;
    }

    private void StartFight()
    {
        myTotemBoss.GetComponent<BehaviorTree>().enabled = true;
        myTotemBoss.GetComponent<BehaviorTree>().SendEvent("Activate");
        foreach (ParticleSystem burningEyes in myBurningEyes)
        {
            burningEyes.Clear();
            burningEyes.Play();
        }

        myTotemBoss.GetComponent<Enemy>().Players = myPlayers;

        mySubscriber = new Subscriber();
        mySubscriber.EventOnReceivedMessage += ReceiveMessage;

        PostMaster.Instance.RegisterSubscriber(ref mySubscriber, MessageCategory.TutorialHealFightComplete);
    }

    private void ReceiveMessage(Message aMessage)
    {
        PostMaster.Instance.UnregisterSubscriber(ref mySubscriber, MessageCategory.TutorialHealFightComplete);
        mySubscriber.EventOnReceivedMessage -= ReceiveMessage;

        myTotemBoss.GetComponent<BehaviorTree>().enabled = true;
        myTotemBoss.GetComponent<BehaviorTree>().SendEvent("Deactivate");

        foreach (ParticleSystem burningEyes in myBurningEyes)
            burningEyes.Stop();

        EndTutorial();
    }

    public override void OnChildTriggerEnter(Collider aChildCollider, Collider aHit)
    {
        if (aChildCollider == myStartCollider)
            StartTutorial();

        if (aChildCollider == myStartFightCollider)
        {
            myTutorialPanel.gameObject.SetActive(false);
            StartFight();
        }
    }
}
