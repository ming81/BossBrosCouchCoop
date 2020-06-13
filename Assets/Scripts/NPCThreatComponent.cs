﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BehaviorDesigner.Runtime;

public class NPCThreatComponent : MonoBehaviour
{
    private TargetingComponent myTargetingComponent;
    private NPCCastingComponent myCastingComponent;
    private BehaviorTree myBehaviorTree;
    private NPCComponent myNPCComponent;
    private Health myHealth;
    public List<GameObject> Players { get; set; } = new List<GameObject>();
    public List<int> myThreatValues = new List<int>();

    private int myTargetIndex;

    private float myTauntDuration;
    private bool myIsTaunted;

    private Subscriber mySubscriber;

    public delegate void OnTaunted();
    public event OnTaunted EventOnTaunted;

    private void Awake()
    {
        myTargetingComponent = GetComponent<TargetingComponent>();
        myCastingComponent = GetComponent<NPCCastingComponent>();
        myBehaviorTree = GetComponent<BehaviorTree>();
        myNPCComponent = GetComponent<NPCComponent>();
        myHealth = GetComponent<Health>();
        myTargetIndex = -1;
    }

    void Start()
    {
        myHealth.EventOnThreatGenerated += AddThreat;
        myHealth.EventOnHealthZero += OnDeath;

        Subscribe();
    }

    private void OnDisable()
    {
        Unsubscribe();
    }

    void Subscribe()
    {
        mySubscriber = new Subscriber();
        mySubscriber.EventOnReceivedMessage += ReceiveMessage;
        PostMaster.Instance.RegisterSubscriber(ref mySubscriber, MessageCategory.SpellSpawned);
        PostMaster.Instance.RegisterSubscriber(ref mySubscriber, MessageCategory.PlayerDied);
    }

    void Unsubscribe()
    {
        mySubscriber.EventOnReceivedMessage -= ReceiveMessage;
        PostMaster.Instance.UnregisterSubscriber(ref mySubscriber, MessageCategory.SpellSpawned);
        PostMaster.Instance.UnregisterSubscriber(ref mySubscriber, MessageCategory.PlayerDied);
    }

    void Update()
    {
        if (myHealth.IsDead())
            return;

        if (myNPCComponent.State != NPCComponent.CombatState.Combat)
            return;

        if (myIsTaunted)
        {
            myTauntDuration -= Time.deltaTime;
            if (myTauntDuration <= 0.0f)
                myIsTaunted = false;
        }

        if (myCastingComponent.IsCasting())
            return;

        DetermineTarget();
    }

    public void DetermineTarget()
    {
        int target = GetHighestAggro();

        if (myTargetIndex != target)
        {
            SetTarget(target);
        }
    }

    private int GetHighestAggro()
    {
        if (myIsTaunted)
            return myTargetIndex;

        int highestAggro = 0;
        if (myThreatValues.Count == 1)
            return highestAggro;

        for (int index = 1; index < myThreatValues.Count; index++)
        {
            if (myThreatValues[index] > myThreatValues[highestAggro] && !Players[index].GetComponent<Health>().IsDead())
                highestAggro = index;
        }

        return highestAggro;
    }

    public void PlayerSpotted(GameObject aGameObject)
    {
        GetComponent<NPCComponent>().SetState(NPCComponent.CombatState.Combat);
        AddThreat(10, aGameObject.GetInstanceID(), true);
        SetTarget(GetHighestAggro());
    }

    public void SetTaunt(int aTaunterID, float aDuration)
    {
        myIsTaunted = true;
        myTauntDuration = aDuration;
        for (int index = 0; index < Players.Count; index++)
        {
            if (Players[index].GetInstanceID() == aTaunterID)
            {
                myTargetIndex = index;
                SetTarget(myTargetIndex);
                EventOnTaunted?.Invoke();
                AddThreat(2000, aTaunterID, true);
                break;
            }
        }
    }

    private void SetTarget(int aTargetIndex)
    {
        myTargetingComponent.SetTarget(Players[aTargetIndex]);
        myTargetIndex = aTargetIndex;
    }

    public void AddPlayer(GameObject aPlayer)
    {
        Players.Add(aPlayer);
        myThreatValues.Add(0);
    }

    private bool AreAllPlayersDead()
    {
        for (int index = 0; index < Players.Count; index++)
        {
            if (!Players[index].GetComponent<Health>().IsDead())
                return false;
        }

        return true;
    }

    public void RemovePlayer(int anIndex)
    {
        myThreatValues.RemoveAt(anIndex);
        Players.RemoveAt(anIndex);

        if (Players.Count == 0)
            myNPCComponent.SetState(NPCComponent.CombatState.Disengage);
    }

    private void DropTarget()
    {
        myTargetIndex = -1;
        GetComponent<NPCMovementComponent>().Stop();

        myTargetingComponent.SetTarget(null);
    }

    private void AddThreat(int aThreatValue, int anID, bool anIgnoreCombatState)
    {
        if (!anIgnoreCombatState && myNPCComponent.State != NPCComponent.CombatState.Combat)
            return;

        for (int index = 0; index < Players.Count; index++)
        {
            if (Players[index].GetInstanceID() == anID)
            {
                myThreatValues[index] += aThreatValue;
                break;
            }
        }
    }

    private void OnDeath()
    {
        myThreatValues.ForEach(item => { item = 0; });
    }

    private void ReceiveMessage(Message anAiMessage)
    {
        switch (anAiMessage.Type)
        {
            case MessageCategory.SpellSpawned:
                {
                    int id = (int)anAiMessage.Data.myVector2.x;
                    int value = (int)anAiMessage.Data.myVector2.y;

                    AddThreat(value, id, false);
                }
                break;
            case MessageCategory.PlayerDied:
                {
                    BehaviorTree behaviorTree = GetComponent<BehaviorTree>();
                    if (behaviorTree)
                        behaviorTree.SendEvent("PlayerDied");

                    int id = anAiMessage.Data.myInt;
                    for (int index = 0; index < Players.Count; index++)
                    {
                        if (Players[index].GetInstanceID() == id)
                        {
                            if (index == myTargetIndex)
                                DropTarget();

                            if (AreAllPlayersDead())
                                myNPCComponent.SetState(NPCComponent.CombatState.Disengage);

                            //RemovePlayer(index);
                            break;
                        }
                    }
                }
                break;
            default:
                break;
        }
    }
}
