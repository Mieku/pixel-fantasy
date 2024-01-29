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

namespace TaskSystem
{
    public class TaskAI : MonoBehaviour
    {
        [SerializeField] private Unit _unit;

        private List<TaskAction> _taskActions;
        private State _state;
        private float _waitingTimer;
        private float _idleTimer;
        private TaskAction _curTaskAction;
        private Item _heldItem;
        private Queue<Task> _queuedTasks = new Queue<Task>();
        
        private const float WAIT_TIMER_MAX = 0.2f; // 200ms
        private const float IDLE_TIME = 10f;

        public Unit Unit => _unit;

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
            return _unit.Schedule.GetHour(currentHour);
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
            if (_unit.AssignedWorkplace != null && task == null)
            {
                task = _unit.AssignedWorkplace.GetBuildingTask();
                if (AttemptStartTask(task, true)) return;
                else task = null;
            }
            
            if (task == null)
            {
                task = TaskManager.Instance.RequestTask(_unit);
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
            if (task == null && _unit.Needs.GetNeedValue(NeedType.Food) < 0.75f)
            {
                if (_unit.AssignedHome != null)
                {
                    task = new Task("Eat Food", _unit.AssignedHome, null, EToolType.None);
                    if (AttemptStartTask(task, false)) return;
                    else task = null;
                }
                else
                {
                    var eatery = BuildingsManager.Instance.GetClosestBuildingOfType<IEateryBuilding>(_unit.transform.position);
                    if (eatery != null)
                    {
                        task = new Task("Eat Food", eatery, null, EToolType.None);
                        if (AttemptStartTask(task, false)) return;
                        else task = null;
                    }
                }
                
            }
            
            // Make sure there is 1 day's worth of food in home
            if (task == null && _unit.AssignedHome != null)
            {
                var suggestedNutrition = _unit.AssignedHome.SuggestedStoredNutrition;
                var curHouseholdNutrition = _unit.AssignedHome.CurrentStoredNutrition;
                if (curHouseholdNutrition < suggestedNutrition)
                {
                    // Set up a task to pick up some food and store it at home
                    task = new Task("Store Food", _unit.AssignedHome, null, EToolType.None);
                    if (AttemptStartTask(task, false)) return;
                    else task = null;
                }
            }
            
            // Go on dates
            
            // Have sex
            if (task == null && _unit.Partner != null)
            {
                if (_unit.Needs.CheckSexDrive() && _unit.Partner.Needs.CheckSexDrive())
                {
                    task = new Task("Mate", _unit.AssignedBed, null, EToolType.None);
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
        
        private void SetupTaskAction(Task task)
        {
            // Find the task's equivalent action
            var taskAction = FindTaskActionFor(task);
            if (taskAction == null)
            {
                // If no action is available, return it to the queue
                TaskManager.Instance.AddTask(task);
                _state = State.WaitingForNextTask;
                return;
            }

            if (!taskAction.CanDoTask(task))
            {
                // If action can't be done, return it to the queue
                TaskManager.Instance.AddTask(task);
                _state = State.WaitingForNextTask;
                return;
            }

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
                // If action can't be done, return it to the queue
                TaskManager.Instance.AddTask(task);
                _state = State.WaitingForNextTask;
                return;
            });
        }

        private Task CheckEquipment()
        {
            return _unit.Equipment.CheckDesiredEquipment();
        }

        /// <summary>
        /// Check important personal requirements, for example look for a house if homeless. Invite Kinlings on dates... etc
        /// </summary>
        private void CheckPersonal()
        {
            // If homeless, find a home
            if (_unit.AssignedHome == null)
            {
                // Does partner have home?
                if (_unit.Partner != null && _unit.Partner.AssignedHome != null)
                {
                    _unit.Partner.AssignedHome.AssignPartner(_unit);
                }
                else
                {
                    BuildingsManager.Instance.ClaimEmptyHome(_unit);
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
            return Unit.UnitAgent.IsDestinationPossible(pos);
        }
        
        public Vector2? GetAdjacentPosition(Vector2 workPosition, float distanceAway = 1f)
        {
            Vector2 unitPos = transform.position;

            var angle = Helper.CalculateAngle(workPosition, unitPos);
            var angle2 = ClampAngleTo360(angle - 90);
            var angle3 = ClampAngleTo360(angle + 90);
            var angle4 = ClampAngleTo360(angle + 180);

            Vector2 suggestedPos = ConvertAngleToPosition(angle, workPosition, distanceAway);
            if (Unit.UnitAgent.IsDestinationPossible(suggestedPos))
            {
                return suggestedPos;
            }
        
            suggestedPos = ConvertAngleToPosition(angle2, workPosition, distanceAway);
            if (Unit.UnitAgent.IsDestinationPossible(suggestedPos))
            {
                return suggestedPos;
            }
        
            suggestedPos = ConvertAngleToPosition(angle3, workPosition, distanceAway);
            if (Unit.UnitAgent.IsDestinationPossible(suggestedPos))
            {
                return suggestedPos;
            }
        
            suggestedPos = ConvertAngleToPosition(angle4, workPosition, distanceAway);
            if (Unit.UnitAgent.IsDestinationPossible(suggestedPos))
            {
                return suggestedPos;
            }

            return null;
        }
        
        private float ClampAngleTo360(float angle)
        {
            if (angle < 0)
            {
                angle += 360;
            }
            else if (angle >= 360)
            {
                angle -= 360;
            }

            return angle;
        }
    
        public Vector2 ConvertAngleToPosition(float angle, Vector2 startPos, float distance)
        {
            Vector2 result;
            
            // Left
            if (angle is >= 45 and < 135)
            {
                result = new Vector2(startPos.x - distance, startPos.y);
            } 
            else if (angle is >= 135 and < 225) // Down
            {
                result = new Vector2(startPos.x, startPos.y - distance);
            }
            else if (angle is >= 225 and < 315) // Right
            {
                result = new Vector2(startPos.x + distance, startPos.y);
            }
            else // Up
            {
                result = new Vector2(startPos.x, startPos.y + distance);
            }

            return result;
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
            return _unit.Equipment.HasToolTypeEquipped(toolType);
        }
        
        private void IdleAtWork()
        {
            _state = State.Idling;
            
            Building buildingToIdleIn = null;
            if (_unit.AssignedWorkplace == null)
            {
                // Idle at town center
                buildingToIdleIn = BuildingsManager.Instance.GetClosestBuildingOfType<TownCenterBuilding>(_unit.transform.position);
            }
            else
            {
                buildingToIdleIn = _unit.AssignedWorkplace;
            }

            if (buildingToIdleIn == null)
            {
                buildingToIdleIn = _unit.AssignedHome;
            }
            
            if (!_unit.IsSeated || (_unit.GetChair != null && _unit.GetChair.ParentBuilding != buildingToIdleIn ))
            {
                Vector2 moveTarget;
                ChairFurniture chair = null;
                if (buildingToIdleIn != null)
                {
                    chair = buildingToIdleIn.FindAvailableChair();
                    if (chair != null)
                    {
                        var seat = chair.ClaimSeat(_unit);
                        moveTarget = seat.Position;
                    }
                    else
                    {
                        moveTarget = buildingToIdleIn.GetRandomIndoorsPosition(_unit);
                    }
                }
                else
                {
                    // Just wander
                    moveTarget = _unit.UnitAgent.PickLocationInRange(10.0f);
                }

                _unit.UnitAgent.SetMovePosition(moveTarget, () =>
                {
                    if (chair != null)
                    {
                        chair.EnterSeat(_unit);
                    }
                });
            }
        }
    }
}

