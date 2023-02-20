using System;
using System.Collections;
using System.Collections.Generic;
using Actions;
using Characters.Interfaces;
using SGoap;
using SGoap.Services;
using UnityEngine;
using UnityEngine.AI;

public class KinlingAgent : Agent
{
    public KinlingAgentData Data;
    [SerializeField] private ProfessionData professionData;
    [SerializeField] private AssignedInteractableSensor _assignedInteractableSensor;
    
    private float waitingTimer;
    private const float WAIT_TIMER_MAX = .2f; // 200ms
    private State state;
    public GoalRequest CurrentRequest;
    private static GoalMaster goalMaster => GoalMaster.Instance;
    
    public enum State
    {
        WaitingForNextTask,
        ExecutingTask
    }
        
    private void Awake()
    {
        Initialize();
    }
    
    public void Initialize()
    {
        var animator = GetComponent<UnitAnimController>();
        var navMeshAgent = GetComponent<NavMeshAgent>();
        navMeshAgent.updateRotation = false;
        navMeshAgent.updateUpAxis = false;

        Data = new KinlingAgentData
        {
            Animator = animator,
            NavMeshAgent = navMeshAgent,
            KinlingAgent = this,
            // Inventory = new Inventory(),
            Cooldown = new CoolDown(),
        };

        var targetDependencies = GetComponentsInChildren<IDataBind<KinlingAgentData>>();
        foreach (var dependency in targetDependencies)
            dependency.Bind(Data);
    }

    public bool IsDestinationPossible(Vector2 position)
    {
        NavMeshPath path = new NavMeshPath();
        Data.NavMeshAgent.CalculatePath(position, path);
        return path.status == NavMeshPathStatus.PathComplete;
    }

    private void Update()
    {
        switch (state)
        {
            case State.WaitingForNextTask:
                // Waiting to request the next task
                waitingTimer -= Time.deltaTime;
                if (waitingTimer <= 0)
                {
                    waitingTimer = WAIT_TIMER_MAX;
                    RequestNextGoal();
                }
                break;
            case State.ExecutingTask:
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }
    
    private void RequestNextGoal()
    {
        GoalRequest request = null;
        foreach (var category in professionData.SortedPriorities)
        {
            request = goalMaster.GetNextGoalByCategory(category);
            if (request != null)
            {
                break;
            }
        }
        
        // TODO: Check if the goal is possible, if not return it

        CurrentRequest = request;
        if (request == null)
        {
            state = State.WaitingForNextTask;
            Data.Animator.SetUnitAction(UnitAction.Nothing, UnitActionDirection.Side);
        }
        else
        {
            Goals.Clear();
            Goals.Add(request.Goal);
            UpdateGoalOrderCache();
            state = State.ExecutingTask;
            
            var interactable = request.Requestor.GetComponent<Interactable>();
            if (interactable != null)
            {
                _assignedInteractableSensor.AssignInteractable(interactable);
            }
        }
    }

    public override void EvaluationGoal()
    {
        if (ActionQueue != null && ActionQueue.Count == 0)
        {
            if (CurrentGoal.Once)
            {
                CurrentRequest = null;
                state = State.WaitingForNextTask;
                States.Clear();
            }
        }
        base.EvaluationGoal();
    }

    public override void LateUpdate()
    {
        if (!Data.Cooldown.Active)
            base.LateUpdate();
    }
}
