﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class Health : NetworkBehaviour
{
    [SyncVar]
    public int myMaxHealth = 100;
    [SyncVar]
    public int myCurrentHealth = 100;

    public delegate void HealthChanged();

    [SyncEvent]
    public event HealthChanged EventOnHealthChange;

    public void TakeDamage(int aValue)
    {
        if (!isServer)
        {
            return;
        }

        myCurrentHealth -= aValue;
        if(myCurrentHealth <= 0)
        {
            myCurrentHealth = 0;
        }

        RpcHealthChanged();
    }

    public void GainHealth(int aValue)
    {
        if (!isServer)
        {
            return;
        }

        myCurrentHealth += aValue;
        if (myCurrentHealth > myMaxHealth)
        {
            myCurrentHealth = myMaxHealth;
        }

        RpcHealthChanged();
    }

    public bool IsDead()
    {
        return myCurrentHealth <= 0;
    }

    public float GetHealthPercentage()
    {
        return (float)myCurrentHealth / myMaxHealth;
    }

    public int MaxHealth
    {
        get { return myMaxHealth; }
        set
        {
            myMaxHealth = value;
            RpcHealthChanged();
        }
    }

    [ClientRpc]
    private void RpcHealthChanged()
    {
        EventOnHealthChange?.Invoke();
    }
}
