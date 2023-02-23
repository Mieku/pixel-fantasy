using System;
using System.Collections;
using System.Collections.Generic;
using Actions;
using Characters.Interfaces;
using Gods;
using Items;
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
    private GoalRequest _currentRequest;
    private NavMeshAgent _navAgent;
    private UnitAnimController _unitAnim;
    private static GoalMaster goalMaster => GoalMaster.Instance;
    private Item _heldItem;
    
    public enum State
    {
        WaitingForNextTask,
        ExecutingTask
    }
        
    private void Awake()
    {
        GameEvents.OnGoalRequestCancelled += Event_OnGoalRequestCancelled;
        
        Initialize();
    }

    private void OnDestroy()
    {
        GameEvents.OnGoalRequestCancelled -= Event_OnGoalRequestCancelled;
    }

    public void Initialize()
    {
        _unitAnim = GetComponent<UnitAnimController>();
        _navAgent = GetComponent<NavMeshAgent>();
        _navAgent.updateRotation = false;
        _navAgent.updateUpAxis = false;

        Data = new KinlingAgentData
        {
            Animator = _unitAnim,
            NavMeshAgent = _navAgent,
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

        if (request != null)
        {
            _assignedInteractableSensor.AssignPayload(request.Payload);
            var interactable = request.Requestor.GetComponent<Interactable>();
            if (interactable != null)
            {
                _assignedInteractableSensor.AssignInteractable(interactable);
            }
            
            // TODO: Check if the goal is possible, if not return it
            var plan = DeterminePlan(request.Goal);
            if (plan == null)
            {
                // No Plan was determined
                GoalMaster.Instance.AddGoal(request);
                return;
            }
        }
        
        _currentRequest = request;
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
            
            // var interactable = request.Requestor.GetComponent<Interactable>();
            // if (interactable != null)
            // {
            //     _assignedInteractableSensor.AssignInteractable(interactable);
            // }
        }
    }

    public override void EvaluationGoal()
    {
        if (ActionQueue != null && ActionQueue.Count == 0)
        {
            if (CurrentGoal.Once)
            {
                _currentRequest = null;
                state = State.WaitingForNextTask;
                States.Clear();
            }
        }
        base.EvaluationGoal();
    }

    private void Event_OnGoalRequestCancelled(GoalRequest goalRequest)
    {
        if (_currentRequest != null && _currentRequest.IsEqual(goalRequest))
        {
            CancelCurrentGoal(goalRequest, false);
        }
    }

    private void CancelCurrentGoal(GoalRequest goalRequest, bool returnToQueue)
    {
        Goals.Remove(goalRequest.Goal);
        UpdateGoalOrderCache();
        CleanPlan();
        _currentRequest = null;
        _navAgent.SetDestination(transform.position);
        _unitAnim.SetUnitAction(UnitAction.Nothing, UnitActionDirection.Side);
        
        DropCarriedItem();

        if (returnToQueue)
        {
            GoalMaster.Instance.AddGoal(goalRequest);
        }
        
        state = State.WaitingForNextTask;
    }

    public void HoldItem(Item item)
    {
        _heldItem = item;
        item.transform.SetParent(transform);
        item.transform.localPosition = Vector3.zero;
    }

    public void DropCarriedItem()
    {
        if (_heldItem == null) return;
        
        Spawner.Instance.SpawnItem(_heldItem.GetItemData(), transform.position, true);
        Destroy(_heldItem.gameObject);
        _heldItem = null;
    }

    public override void LateUpdate()
    {
        if (!Data.Cooldown.Active)
            base.LateUpdate();
    }

    public AgentStateData GetSaveData()
    {
        
        return new AgentStateData
        {
            ProfessionData = professionData,
        };
    }

    public void SetLoadData(AgentStateData agentData)
    {
        professionData = agentData.ProfessionData;
    }
    
    public struct AgentStateData
    {
        public ProfessionData ProfessionData;
    }
}
