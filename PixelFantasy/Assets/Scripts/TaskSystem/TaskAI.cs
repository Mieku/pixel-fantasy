using System;
using System.Collections.Generic;
using System.Linq;
using Buildings;
using Buildings.Building_Types;
using Characters;
using Items;
using Managers;
using Mono.CSharp;
using ScriptableObjects;
using UnityEngine;
using UnityEngine.Analytics;
using UnityEngine.Serialization;

namespace TaskSystem
{
    public class TaskAI : MonoBehaviour
    {
        [FormerlySerializedAs("_unit")] [SerializeField] private Kinling _kinling;

        private List<TaskAction> _taskActions;
        private State _state;
        private float _waitingTimer;
        private float _idleTimer;
        private TaskAction _curTaskAction;
        private Item _heldItem;
        private Queue<Task> _queuedTasks = new Queue<Task>();
        
        private const float WAIT_TIMER_MAX = 0.2f; // 200ms
        private const float IDLE_TIME = 10f;

        public Kinling Kinling => _kinling;
        public TaskAction CurrentAction => _curTaskAction;

        public enum State
        {
            WaitingForNextTask,
            GettingTool,
            ExecutingTask,
            ForcedTask,
            Idling,
        }
        
        private void Awake()
        {
            FindTaskActions();
        }
        
        private void FindTaskActions()
        {
            _taskActions = GetComponentsInChildren<TaskAction>().ToList();
            foreach (var taskAction in _taskActions)
            {
                taskAction.AssignOwner(this);
            }
        }

        private void Update()
        {
            switch (_state)
            {
                case State.WaitingForNextTask:
                    // Waiting to request the next task
                    _waitingTimer -= TimeManager.Instance.DeltaTime;
                    if (_waitingTimer <= 0)
                    {
                        _waitingTimer = WAIT_TIMER_MAX;
                        RequestNextTask();
                    }
                    break;
                case State.GettingTool:
                    break;
                case State.ExecutingTask:
                    _curTaskAction?.DoAction();
                    break;
                case State.ForcedTask:
                    _curTaskAction?.DoAction();
                    break;
                case State.Idling:
                    _idleTimer += TimeManager.Instance.DeltaTime;
                    if (_idleTimer > IDLE_TIME)
                    {
                        _idleTimer = 0;
                        RequestNextTask();
                    }
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public TaskAction FindTaskActionFor(Task task)
        {
            return _taskActions.Find(taskAction => taskAction.TaskId == task.TaskId);
        }

        public void CurrentTaskDone()
        {
            _curTaskAction = null;
            _state = State.WaitingForNextTask;
        }

        public ScheduleOption GetCurrentScheduleOption()
        {
            int currentHour = EnvironmentManager.Instance.GameTime.GetCurrentHour24();
            return _kinling.Schedule.GetHour(currentHour);
        }

        private void RequestNextTask()
        {
            var currentSchedule = GetCurrentScheduleOption();
            switch (currentSchedule)
            {
                case ScheduleOption.Sleep:
                    ForceTask("Go To Sleep");
                    break;
                case ScheduleOption.Work:
                    RequestNextJobTask();
                    break;
                case ScheduleOption.Recreation:
                    RequestNextRecreationTask();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
        
        private void RequestNextJobTask()
        {
            if (_state == State.ForcedTask) return;
            
            Task task = null;
            
            // Check if they have a queued task
            if (_queuedTasks.Count > 0)
            {
                task = _queuedTasks.Dequeue();
                if (AttemptStartTask(task, false)) return;
                else
                {
                    _queuedTasks.Enqueue(task);
                    task = null;
                }
            }
            
            if (task == null)
            {
                CheckPersonal();
            }
            
            if (task == null)
            {
                // Check if they are missing any required Equipment
                task = CheckEquipment();
                if (AttemptStartTask(task, false)) return;
                else task = null;
            }

            // First Check Their Assigned Workplace
            if (_kinling.AssignedWorkplace != null && task == null)
            {
                task = _kinling.AssignedWorkplace.GetBuildingTask();
                if (AttemptStartTask(task, true)) return;
                else task = null;
            }
            
            if (task == null)
            {
                task = TaskManager.Instance.RequestTask(_kinling);
                if (task != null)
                {
                    if (AttemptStartTask(task, true)) return;
                    else task = null;
                }
            }
            
            if (task == null)
            {
                IdleAtWork();
                return;
            }

            // Queue the subtasks
            if (task.SubTasks.Count > 0)
            {
                Queue<Task> newSubtasks = new Queue<Task>(task.SubTasks);
                task.SubTasks.Clear();
                
                foreach (var queuedTask in _queuedTasks)
                {
                    newSubtasks.Enqueue(queuedTask);
                }

                _queuedTasks = newSubtasks;
                
                _state = State.WaitingForNextTask;
                return;
            }
        }

        private void RequestNextRecreationTask()
        {
            if (_state == State.ForcedTask) return;
            
            Task task = null;
            
            // Check if they have a queued task
            if (_queuedTasks.Count > 0)
            {
                task = _queuedTasks.Dequeue();
                if (AttemptStartTask(task, false)) return;
                else
                {
                    _queuedTasks.Enqueue(task);
                    task = null;
                }
            }
            
            if (task == null)
            {
                CheckPersonal();
            }
            
            
            // New recreation tasks go here
            
            // Eat food
            if (task == null && _kinling.Needs.GetNeedValue(NeedType.Food) < 0.75f)
            {
                if (_kinling.AssignedHome != null)
                {
                    task = new Task("Eat Food", _kinling.AssignedHome, null, EToolType.None);
                    if (AttemptStartTask(task, false)) return;
                    else task = null;
                }
                else
                {
                    var eatery = BuildingsManager.Instance.GetClosestBuildingOfType<IEateryBuilding>(_kinling.transform.position);
                    if (eatery != null)
                    {
                        task = new Task("Eat Food", eatery, null, EToolType.None);
                        if (AttemptStartTask(task, false)) return;
                        else task = null;
                    }
                }
                
            }
            
            // Make sure there is 1 day's worth of food in home
            if (task == null && _kinling.AssignedHome != null)
            {
                var suggestedNutrition = _kinling.AssignedHome.SuggestedStoredNutrition;
                var curHouseholdNutrition = _kinling.AssignedHome.CurrentStoredNutrition;
                if (curHouseholdNutrition < suggestedNutrition)
                {
                    // Set up a task to pick up some food and store it at home
                    task = new Task("Store Food", _kinling.AssignedHome, null, EToolType.None);
                    if (AttemptStartTask(task, false)) return;
                    else task = null;
                }
            }
            
            // Go on dates
            
            // Have sex
            if (task == null && _kinling.Partner != null)
            {
                if (_kinling.Needs.CheckSexDrive() && _kinling.Partner.Needs.CheckSexDrive())
                {
                    task = new Task("Mate", _kinling.AssignedBed, null, EToolType.None);
                    if (AttemptStartTask(task, false)) return;
                    else task = null;
                }
            }
            
            // Do fun things
            
            // End of new recreation tasks
            
            
            if (task == null)
            {
                _state = State.WaitingForNextTask;
                return;
            }

            // Queue the subtasks
            if (task.SubTasks.Count > 0)
            {
                Queue<Task> newSubtasks = new Queue<Task>(task.SubTasks);
                task.SubTasks.Clear();
                
                foreach (var queuedTask in _queuedTasks)
                {
                    newSubtasks.Enqueue(queuedTask);
                }

                _queuedTasks = newSubtasks;
                
                _state = State.WaitingForNextTask;
                return;
            }
        }

        private bool AttemptStartTask(Task task, bool returnToQueueOnFail)
        {
            if (task == null)
            {
                return false;
            }
            
            var taskAction = FindTaskActionFor(task);
            if (!taskAction.CanDoTask(task))
            {
                if (returnToQueueOnFail)
                {
                    // If action can't be done, return it to the queue
                    TaskManager.Instance.AddTask(task);
                }
                
                return false;
            }
            else
            {
                _curTaskAction = taskAction;
                taskAction.InitAction(task);
                
                // Get tool if needed
                _state = State.GettingTool;
                taskAction.PickupRequiredTool(() =>
                    {
                        taskAction.PrepareAction(task);
                        _state = State.ExecutingTask;
                    }, 
                    () =>
                    {
                        if (returnToQueueOnFail)
                        {
                            // If action can't be done, return it to the queue
                            TaskManager.Instance.AddTask(task);
                        }
                        _state = State.WaitingForNextTask;
                    });

                return true;
            }
        }

        private Task CheckEquipment()
        {
            return _kinling.Equipment.CheckDesiredEquipment();
        }

        /// <summary>
        /// Check important personal requirements, for example look for a house if homeless. Invite Kinlings on dates... etc
        /// </summary>
        private void CheckPersonal()
        {
            // If homeless, find a home
            if (_kinling.AssignedHome == null)
            {
                // Does partner have home?
                if (_kinling.Partner != null && _kinling.Partner.AssignedHome != null)
                {
                    _kinling.Partner.AssignedHome.AssignPartner(_kinling);
                }
                else
                {
                    BuildingsManager.Instance.ClaimEmptyHome(_kinling);
                }
            }
        }
        
        public UnitActionDirection GetActionDirection(Vector3 targetPos)
        {
            return DetermineUnitActionDirection(targetPos, transform.position);
        }
        
        private UnitActionDirection DetermineUnitActionDirection(Vector3 workPos, Vector3 standPos)
        {
            const float threshold = 0.25f;

            if (standPos.y >= workPos.y + threshold)
            {
                return UnitActionDirection.Down;
            } 
            else if (standPos.y <= workPos.y - threshold)
            {
                return UnitActionDirection.Up;
            }
            else
            {
                return UnitActionDirection.Side;
            }
        }

        public bool IsPositionPossible(Vector2 pos)
        {
            return Kinling.KinlingAgent.IsDestinationPossible(pos);
        }
        
        public void HoldItem(Item item)
        {
            _heldItem = item;
            item.transform.SetParent(transform);
            item.transform.localPosition = Vector3.zero;
        }

        public Item HeldItem => _heldItem;

        public void DropCarriedItem()
        {
            if (_heldItem == null) return;

            _heldItem.transform.SetParent(Spawner.Instance.ItemsParent);
            _heldItem.IsAllowed = true;
            _heldItem.DropItem();
            _heldItem = null;
        }

        public void DepositHeldItemInStorage(Storage storage)
        {
            if (_heldItem == null) return;
            
            storage.DepositItems(_heldItem);
            _heldItem = null;
        }

        public void CancelTask(string taskID)
        {
            // Check queued tasks
            RemoveTaskFromQueue(taskID);
            if (_curTaskAction.TaskId == taskID)
            {
                _curTaskAction.OnTaskCancel();
            }
        }

        public void QueueTask(Task task)
        {
            _queuedTasks.Enqueue(task);
        }

        private void RemoveTaskFromQueue(string taskID)
        {
            Queue<Task> tempQueue = new Queue<Task>();

            while (_queuedTasks.Count > 0)
            {
                var item = _queuedTasks.Dequeue();
                if (item.TaskId != taskID)
                {
                    tempQueue.Enqueue(item);
                }
            }

            _queuedTasks = tempQueue;
        }

        public bool AssignCommandTask(Task task)
        {
            if(_curTaskAction != null)
                _curTaskAction.OnTaskCancel();

            bool success = AttemptStartTask(task, false);
            return success;
        }

        public TaskAction ForceTask(string taskID)
        {
            if(_curTaskAction != null)
                 _curTaskAction.OnTaskCancel();
            
            Task forcedTask = new Task(taskID, null, null, EToolType.None)
            {
                OnTaskComplete = OnForcedTaskComplete
            };

            var forcedTaskAction = FindTaskActionFor(forcedTask);
            if (forcedTaskAction == null) return null;
            if (!forcedTaskAction.CanDoTask(forcedTask))
            {
                _state = State.WaitingForNextTask;
                return null;
            }

            _curTaskAction = forcedTaskAction;
            forcedTaskAction.InitAction(forcedTask);
            forcedTaskAction.PrepareAction(forcedTask);
            _state = State.ForcedTask;
            
            return _curTaskAction;
        }

        private void OnForcedTaskComplete(Task forcedTask)
        {
            _state = State.WaitingForNextTask;
        }

        public bool IsActionPossible(string taskID)
        {
            Task forcedTask = new Task(taskID, null, null, EToolType.None)
            {
                OnTaskComplete = OnForcedTaskComplete
            };
            
            var forcedTaskAction = FindTaskActionFor(forcedTask);
            if (forcedTaskAction == null) return false;

            return forcedTaskAction.CanDoTask(forcedTask);
        }

        public bool HasToolTypeEquipped(EToolType toolType)
        {
            return _kinling.Equipment.HasToolTypeEquipped(toolType);
        }
        
        private void IdleAtWork()
        {
            _state = State.Idling;
            
            Building buildingToIdleIn = null;
            if (_kinling.AssignedWorkplace == null)
            {
                // Idle at town center
                buildingToIdleIn = BuildingsManager.Instance.GetClosestBuildingOfType<TownCenterBuilding>(_kinling.transform.position);
            }
            else
            {
                buildingToIdleIn = _kinling.AssignedWorkplace;
            }

            if (buildingToIdleIn == null)
            {
                buildingToIdleIn = _kinling.AssignedHome;
            }
            
            if (!_kinling.IsSeated || (_kinling.GetChair != null && _kinling.GetChair.ParentBuilding != buildingToIdleIn ))
            {
                Vector2 moveTarget;
                ChairFurniture chair = null;
                if (buildingToIdleIn != null)
                {
                    chair = buildingToIdleIn.FindAvailableChair();
                    if (chair != null)
                    {
                        var seat = chair.ClaimSeat(_kinling);
                        moveTarget = seat.Position;
                    }
                    else
                    {
                        moveTarget = buildingToIdleIn.GetRandomIndoorsPosition(_kinling);
                    }
                }
                else
                {
                    // Just wander
                    moveTarget = _kinling.KinlingAgent.PickLocationInRange(10.0f);
                }

                _kinling.KinlingAgent.SetMovePosition(moveTarget, () =>
                {
                    if (chair != null)
                    {
                        chair.EnterSeat(_kinling);
                    }
                });
            }
        }
    }
}

