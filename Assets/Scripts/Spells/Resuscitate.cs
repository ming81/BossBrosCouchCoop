﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Resuscitate : Spell
{
    [SerializeField]
    private float myChannelTime;

    protected override void Start()
    {
        StartCoroutine();
        SpawnVFX(myChannelTime + 1.5f);
    }

    protected override void Update()
    {
        myChannelTime -= Time.deltaTime;
        if (myChannelTime <= 0.0f)
        {
            myTarget.GetComponent<Health>().GainHealth(myDamage);
            myTarget.GetComponent<Player>().OnRevive();
            PostMaster.Instance.PostMessage(new Message(MessageCategory.PlayerResucitated, myTarget.GetInstanceID()));
            ReturnToPool();
        }
    }

    private void StartCoroutine()
    {
        myParent.GetComponent<Player>().StartChannel(myChannelTime, this, null);
    }

    protected override string GetSpellDetail()
    {
        string detail = "to resuscitate the target, brining them back to life after " + myChannelTime + " seconds";

        return detail;
    }
}
