﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AutoAttackBoost : Spell
{
    [Header("Duration which the boost will last until auto attack")]
    [SerializeField]
    private float myLifeTime = 4.0f;

    private Animator myParentAnimator;

    protected override void Start()
    {
        myParentAnimator = myParent.GetComponent<Animator>();

        Transform weaponSlot = myParent.transform.FindInChildren("MainHandVFX");
        transform.parent = weaponSlot;
        transform.localPosition = Vector3.zero;
    }
    
    protected override void Update()
    {
        myLifeTime -= Time.deltaTime;
        if(myLifeTime <= 0.0f)
        {
            ReturnToPool();
        }

        Debug.LogWarning("AutoAtk is probably wrong name in code for setting auto attack boost damage when used. Revisit when used.");
        if (myParentAnimator.GetCurrentAnimatorClipInfo(0)[0].clip.name == "AutoAtk")
        {

            myTarget = myParent.GetComponent<TargetingComponent>().Target;
            if (myTarget == null)
                return;

            DealSpellEffect();
            transform.parent = myTarget.transform;
            transform.localPosition = Vector3.zero;

            SpawnVFX(2.5f);

            ReturnToPool();
        }
    }
}
