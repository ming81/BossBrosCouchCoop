﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BehaviorDesigner.Runtime;

public class NPCCastingComponent : CastingComponent
{
    private Resource myResource;
    private BehaviorTree myBehaviorTree;
    private TargetingComponent myTargetingComponent;
    private UIComponent myUIComponent;

    protected override void Awake()
    {
        base.Awake();

        myResource = GetComponent<Resource>();
        myTargetingComponent = GetComponent<TargetingComponent>();
        myUIComponent = GetComponent<UIComponent>();
    }

    public bool CastSpell(GameObject aSpell, GameObject aTarget, Transform aSpawnTransform, bool aShouldIgnoreCastability = false)
    {
        Spell spellScript = aSpell.GetComponent<Spell>();

        if (!aShouldIgnoreCastability && !IsAbleToCastSpell(spellScript))
            return false;

        if (spellScript.myCastTime <= 0.0f)
        {
            SpawnSpell(aSpell, aTarget, aSpawnTransform);
            if (myResource)
                myResource.LoseResource(spellScript.myResourceCost);
            if (myAnimatorWrapper)
                myAnimatorWrapper.SetTrigger(spellScript.myAnimationType);

            return true;
        }

        myAnimatorWrapper.SetBool(AnimationVariable.IsCasting, true);
        myUIComponent.SetCastbarStartValues(spellScript);

        myCastingRoutine = StartCoroutine(CastbarProgress(aSpell, aTarget, aSpawnTransform, aShouldIgnoreCastability));

        return true;
    }

    protected IEnumerator CastbarProgress(GameObject aSpell, GameObject aTarget, Transform aSpawnTransform, bool aShouldIgnoreCastability)
    {
        Spell spellScript = aSpell.GetComponent<Spell>();

        GetComponent<AudioSource>().clip = spellScript.GetSpellSFX().myCastSound;
        GetComponent<AudioSource>().Play();

        myTargetingComponent.SetSpellTarget(aTarget);

        myIsCasting = true;
        float castSpeed = spellScript.myCastTime / myStats.myAttackSpeed;
        float rate = 1.0f / castSpeed;
        float progress = 0.0f;

        while (progress <= 1.0f)
        {
            myUIComponent.SetCastbarValues(Mathf.Lerp(0, 1, progress), (castSpeed - (progress * castSpeed)).ToString("0.0"));

            progress += rate * Time.deltaTime;

            yield return null;
        }

        StopCasting();

        if (aShouldIgnoreCastability || IsAbleToCastSpell(spellScript))
        {
            SpawnSpell(aSpell, myTargetingComponent.SpellTarget, aSpawnTransform);
            if (myResource)
                GetComponent<Resource>().LoseResource(spellScript.myResourceCost);
            myAnimatorWrapper.SetTrigger(AnimationVariable.CastingDone);
        }
    }

    public override IEnumerator SpellChannelRoutine(float aDuration, float aStunDuration)
    {
        Stats stats = GetComponent<Stats>();
        stats.SetStunned(aStunDuration);
        myIsCasting = true;
        float castSpeed = aDuration;
        float rate = 1.0f / castSpeed;
        float progress = 0.0f;

        myAnimatorWrapper.SetBool(AnimationVariable.IsCasting, true);
        UIComponent uiComponent = GetComponent<UIComponent>();

        while (progress <= 1.0f)
        {
            uiComponent.SetCastbarValues(Mathf.Lerp(1, 0, progress), (castSpeed - (progress * castSpeed)).ToString("0.0"));

            progress += rate * Time.deltaTime;

            if (GetComponent<PlayerMovementComponent>().IsMoving() || (Input.GetKeyDown(KeyCode.Escape) && myChannelGameObject != null))
            {
                //myCastbar.SetCastTimeText("Cancelled");
                myAnimatorWrapper.SetTrigger(AnimationVariable.CastingCancelled);
                stats.SetStunned(0.0f);
                StopCasting();
                PoolManager.Instance.ReturnObject(myChannelGameObject, myChannelGameObject.GetComponent<UniqueID>().GetID());
                myChannelGameObject = null;
                yield break;
            }

            yield return null;
        }

        StopCasting();
        if (myChannelGameObject != null)
        {
            PoolManager.Instance.ReturnObject(myChannelGameObject, myChannelGameObject.GetComponent<UniqueID>().GetID());
            myChannelGameObject = null;
        }
    }

    public void SpawnSpell(GameObject aSpell, GameObject aTarget, Transform aSpawnTransform)
    {
        GameObject spellGO = PoolManager.Instance.GetPooledObject(aSpell.GetComponent<UniqueID>().GetID());
        spellGO.transform.position = aSpawnTransform.position;
        spellGO.transform.rotation = aSpawnTransform.rotation;
        spellGO.transform.localScale = aSpawnTransform.localScale;

        Spell spellScript = spellGO.GetComponent<Spell>();
        spellScript.SetParent(transform.gameObject);
        spellScript.AddDamageIncrease(myStats.myDamageIncrease);

        spellScript.SetTarget(aTarget);

        if (aTarget && aSpawnTransform.position != aTarget.transform.position)
            GetComponent<AudioSource>().PlayOneShot(spellScript.GetSpellSFX().mySpawnSound);

        GetComponent<BehaviorTree>().SendEvent("SpellSpawned");
    }

    protected override void StopCasting()
    {
        base.StopCasting();

        if (myBehaviorTree)
            myBehaviorTree.SendEvent("SpellInterrupted");
    }

    protected override bool IsAbleToCastSpell(Spell aSpellScript)
    {
        if (myIsCasting)
        {
            Debug.Log(gameObject.name + " failed to cast spell due to already casting spell");
            return false;
        }

        if (GetComponent<Resource>().myCurrentResource < aSpellScript.myResourceCost)
        {
            Debug.Log(gameObject.name + " failed to cast spell due to no resources");
            return false;
        }

        if (!aSpellScript.IsCastableWhileMoving() && GetComponent<NPCMovementComponent>().IsMoving())
        {
            Debug.Log(gameObject.name + " failed to cast spell while moving");
            return false;
        }

        if (aSpellScript.myIsOnlySelfCast)
            return true;

        GameObject spellTarget = myTargetingComponent.SpellTarget;
        if (!spellTarget)
        {
            if ((aSpellScript.GetSpellTarget() & SpellTarget.Friend) != 0)
            {
                spellTarget = gameObject;
            }
            else
            {
                Debug.Log(gameObject.name + " failed to cast spell due to no target");
                return false;
            }
        }

        if (spellTarget.GetComponent<Health>().IsDead())
        {
            Debug.Log(gameObject.name + " failed to cast spell due to target being dead");
            return false;
        }

        if (!GetComponent<Character>().CanRaycastToObject(spellTarget))
        {
            Debug.Log(gameObject.name + " failed to cast spell due to no vision of target");
            return false;
        }

        float distance = Vector3.Distance(transform.position, spellTarget.transform.position);
        if (distance > aSpellScript.myRange)
        {
            Debug.Log(gameObject.name + " failed to cast spell due to target being without of range");
            return false;
        }

        if ((aSpellScript.GetSpellTarget() & SpellTarget.Enemy) == 0 && spellTarget.tag == "Player")
        {
            Debug.Log(gameObject.name + " can't cast friendly spells on players");
            return false;
        }

        if ((aSpellScript.GetSpellTarget() & SpellTarget.Friend) == 0 && spellTarget.tag == "Enemy")
        {
            Debug.Log(gameObject.name + " can't cast hostile spells on friends");
            return false;
        }

        if (!aSpellScript.myCanCastOnSelf && spellTarget == transform.gameObject)
        {
            Debug.Log(gameObject.name + " failed to cast spell due to spell not castable on self");
            return false;
        }

        return true;
    }
}