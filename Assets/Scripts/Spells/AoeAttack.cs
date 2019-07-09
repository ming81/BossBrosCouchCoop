﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AoeAttack : Spell
{

    private float myLifeTime = 0.05f;

    [Header("The target to damage")]
    [SerializeField]
    private string myAttackTag = "Enemy";

    protected override void Update()
    {
        myLifeTime -= Time.deltaTime;
        if (myLifeTime <= 0.0f)
        {
            Destroy(gameObject);
            SpawnVFX(2.5f);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == myAttackTag)
        {
            if (myDamage > 0.0f)
                other.GetComponentInParent<Health>().TakeDamage(myDamage, myParent.GetComponent<Character>().myCharacterColor);
        }
    }

    protected override string GetSpellDetail()
    {
        string detail = "to deal damage to everyone around you for " + myDamage + " damage";

        return detail;
    }
}
