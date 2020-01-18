﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum SpellType
{
    Damage,
    DOT,
    Heal,
    HOT,
    Shield,
    Interrupt,
    Taunt,
    Slow,
    Buff,
    Ressurect,
    Special
}

[System.Flags]
public enum SpellTarget
{
    Friend = 1 << 1,
    Enemy = 1 << 2,
    Anyone = Friend | Enemy
}

public class Class : MonoBehaviour
{
    public string myClassName;
    public ClassRole myClassRole;

    [Header("Image to show on hud")]
    public Sprite mySprite;

    [SerializeField]
    private GameObject myAutoAttack = null;
    public GameObject[] mySpells;

    private GameObject myActionBar;
    private GameObject[] myActionButtons;

    public float[] myCooldownTimers;

    private float myResource;

    public delegate void EventOnInfoToggle(GameObject aPlayer);
    public event EventOnInfoToggle myEventOnInfoToggle;

    public enum ClassRole
    {
        MeleeDps,
        RangedDps,
        Healer,
        Tank
    }

    public Class()
    {
        const int mySpellSize = 4;
        mySpells = new GameObject[mySpellSize];
        myActionButtons = new GameObject[mySpellSize];
        myCooldownTimers = new float[mySpellSize];

        for (int index = 0; index < myCooldownTimers.Length; index++)
        {
            myCooldownTimers[index] = 0.0f;
        }
    }

    public void Start()
    {
        Transform poolManagerTransform = PoolManager.Instance.transform;
        GameObject poolPrefab = PoolManager.Instance.GetPoolPrefab();

        for (int index = 0; index < mySpells.Length; index++)
        {
            if (mySpells[index] == null)
                continue;

            GameObject pool = Instantiate(poolPrefab, poolManagerTransform);
            pool.name = mySpells[index].name + " Pool";
            pool.GetComponent<ObjectPool>().SetPrefab(mySpells[index]);

            mySpells[index].GetComponent<PoolableObject>().SetParentPool(pool.GetComponent<ObjectPool>());
        }

        myAutoAttack.GetComponent<PoolableObject>().SetParentPool(PoolManager.Instance.GetAutoAttackPool());
    }

    private void FindActionBar(Transform aUIParent)
    {
        myActionBar = aUIParent.Find("ActionBar").gameObject;
        myActionBar.GetComponent<CanvasGroup>().alpha = 1.0f;
    }

    private void Update()
    {
        for (int index = 0; index < myCooldownTimers.Length; index++)
        {
            if (myCooldownTimers[index] > 0.0f)
            {
                myCooldownTimers[index] -= Time.deltaTime;
                if (myActionButtons[index] == null)
                    continue;

                if (myCooldownTimers[index] > 0.0f)
                {
                    myActionButtons[index].GetComponentInChildren<Text>().text = myCooldownTimers[index].ToString("0.0");
                }
                else
                {
                    myActionButtons[index].GetComponent<ActionKey>().SetCooldown(0.0f);
                }
            }
        }
    }

    public void SpellPressed(int anIndex)
    {
        myActionButtons[anIndex].GetComponent<ActionKey>().SpellPressed();
    }

    public void SpellHeldDown(int anIndex)
    {
        myActionButtons[anIndex].GetComponent<ActionKey>().SpellHeldDown();
    }

    public void SpellReleased(int anIndex)
    {
        myActionButtons[anIndex].GetComponent<ActionKey>().SpellReleased();
    }

    public bool IsSpellCastOnFriends(int anIndex)
    {
        return mySpells[anIndex].GetComponent<Spell>().IsCastOnFriends();
    }

    public void ToggleSpellInfo()
    {
        myEventOnInfoToggle?.Invoke(gameObject);
        for (int index = 0; index < myActionButtons.Length; index++)
        {
            myActionButtons[index].GetComponent<ActionKey>().ToggleInfo();
        }
    }

    public bool IsSpellOnCooldown(int anIndex)
    {
        if (myCooldownTimers[anIndex] > 0.0f)
            return true;

        return false;
    }

    public GameObject GetAutoAttack()
    {
        return myAutoAttack;
    }

    public GameObject GetSpell(int anIndex)
    {
        return mySpells[anIndex];
    }

    public void SetSpellOnCooldown(int anIndex)
    {
        myCooldownTimers[anIndex] = mySpells[anIndex].GetComponent<Spell>().myCooldown;

        if (myActionButtons[anIndex] == null)
            return;

        myActionButtons[anIndex].GetComponent<ActionKey>().SetCooldown(myCooldownTimers[anIndex]);
    }

    public delegate void ActionClick(int anIndex, bool aIsPressed);
    public void SetupSpellHud(ActionClick anActionClickFunction, Transform aUIParent)
    {
        FindActionBar(aUIParent);

        for (int index = 0; index < myActionButtons.Length; index++)
        {
            int tempIndex = index;

            string name = "ActionButton" + (tempIndex + 1).ToString();
            for (int childIndex = 0; childIndex < myActionBar.transform.childCount; childIndex++)
            {
                if (myActionBar.transform.GetChild(childIndex).name == name)
                {
                    myActionButtons[index] = myActionBar.transform.GetChild(childIndex).gameObject;
                    Spell spell = mySpells[index].GetComponent<Spell>();
                    myActionButtons[index].GetComponent<Image>().sprite = spell.mySpellIcon;
                    myActionButtons[index].GetComponent<ActionKey>().SetSpellInfo(spell.myQuickInfo);
                    break;
                }
            }

            myActionButtons[tempIndex].GetComponent<Button>().onClick.AddListener(delegate { anActionClickFunction(tempIndex, true); });
        }
    }
}
