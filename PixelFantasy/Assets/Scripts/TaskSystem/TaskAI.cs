using System;
using System.Collections.Generic;
using System.Linq;
using Characters;
using Data.Item;
using Handlers;
using Items;
using Managers;
using ScriptableObjects;
using Systems.Stats.Scripts;
using UnityEngine;

namespace TaskSystem
{
    public class TaskAI : MonoBehaviour
    {
        [SerializeField] private Kinling _kinling;
        
        private List<TaskAction> _taskActions;
        private Queue<Task> _queuedTasks = new Queue<Task>();
        
        private const float WAIT_TIMER_MAX = 0.2f; // 200ms
        private const float IDLE_TIME = 10f;

        public Kinling Kinling => _kinling;
        public TaskAction CurrentAction => _data.CurrentTaskAction;

        private KinlingData _data => _kinling.RuntimeData;

        public enum TaskAIState
        {
            WaitingForNextTask,
            GettingTool,
            ExecutingTask,
            ForcedTask,
            Idling,
        }

        public string CurrentStateName
        {
            get
            {
                switch (_data.TaskAIState)
                {
                    case TaskAIState.WaitingForNextTask:
                        return "Idle";
                    case TaskAIState.GettingTool:
                        return "Equipping";
                    case TaskAIState.ForcedTask:
                    case TaskAIState.ExecutingTask:
                        return $"{_data.CurrentTaskAction.TaskId}";
                    case TaskAIState.Idling:
                        return "Idle";
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
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
            switch (_data.TaskAIState)
            {
                case TaskAIState.WaitingForNextTask:
                    // Waiting to request the next task
                    _data.WaitingTimer -= TimeManager.Instance.DeltaTime;
                    if (_data.WaitingTimer <= 0)
                    {
                        _data.WaitingTimer = WAIT_TIMER_MAX;
                        RequestNextTask();
                    }
                    break;
                case TaskAIState.GettingTool:
                    break;
                case TaskAIState.ExecutingTask:
                    _data.CurrentTaskAction?.DoAction();
                    break;
                case TaskAIState.ForcedTask:
                    _data.CurrentTaskAction?.DoAction();
                    break;
                case TaskAIState.Idling:
                    _data.IdleTimer += TimeManager.Instance.DeltaTime;
                    if (_data.IdleTimer > IDLE_TIME)
                    {
                        _data.IdleTimer = 0;
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
            _data.CurrentTaskAction = null;
            _data.TaskAIState = TaskAIState.WaitingForNextTask;
        }

        public ScheduleOption GetCurrentScheduleOption()
        {
            int currentHour = EnvironmentManager.Instance.GameTime.GetCurrentHour24();
            return _kinling.RuntimeData.Schedule.GetHour(currentHour);
        }

        private void RequestNextTask()
        {
            var currentSchedule = GetCurrentScheduleOption();
            switch (currentSchedule)
            {
                case ScheduleOption.Sleep:
                    RequestSleepTask();
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

        private void RequestSleepTask()
        {
            // If the kinling lacks a bed, claim one if available
            if (_kinling.RuntimeData.AssignedBed == null)
            {
                var bed = FurnitureManager.Instance.FindClosestUnclaimedBed(_kinling);
                bed.AssignKinling(_kinling);
            }
            
            ForceTask("Go To Sleep");
        }
        
        private void RequestNextJobTask()
        {
            if (_data.TaskAIState == TaskAIState.ForcedTask) return;
            
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
            
            // if (task == null)
            // {
            //     // Check if they are missing any required Equipment
            //     task = CheckEquipment();
            //     if (AttemptStartTask(task, false)) return;
            //     else task = null;
            // }
            
            if (task == null)
            {
                task = TaskManager.Instance.RequestTask(_kinling, _data.TaskPriorities.SortedPriorities());
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
        }

        private void RequestNextRecreationTask()
        {
            if (_data.TaskAIState == TaskAIState.ForcedTask) return;
            
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
                // TODO: Look for a table near the food, if none, choose a spot to stand and eat
                
                // if (_kinling.AssignedHome != null)
                // {
                //     task = new Task("Eat Food", ETaskType.Personal, _kinling.AssignedHome, EToolType.None);
                //     if (AttemptStartTask(task, false)) return;
                //     else task = null;
                // }
                // else
                // {
                //     var eatery = BuildingsManager.Instance.GetClosestBuildingOfType<IEateryBuilding>(_kinling.transform.position);
                //     if (eatery != null)
                //     {
                //         task = new Task("Eat Food", ETaskType.Personal, eatery, EToolType.None);
                //         if (AttemptStartTask(task, false)) return;
                //         else task = null;
                //     }
                // }
                
            }
            
            // Go on dates
            
            // Have sex
            if (task == null && _kinling.RuntimeData.Partner != null)
            {
                if (_kinling.Needs.CheckSexDrive() && _kinling.RuntimeData.Partner.Kinling.Needs.CheckSexDrive())
                {
                    task = new Task("Mate", ETaskType.Personal, _kinling.RuntimeData.AssignedBed.LinkedFurniture, EToolType.None);
                    if (AttemptStartTask(task, false)) return;
                    else task = null;
                }
            }
            
            // Do fun things
            
            // End of new recreation tasks

            if (task == null)
            {
                _data.TaskAIState = TaskAIState.WaitingForNextTask;
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
                _data.CurrentTaskAction = taskAction;
                taskAction.InitAction(task, _kinling);
                
                // Get tool if needed
                _data.TaskAIState = TaskAIState.GettingTool;
                taskAction.PickupRequiredTool(() =>
                    {
                        taskAction.PrepareAction(task);
                        _data.TaskAIState = TaskAIState.ExecutingTask;
                    }, 
                    () =>
                    {
                        if (returnToQueueOnFail)
                        {
                            // If action can't be done, return it to the queue
                            TaskManager.Instance.AddTask(task);
                        }
                        _data.TaskAIState = TaskAIState.WaitingForNextTask;
                    });

                return true;
            }
        }

        // private Task CheckEquipment()
        // {
        //     return _kinling.Equipment.CheckDesiredEquipment();
        // }

        /// <summary>
        /// Check important personal requirements, for example look for a house if homeless. Invite Kinlings on dates... etc
        /// </summary>
        private void CheckPersonal()
        {
            // // If homeless, find a home
            // if (_kinling.AssignedHome == null)
            // {
            //     // Does partner have home?
            //     if (_kinling.Partner != null && _kinling.Partner.AssignedHome != null)
            //     {
            //         _kinling.Partner.AssignedHome.AssignPartner(_kinling);
            //     }
            //     else
            //     {
            //         BuildingsManager.Instance.ClaimEmptyHome(_kinling);
            //     }
            // }
            //
            // // Auto Assign to a building if missing a workplace and have a job
            // if (_kinling.AssignedWorkplace == null & _kinling.Job != null)
            // {
            //     BuildingsManager.Instance.ClaimUnfilledWorkplace(_kinling);
            // }
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
            _data.HeldItem = item;
            item.ItemPickedUp(_kinling);
            item.transform.SetParent(transform);
            item.transform.localPosition = Vector3.zero;
        }
        
        public Item DropCarriedItem(bool allowHauling)
        {
            if (_data.HeldItem == null) return null;

            _data.HeldItem.transform.SetParent(ParentsManager.Instance.ItemsParent);
            _data.HeldItem.IsAllowed = allowHauling;
            _data.HeldItem.ItemDropped();
            var item = _data.HeldItem;
            _data.HeldItem = null;
            return item;
        }

        public void DepositHeldItemInStorage(IStorage storage)
        {
            if (_data.HeldItem == null) return;
            
            storage.DepositItems(_data.HeldItem);
            _data.HeldItem = null;
        }

        public void CancelTask(string taskID)
        {
            // Check queued tasks
            RemoveTaskFromQueue(taskID);
            if (_data.CurrentTaskAction.TaskId == taskID)
            {
                _data.CurrentTaskAction.OnTaskCancel();
            }
        }

        public void CancelCurrentTask()
        {
            if(_data.CurrentTaskAction == null) return;

            var taskID = _data.CurrentTaskAction.TaskId;
            RemoveTaskFromQueue(taskID);
            _data.CurrentTaskAction.OnTaskCancel();
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
            if(_data.CurrentTaskAction != null)
                _data.CurrentTaskAction.OnTaskCancel();

            bool success = AttemptStartTask(task, false);
            return success;
        }

        public TaskAction ForceTask(string taskID)
        {
            if(_data.CurrentTaskAction != null)
                _data.CurrentTaskAction.OnTaskCancel();
            
            Task forcedTask = new Task(taskID, ETaskType.Personal, null, EToolType.None)
            {
                OnTaskComplete = OnForcedTaskComplete
            };

            var forcedTaskAction = FindTaskActionFor(forcedTask);
            if (forcedTaskAction == null) return null;
            if (!forcedTaskAction.CanDoTask(forcedTask))
            {
                _data.TaskAIState = TaskAIState.WaitingForNextTask;
                return null;
            }

            _data.CurrentTaskAction = forcedTaskAction;
            forcedTaskAction.InitAction(forcedTask, _kinling);
            forcedTaskAction.PrepareAction(forcedTask);
            _data.TaskAIState = TaskAIState.ForcedTask;
            
            return _data.CurrentTaskAction;
        }

        private void OnForcedTaskComplete(Task forcedTask)
        {
            _data.TaskAIState = TaskAIState.WaitingForNextTask;
        }

        public bool IsActionPossible(string taskID)
        {
            Task forcedTask = new Task(taskID, ETaskType.Personal, null, EToolType.None)
            {
                OnTaskComplete = OnForcedTaskComplete
            };
            
            var forcedTaskAction = FindTaskActionFor(forcedTask);
            if (forcedTaskAction == null) return false;

            return forcedTaskAction.CanDoTask(forcedTask);
        }

        public bool HasToolTypeEquipped(EToolType toolType)
        {
            // Temp
            return true;
            
            // return _kinling.Equipment.HasToolTypeEquipped(toolType);
        }
        
        private void IdleAtWork()
        {
            _data.TaskAIState = TaskAIState.Idling;
            
            // Building buildingToIdleIn = null;
            // if (_kinling.AssignedWorkplace == null)
            // {
            //     // Idle at town center
            //     buildingToIdleIn = BuildingsManager.Instance.GetClosestBuildingOfType<TownCenterBuilding>(_kinling.transform.position);
            // }
            // else
            // {
            //     buildingToIdleIn = _kinling.AssignedWorkplace;
            // }
            //
            // if (buildingToIdleIn == null)
            // {
            //     buildingToIdleIn = _kinling.AssignedHome;
            // }
            
            if (!_kinling.IsSeated) //|| (_kinling.GetChair != null && _kinling.GetChair.ParentBuilding != buildingToIdleIn ))
            {
                // Vector2 moveTarget;
                // ChairFurniture chair = null;
                // if (buildingToIdleIn != null)
                // {
                //     chair = buildingToIdleIn.FindAvailableChair();
                //     if (chair != null)
                //     {
                //         var seat = chair.ClaimSeat(_kinling);
                //         moveTarget = seat.Position;
                //     }
                //     else
                //     {
                //         moveTarget = buildingToIdleIn.GetRandomIndoorsPosition(_kinling);
                //     }
                // }
                // else
                // {
                //     // Just wander
                //     moveTarget = _kinling.KinlingAgent.PickLocationInRange(10.0f);
                // }
                //
                // _kinling.KinlingAgent.SetMovePosition(moveTarget, () =>
                // {
                //     if (chair != null)
                //     {
                //         chair.EnterSeat(_kinling);
                //     }
                // });
                
                
                var moveTarget = _kinling.KinlingAgent.PickLocationInRange(10.0f);
                _kinling.KinlingAgent.SetMovePosition(moveTarget, () =>
                {
                    
                });
            }
        }
    }
}

