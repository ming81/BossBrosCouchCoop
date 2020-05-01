﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShieldWall : PoolableObject
{
    public float myLifeTime;
    private float myCurrentLifeTime = 0.0f;

    public override void Reset()
    {
        myCurrentLifeTime = 0.0f;
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<Spell>())
            other.GetComponent<PoolableObject>().ReturnToPool();     
    }

    void Update()
    {

        myCurrentLifeTime += Time.deltaTime;

        if (myCurrentLifeTime >= myLifeTime)
            PoolManager.Instance.ReturnObject(gameObject, GetComponent<UniqueID>().GetID());
    }
}
