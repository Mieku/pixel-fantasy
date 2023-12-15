using System;
using System.Collections.Generic;
using System.Linq;
using Characters;
using Items;
using Managers;
using ScriptableObjects;
using Systems.SmartObjects.Scripts;
using Systems.Stats.Scripts;
using UnityEngine;

namespace TaskSystem
{
    public class TaskAI : MonoBehaviour
    {
        [SerializeField] private Unit _unit;
        [SerializeField] private NeedsAI _needsAI;

        private List<TaskAction> _taskActions;
        private State _state;
        private float _waitingTimer;
        private TaskAction _curTaskAction;
        private Item _heldItem;
        private Queue<Task> _queuedTasks = new Queue<Task>();
        
        private const float WAIT_TIMER_MAX = 0.2f; // 200ms

        public Unit Unit => _unit;

        public enum State
        {
            WaitingForNextTask,
            GettingTool,
            ExecutingTask,
            ExecutingInteraction,
            ForcedTask,
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
                    _waitingTimer -= Time.deltaTime;
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
                case State.ExecutingInteraction:
                    break;
                case State.ForcedTask:
                    _curTaskAction?.DoAction();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private TaskAction FindTaskActionFor(Task task)
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
            return _unit.GetUnitState().Schedule.GetHour(currentHour);
        }

        private void RequestNextTask()
        {
            var currentSchedule = GetCurrentScheduleOption();
            switch (currentSchedule)
            {
                case ScheduleOption.Sleep:
                    var energyStat = Librarian.Instance.GetStat("Energy");
                    if (_needsAI.GetStatValue(energyStat) <= 0.70f)
                    {
                        DoInteraction(Librarian.Instance.GetStat("Energy"));
                    }
                    else
                    {
                        DoInteraction();
                    }
                    break;
                case ScheduleOption.Work:
                    RequestNextJobTask();
                    break;
                case ScheduleOption.Recreation:
                    DoInteraction();
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
            }
            
            if (task == null && _state != State.ExecutingInteraction)
            {
                // Check if any needs are critical
                // if (_needsAI.AreAnyNeedsCritical(out var criticalStat))
                // {
                //     DoInteraction(criticalStat);
                // }
            }
            
            if (task == null && _state != State.ExecutingInteraction)
            {
                // Check if they are missing any required Equipment
                task = CheckEquipment();
            }

            // First Check Their Assigned Workplace
            if (_unit.GetUnitState().AssignedWorkplace != null && task == null && _state != State.ExecutingInteraction)
            {
                task = _unit.GetUnitState().AssignedWorkplace.GetBuildingTask();
            }
            
            if (task == null && _state != State.ExecutingInteraction)
            {
                task = TaskManager.Instance.GetTask(_unit.GetUnitState().CurrentJob);
                if (task != null)
                {
                    var taskAction = FindTaskActionFor(task);
                    if (!taskAction.CanDoTask(task))
                    {
                        TaskManager.Instance.AddTask(task);
                        task = null;
                    }
                }
            }

            // if (task == null && _state != State.ExecutingInteraction)
            // {
            //     var sortedPriorities = Priorities.SortedPriorities;
            //     foreach (var sortedPriority in sortedPriorities)
            //     {
            //         task = TaskManager.Instance.GetNextTaskByType(sortedPriority.TaskType);
            //         if (task != null)
            //         {
            //             var taskAction = FindTaskActionFor(task);
            //             if (taskAction.CanDoTask(task))
            //             {
            //                 break;
            //             }
            //             else
            //             {
            //                 TaskManager.Instance.AddTask(task);
            //                 task = null;
            //             }
            //         }
            //     }
            // }

            // if (task == null && _state != State.ExecutingInteraction)
            // {
            //     // Fulfill personal needs if nothing to do
            //     DoInteraction();
            // }

            if (task == null)
            {
                if (_state != State.ExecutingInteraction)
                {
                    _state = State.WaitingForNextTask;
                }
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

            SetupTaskAction(task);
        }

        private void DoInteraction(AIStat focusStat = null)
        {
            if (_needsAI.PickBestInteraction(OnInteractionComplete, focusStat))
            {
                _state = State.ExecutingInteraction;
            }
        }

        private void OnInteractionComplete(BaseInteraction interaction)
        {
            _state = State.WaitingForNextTask;
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

        public void QueueTask(Task task)
        {
            _queuedTasks.Enqueue(task);
        }

        public TaskAction ForceTask(string taskID)
        {
            if(_curTaskAction != null)
                 _curTaskAction.OnTaskCancel();
            
            _needsAI.CancelInteraction();
            
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
    }
}

