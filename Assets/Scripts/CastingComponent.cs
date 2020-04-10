﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class CastingComponent : MonoBehaviour
{
    protected UIComponent myUIComponent;
    protected AnimatorWrapper myAnimatorWrapper;

    protected GameObject myChannelGameObject;
    protected Coroutine myCastingRoutine;
    protected float myAutoAttackCooldown;
    protected float myAutoAttackCooldownReset = 1.0f;

    protected bool myIsCasting;
    public bool IsInterruptable { get; set; }

    protected virtual void Awake()
    {
        myUIComponent = GetComponent<UIComponent>();
        myAnimatorWrapper = GetComponent<AnimatorWrapper>();
    }

    void Start()
    {
        myIsCasting = false;
        IsInterruptable = true;
    }

    void Update()
    {
        
    }

    protected abstract IEnumerator CastbarProgress(int aKeyIndex);

    public abstract IEnumerator SpellChannelRoutine(float aDuration, float aStunDuration);

    public void StartChannel(float aDuration, Spell aSpellScript, GameObject aChannelGO, float aStunDuration = 1.0f)
    {
        GetComponent<UIComponent>().SetCastbarChannelingStartValues(aSpellScript, aDuration);
        GetComponent<AudioSource>().clip = aSpellScript.GetSpellSFX().myCastSound;
        GetComponent<AudioSource>().Play();

        myChannelGameObject = aChannelGO;
        myCastingRoutine = StartCoroutine(SpellChannelRoutine(aDuration, aStunDuration));
    }

    protected abstract bool IsAbleToCastSpell(Spell aSpellScript);

    public Vector3 GetSpellSpawnPosition(Spell aSpellScript)
    {
        GameObject target = GetComponent<TargetingComponent>().Target;
        if (aSpellScript.mySpeed <= 0.0f && !aSpellScript.myIsOnlySelfCast && target != null)
        {
            return target.transform.position;
        }

        return transform.position;
    }

    protected virtual void StopCasting()
    {
        if (myCastingRoutine != null)
            StopCoroutine(myCastingRoutine);
        myIsCasting = false;
        GetComponent<UIComponent>().SetCastbarInterrupted();
        GetComponent<AudioSource>().Stop();

        if (myAnimatorWrapper)
        {
            myAnimatorWrapper.SetBool(AnimationVariable.IsCasting, false);
            myAnimatorWrapper.SetTrigger(AnimationVariable.CastingDone);
        }
    }

    public void InterruptSpellCast()
    {
        if (myIsCasting && IsInterruptable)
        {
            StopCasting();
        }
    }

    private void OnDeath()
    {
        StopAllCoroutines();
        StopCasting();
        GetComponent<AudioSource>().Stop();

        if (myChannelGameObject)
        {
            PoolManager.Instance.ReturnObject(myChannelGameObject, myChannelGameObject.GetComponent<UniqueID>().GetID());
            myChannelGameObject = null;
        }
    }
}
