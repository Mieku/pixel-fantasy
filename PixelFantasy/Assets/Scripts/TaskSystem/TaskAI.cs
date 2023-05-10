using System;
using System.Collections.Generic;
using System.Linq;
using Buildings;
using Characters;
using Gods;
using Items;
using UnityEngine;
using UnityEngine.AI;

namespace TaskSystem
{
    public class TaskAI : MonoBehaviour
    {
        [SerializeField] private ProfessionData _professionData;
        [SerializeField] private Unit _unit;
        
        private List<TaskAction> _taskActions;
        private State _state;
        private float _waitingTimer;
        private TaskAction _curTaskAction;
        private Item _heldItem;
        private Queue<Task> _queuedTasks = new Queue<Task>();
        
        private const float WAIT_TIMER_MAX = .2f; // 200ms

        public Unit Unit => _unit;
        public Family Family => FamilyManager.Instance.GetFamily(_unit.GetUnitState());
        public ProductionBuilding Occupation => _unit.GetUnitState().Occupation;
        public Profession Profession => _unit.GetUnitState().Profession;

        public enum State
        {
            WaitingForNextTask,
            ExecutingTask
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
                case State.ExecutingTask:
                    if (_curTaskAction != null)
                    {
                        _curTaskAction.DoAction();
                    }
                    else
                    {
                        Debug.LogError("Attempted to Execute null task action");
                    }
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private TaskAction FindTaskActionFor(Task task)
        {
            foreach (var taskAction in _taskActions)
            {
                if (taskAction.TaskId == task.TaskId)
                {
                    return taskAction;
                }
            }

            return null;
        }

        public void CurrentTaskDone()
        {
            _curTaskAction = null;
            _state = State.WaitingForNextTask;
        }

        private void RequestNextTask()
        {
            Task task = null;
            
            // Check if they have a queued task
            if (_queuedTasks.Count > 0)
            {
                task = _queuedTasks.Dequeue();
            }

            if (Occupation != null)
            {
                task = Occupation.GetTask();
            }

            if (task == null)
            {
                task = TaskManager.Instance.GetNextTaskByProfession(Profession);
            }

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

            SetupTaskAction(task);
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
                // If no action is can't be done, return it to the queue
                TaskManager.Instance.AddTask(task);
                _state = State.WaitingForNextTask;
                return;
            }

            _curTaskAction = taskAction;
            taskAction.PrepareAction(task);
            _state = State.ExecutingTask;
        }
        
        public UnitActionDirection GetActionDirection(Vector3 targetPos)
        {
            return DetermineUnitActionDirection(targetPos, transform.position);
        }
        
        private UnitActionDirection DetermineUnitActionDirection(Vector3 workPos, Vector3 standPos)
        {
            const float threshold = .25f;

            if (standPos.y >= workPos.y + threshold)
            {
                return UnitActionDirection.Down;
            } else if (standPos.y <= workPos.y - threshold)
            {
                return UnitActionDirection.Up;
            }
            else
            {
                return UnitActionDirection.Side;
            }
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
            Vector2 result = new Vector2();
            
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

        public void DropCarriedItem()
        {
            if (_heldItem == null) return;
        
            Spawner.Instance.SpawnItem(_heldItem.GetItemData(), transform.position, true);
            Destroy(_heldItem.gameObject);
            _heldItem = null;
        }
    }
}
