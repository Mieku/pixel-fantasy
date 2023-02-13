using System;
using System.Collections;
using System.Collections.Generic;
using Actions;
using Characters.Interfaces;
using DataPersistence;
using Gods;
using Items;
using Tasks;
using UnityEngine;
using Zones;
using Action = System.Action;
using Random = UnityEngine.Random;

namespace Characters
{
    public class UnitTaskAI : UniqueObject
    {
        // TODO: Make this no longer be able to be changed in inspector later
        [SerializeField] private ProfessionData professionData;
        [SerializeField] private UnitAnimController _unitAnim;
        [SerializeField] private GameObject _itemPrefab;
        
        public float WorkSpeed = 1f;

        private Action<float> _onWork;
        private Action _onWorkCompleted;
        private bool _isDoingWork;
        private UnitState _unitState;
        private float _unitProductivity;
        private UnitAgent _unitAgent;
        
        public enum State
        {
            WaitingForNextTask,
            ExecutingTask
        }
    
        private UnitThought thought;
        private IMovePosition workerMover;

        private TaskBase _curTask;
        private State state;
        private float waitingTimer;
        private Action _onWorkComplete;
        private float _workSpeed = 1f;
        private Item _heldItem;

        public ActionBase currentAction;
        public string currentActionRequestorUID;

        private bool _isGameLoading;
        private bool _isGameSaving;

        private const float WAIT_TIMER_MAX = .2f; // 200ms

        private static TaskMaster taskMaster => TaskMaster.Instance;

        private void Awake()
        {
            workerMover = GetComponent<IMovePosition>();
            thought = GetComponent<UnitThought>();
            _unitState = GetComponent<UnitState>();
            _unitAgent = GetComponent<UnitAgent>();

            _unitProductivity = _unitState.Productivity;
            
            GameEvents.OnSavingGameBeginning += OnGameSaveStarted;
            GameEvents.OnSavingGameEnd += OnGameSaveEnded;
            GameEvents.OnLoadingGameBeginning += OnGameLoadStarted;
            GameEvents.OnLoadingGameEnd += OnGameLoadEnded;
            GameEvents.OnUnitStatsChanged += OnUnitStatsChanged;
        }

        private void OnDestroy()
        {
            GameEvents.OnSavingGameBeginning -= OnGameSaveStarted;
            GameEvents.OnSavingGameEnd -= OnGameSaveEnded;
            GameEvents.OnLoadingGameBeginning -= OnGameLoadStarted;
            GameEvents.OnLoadingGameEnd -= OnGameLoadEnded;
            GameEvents.OnUnitStatsChanged -= OnUnitStatsChanged;
            
        }

        private void OnUnitStatsChanged(string unitUID)
        {
            if (unitUID.Equals(GetComponent<UID>().uniqueID))
            {
                _unitProductivity = _unitState.Productivity;
            }
        }

        private void OnGameLoadStarted()
        {
            _isGameLoading = true;
        }

        private void OnGameLoadEnded()
        {
            _isGameLoading = false;
        }

        private void OnGameSaveStarted()
        {
            _isGameSaving = true;
        }

        private void OnGameSaveEnded()
        {
            _isGameSaving = false;
        }

        private void Update()
        {
            DoingWorkSequence();
            
            if (_isGameLoading) return;
            if (_isGameSaving) return;
            if(_curTask != null) return;
            
            switch (state)
            {
                case State.WaitingForNextTask:
                    // Waiting to request the next task
                    waitingTimer -= Time.deltaTime;
                    if (waitingTimer <= 0)
                    {
                        waitingTimer = WAIT_TIMER_MAX;
                        RequestNextTask();
                    }
                    break;
                case State.ExecutingTask:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
        
        private void GetAdjacentPosition(Vector2 workPosition, Action<Vector2?, UnitActionDirection> positionCallback, float distanceAway = 1f)
        {
            Vector2 unitPos = transform.position;

            var angle = Helper.CalculateAngle(workPosition, unitPos);
            var angle2 = ClampAngleTo360(angle - 90);
            var angle3 = ClampAngleTo360(angle + 90);
            var angle4 = ClampAngleTo360(angle + 180);

            Vector2 suggestedPos = ConvertAngleToPosition(angle, workPosition, distanceAway);
            if (_unitAgent.IsDestinationPossible(suggestedPos))
            {
                UnitActionDirection dir = DetermineUnitActionDirection(workPosition, suggestedPos);
                positionCallback(suggestedPos, dir);
                return;
            }
            
            suggestedPos = ConvertAngleToPosition(angle2, workPosition, distanceAway);
            if (_unitAgent.IsDestinationPossible(suggestedPos))
            {
                UnitActionDirection dir = DetermineUnitActionDirection(workPosition, suggestedPos);
                positionCallback(suggestedPos, dir);
                return;
            }
            
            suggestedPos = ConvertAngleToPosition(angle3, workPosition, distanceAway);
            if (_unitAgent.IsDestinationPossible(suggestedPos))
            {
                UnitActionDirection dir = DetermineUnitActionDirection(workPosition, suggestedPos);
                positionCallback(suggestedPos, dir);
                return;
            }
            
            suggestedPos = ConvertAngleToPosition(angle4, workPosition, distanceAway);
            if (_unitAgent.IsDestinationPossible(suggestedPos))
            {
                UnitActionDirection dir = DetermineUnitActionDirection(workPosition, suggestedPos);
                positionCallback(suggestedPos, dir);
                return;
            }

            positionCallback(null, UnitActionDirection.Side);
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

        // TODO: Delete this
        private Vector2 GetAdjacentPosition(Vector2 workPosition, float distanceAway = 1f)
        {
            return Vector2.one;
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
        
        public void AssignHeldItem(Item itemToHold)
        {
            _heldItem = itemToHold;
            itemToHold.transform.SetParent(transform);
            itemToHold.transform.localPosition = Vector3.zero;
        }

        private void RequestNextTask()
        {
            TaskBase task = null;
            foreach (var category in professionData.SortedPriorities)
            {
                task = taskMaster.GetNextTaskByCategory(category);
                if (task != null)
                {
                    break;
                }
            }

            _curTask = task;
            if (task == null)
            {
                state = State.WaitingForNextTask;
                _unitAnim.SetUnitAction(UnitAction.Nothing, UnitActionDirection.Side);
            }
            else
            {
                ExecuteTask(task);
            }
        }

        public void ExecuteTask(TaskBase task)
        {
            if (task == null) return;

            // Move to location
            if (task is EmergencyTask.MoveToPosition)
            {
                var moveTask = task as EmergencyTask.MoveToPosition;
                ExecuteIfReachable(moveTask.targetPosition, moveTask, ExecuteTask_MoveToPosition);
                return;
            }
            
            // Clean up garbage
            if (task is CleaningTask.GarbageCleanup)
            {
                var cleanTask = task as CleaningTask.GarbageCleanup;
                ExecuteIfReachable(cleanTask.targetPosition, cleanTask, ExecuteTask_CleanUpGarbage);
                return;
            }
            
            // Pick up item and move to slot
            if (task is HaulingTask.TakeItemToItemSlot)
            {
                var haulTask = task as HaulingTask.TakeItemToItemSlot;
                ExecuteIfReachable(haulTask.itemPosition, haulTask, ExecuteTask_TakeItemToItemSlot);
                return;
            }
            
            if (task is HaulingTask.TakeResourceToBlueprint)
            {
                var haulTask = task as HaulingTask.TakeResourceToBlueprint;
                ExecuteIfReachable(haulTask.resourcePosition, haulTask, ExecuteTask_TakeResourceToBlueprint);
                return;
            }
            
            if (task is ConstructionTask.ConstructStructure)
            {
                var buildTask = task as ConstructionTask.ConstructStructure;
                ExecuteIfReachable(buildTask.structurePosition, buildTask, ExecuteTask_ConstructStructure);
                return;
            }

            if (task is ConstructionTask.DeconstructStructure)
            {
                var deconstructTask = task as ConstructionTask.DeconstructStructure;
                ExecuteIfReachable(deconstructTask.structurePosition, deconstructTask, ExecuteTask_DeconstructStructure);
                return;
            }

            if (task is FellingTask.CutTree)
            {
                var cutTreeTask = task as FellingTask.CutTree;
                ExecuteIfReachable(cutTreeTask.treePosition, cutTreeTask, ExecuteTask_CutTree);
                return;
            }

            if (task is FarmingTask.CutPlant)
            {
                var cutPlantTask = task as FarmingTask.CutPlant;
                ExecuteIfReachable(cutPlantTask.plantPosition, cutPlantTask, ExecuteTask_CutPlant, 0.5f);
                return;
            }

            if (task is FarmingTask.HarvestFruit)
            {
                var harvestFruitTask = task as FarmingTask.HarvestFruit;
                ExecuteIfReachable(harvestFruitTask.plantPosition, harvestFruitTask, ExecuteTask_HarvestFruit, 0.5f);
                return;
            }

            if (task is FarmingTask.ClearGrass)
            {
                var clearGrassTask = task as FarmingTask.ClearGrass;
                ExecuteIfReachable(clearGrassTask.grassPosition, clearGrassTask, ExecuteTask_ClearGrass);
                return;
            }

            if (task is CarpentryTask.CraftItem)
            {
                var craftItemTask = task as CarpentryTask.CraftItem;
                ExecuteIfReachable(craftItemTask.craftPosition, craftItemTask, ExecuteTask_CraftItem_Carpentry);
                return;
            }
            
            if (task is CarpentryTask.GatherResourceForCrafting)
            {
                var gatherTask = task as CarpentryTask.GatherResourceForCrafting;
                ExecuteIfReachable(gatherTask.resourcePosition, gatherTask, ExecuteTask_GatherResourceForCrafting_Carpentry);
                return;
            }

            if (task is FarmingTask.DigHole)
            {
                var digHoleTask = task as FarmingTask.DigHole;
                ExecuteIfReachable(digHoleTask.holePosition, digHoleTask, ExecuteTask_DigHole);
                return;
            }
            
            if (task is FarmingTask.HarvestCrop)
            {
                var harvestCropTask = task as FarmingTask.HarvestCrop;
                ExecuteIfReachable(harvestCropTask.cropPosition, harvestCropTask, ExecuteTask_HarvestCrop, 0.5f);
                return;
            }
            
            if (task is FarmingTask.PlantCrop)
            {
                var plantCropTask = task as FarmingTask.PlantCrop;
                ExecuteIfReachable(plantCropTask.holePosition, plantCropTask, ExecuteTask_PlantCrop);
                return;
            }
            
            if (task is FarmingTask.WaterCrop)
            {
                var waterCropTask = task as FarmingTask.WaterCrop;
                ExecuteIfReachable(waterCropTask.cropPosition, waterCropTask, ExecuteTask_WaterCrop);
                return;
            }

            if (task is FarmingTask.ClearCrop)
            {
                var clearCropTask = task as FarmingTask.ClearCrop;
                ExecuteIfReachable(clearCropTask.cropPosition, clearCropTask, ExecuteTask_ClearCrop);
            }
            
            if (task is FarmingTask.SwapCrop)
            {
                var swapCropTask = task as FarmingTask.SwapCrop;
                ExecuteIfReachable(swapCropTask.cropPosition, swapCropTask, ExecuteTask_SwapCrop);
            }

            if (task is MiningTask.Mine)
            {
                var mineTask = task as MiningTask.Mine;
                ExecuteIfReachable(mineTask.mountainPosition, mineTask, ExecuteTask_Mine);
                return;
            }
            
            // Other task types go here
        }

        private void ExecuteIfReachable(Vector2 targetPosition, TaskBase task, Action<TaskBase, Vector2, UnitActionDirection>  ReachableCallback, float distanceAway = 1f)
        {
            GetAdjacentPosition(targetPosition, (pos, dir) =>
            {
                if (pos == null)
                {
                    // Return to queue
                    _isDoingWork = false;
                    state = State.WaitingForNextTask;
                    ClearAction();

                    TaskMaster.Instance.ReturnTaskToQueue(task);
                }
                else
                {
                    _curTask = task;
                    state = State.ExecutingTask;
                    ReachableCallback(task, (Vector2)pos, dir);
                }
            }, distanceAway);
        }

        private void ClearAction()
        {
            claimedSlot = null;
            _curTask = null;
            currentActionRequestorUID = "";
            currentAction = null;
        }

        #region Execute Tasks

        private void ExecuteTask_MoveToPosition(TaskBase taskbase, Vector2 workPosition, UnitActionDirection actionDir)
        {
            var task = taskbase as EmergencyTask.MoveToPosition;
            workerMover.SetMovePosition(workPosition, () =>
            {
                state = State.WaitingForNextTask;
                ClearAction();
            });
        }

        private void ExecuteTask_CleanUpGarbage(TaskBase taskbase, Vector2 workPosition, UnitActionDirection actionDir)
        {
            var task = taskbase as CleaningTask.GarbageCleanup;
            workerMover.SetMovePosition(workPosition, () =>
            {
                task.cleanUpAction();
                state = State.WaitingForNextTask;
                ClearAction();
            });
        }

        public StorageSlot claimedSlot;
        private void ExecuteTask_TakeItemToItemSlot(TaskBase taskbase, Vector2 workPosition, UnitActionDirection actionDir)
        {
            var task = taskbase as HaulingTask.TakeItemToItemSlot;
            currentAction = task.TaskAction;
            currentActionRequestorUID = task.RequestorUID;
            task.OnTaskAccepted(task.TaskAction);
            task.claimItemSlot(this);
            workerMover.SetMovePosition(workPosition, () =>
            {
                task.grabItem(this, task.item);
                workerMover.SetMovePosition(claimedSlot.GetPosition(), () =>
                {
                    task.dropItem(task.item);
                    claimedSlot.StoreItem(task.item);
                    state = State.WaitingForNextTask;
                    ClearAction();
                });
            });
        }

        private void ExecuteTask_TakeResourceToBlueprint(TaskBase taskbase, Vector2 workPosition, UnitActionDirection actionDir)
        {
            var task = taskbase as HaulingTask.TakeResourceToBlueprint;
            currentAction = task.TaskAction;
            currentActionRequestorUID = task.RequestorUID;
            task.OnTaskAccepted(task.TaskAction);
            task.claimItemSlot(this);
            workerMover.SetMovePosition(workPosition, () =>
            {
                task.grabResource(this);
                claimedSlot = null;
                workerMover.SetMovePosition(task.blueprintPosition, () =>
                {
                    task.useResource(_heldItem);
                    state = State.WaitingForNextTask;
                    ClearAction();
                });
            });
        }

        private void ExecuteTask_ConstructStructure(TaskBase taskbase, Vector2 workPosition, UnitActionDirection actionDir)
        {
            var task = taskbase as ConstructionTask.ConstructStructure;
            currentAction = task.TaskAction;
            currentActionRequestorUID = task.RequestorUID;
            task.OnTaskAccepted(task.TaskAction);
            workerMover.SetMovePosition(workPosition, () =>
            {
                _unitAnim.LookAtPostion(task.structurePosition);
                _unitAnim.SetUnitAction(UnitAction.Building, actionDir);
                DoingWork((WorkAmount ) =>
                {
                    task.OnWork(WorkAmount, () =>
                    {
                        // Work is complete
                        _isDoingWork = false;
                        task.OnCompleteTask();
                        state = State.WaitingForNextTask;
                        _unitAnim.SetUnitAction(UnitAction.Nothing, UnitActionDirection.Side);
                        ClearAction();
                    });
                });
            });
        }

        private void ExecuteTask_DeconstructStructure(TaskBase taskbase, Vector2 workPosition, UnitActionDirection actionDir)
        {
            var task = taskbase as ConstructionTask.DeconstructStructure;
            currentAction = task.TaskAction;
            currentActionRequestorUID = task.RequestorUID;
            task.OnTaskAccepted(task.TaskAction);
            task.claimStructure(this);
            workerMover.SetMovePosition(workPosition, () =>
            {
                _unitAnim.LookAtPostion(task.structurePosition);
                _unitAnim.SetUnitAction(UnitAction.Building, actionDir);
                DoingWork((WorkAmount ) =>
                {
                    task.OnWork(WorkAmount, () =>
                    {
                        // Work is complete
                        _isDoingWork = false;
                        task.OnCompleteTask();
                        state = State.WaitingForNextTask;
                        _unitAnim.SetUnitAction(UnitAction.Nothing, UnitActionDirection.Side);
                        ClearAction();
                    });
                });
            });
        }

        private void ExecuteTask_CutTree(TaskBase taskbase, Vector2 workPosition, UnitActionDirection actionDir)
        {
            var task = taskbase as FellingTask.CutTree;
            currentAction = task.TaskAction;
            currentActionRequestorUID = task.RequestorUID;
            task.OnTaskAccepted(task.TaskAction);
            task.claimTree(this);
            workerMover.SetMovePosition(workPosition, () =>
            {
                _unitAnim.LookAtPostion(task.treePosition);
                _unitAnim.SetUnitAction(UnitAction.Axe, actionDir);
                DoingWork((WorkAmount ) =>
                {
                    task.OnWork(WorkAmount, () =>
                    {
                        // Work is complete
                        _isDoingWork = false;
                        task.OnCompleteTask();
                        state = State.WaitingForNextTask;
                        _unitAnim.SetUnitAction(UnitAction.Nothing, UnitActionDirection.Side);
                        ClearAction();
                    });
                });
            });
        }
        
        private void ExecuteTask_Mine(TaskBase taskbase, Vector2 workPosition, UnitActionDirection actionDir)
        {
            var task = taskbase as MiningTask.Mine;
            currentAction = task.TaskAction;
            currentActionRequestorUID = task.RequestorUID;
            task.OnTaskAccepted(task.TaskAction);
            task.claimMountain(this);
            workerMover.SetMovePosition(workPosition, () =>
            {
                _unitAnim.LookAtPostion(task.mountainPosition);
                _unitAnim.SetUnitAction(UnitAction.Mining, actionDir);
                DoingWork((WorkAmount ) =>
                {
                    task.OnWork(WorkAmount, () =>
                    {
                        // Work is complete
                        _isDoingWork = false;
                        task.OnCompleteTask();
                        state = State.WaitingForNextTask;
                        _unitAnim.SetUnitAction(UnitAction.Nothing, UnitActionDirection.Side);
                        ClearAction();
                    });
                });
            });
        }
        
        private void ExecuteTask_CutPlant(TaskBase taskbase, Vector2 workPosition, UnitActionDirection actionDir)
        {
            var task = taskbase as FarmingTask.CutPlant;
            currentAction = task.TaskAction;
            currentActionRequestorUID = task.RequestorUID;
            task.OnTaskAccepted(task.TaskAction);
            task.claimPlant(this);
            workerMover.SetMovePosition(workPosition, () =>
            {
                _unitAnim.LookAtPostion(task.plantPosition);
                _unitAnim.SetUnitAction(UnitAction.Doing, actionDir);
                DoingWork((WorkAmount ) =>
                {
                    task.OnWork(WorkAmount, () =>
                    {
                        // Work is complete
                        _isDoingWork = false;
                        task.OnCompleteTask();
                        state = State.WaitingForNextTask;
                        _unitAnim.SetUnitAction(UnitAction.Nothing, UnitActionDirection.Side);
                        ClearAction();
                    });
                });
            });
        }
        
        private void ExecuteTask_HarvestFruit(TaskBase taskbase, Vector2 workPosition, UnitActionDirection actionDir)
        {
            var task = taskbase as FarmingTask.HarvestFruit;
            currentAction = task.TaskAction;
            currentActionRequestorUID = task.RequestorUID;
            task.OnTaskAccepted(task.TaskAction);
            task.claimPlant(this);
            workerMover.SetMovePosition(workPosition, () =>
            {
                _unitAnim.LookAtPostion(task.plantPosition);
                _unitAnim.SetUnitAction(UnitAction.Doing, actionDir);
                DoingWork((WorkAmount ) =>
                {
                    task.OnWork(WorkAmount, () =>
                    {
                        // Work is complete
                        _isDoingWork = false;
                        task.OnCompleteTask();
                        state = State.WaitingForNextTask;
                        _unitAnim.SetUnitAction(UnitAction.Nothing, UnitActionDirection.Side);
                        ClearAction();
                    });
                });
            });
        }
        
        private void ExecuteTask_ClearGrass(TaskBase taskbase, Vector2 workPosition, UnitActionDirection actionDir)
        {
            var task = taskbase as FarmingTask.ClearGrass;
            currentAction = task.TaskAction;
            currentActionRequestorUID = task.RequestorUID;
            task.OnTaskAccepted(task.TaskAction);
            task.claimDirt(this);
            workerMover.SetMovePosition(workPosition, () =>
            {
                _unitAnim.LookAtPostion(task.grassPosition);
                _unitAnim.SetUnitAction(UnitAction.Digging, actionDir);
                DoingWork((WorkAmount ) =>
                {
                    task.OnWork(WorkAmount, () =>
                    {
                        // Work is complete
                        _isDoingWork = false;
                        task.OnCompleteTask();
                        state = State.WaitingForNextTask;
                        _unitAnim.SetUnitAction(UnitAction.Nothing, UnitActionDirection.Side);
                        ClearAction();
                    });
                });
            });
        }

        private void ExecuteTask_DigHole(TaskBase taskbase, Vector2 workPosition, UnitActionDirection actionDir)
        {
            var task = taskbase as FarmingTask.DigHole;
            currentAction = task.TaskAction;
            currentActionRequestorUID = task.RequestorUID;
            task.OnTaskAccepted(task.TaskAction);
            task.claimHole(this);
            workerMover.SetMovePosition(workPosition, () =>
            {
                _unitAnim.LookAtPostion(task.holePosition);
                _unitAnim.SetUnitAction(UnitAction.Digging, actionDir);
                DoingWork((WorkAmount ) =>
                {
                    task.OnWork(WorkAmount, () =>
                    {
                        // Work is complete
                        _isDoingWork = false;
                        task.OnCompleteTask();
                        state = State.WaitingForNextTask;
                        _unitAnim.SetUnitAction(UnitAction.Nothing, UnitActionDirection.Side);
                        ClearAction();
                    });
                });
            });
        }
        
        private void ExecuteTask_PlantCrop(TaskBase taskbase, Vector2 workPosition, UnitActionDirection actionDir)
        {
            var task = taskbase as FarmingTask.PlantCrop;
            currentAction = task.TaskAction;
            currentActionRequestorUID = task.RequestorUID;
            task.OnTaskAccepted(task.TaskAction);
            task.claimHole(this);
            workerMover.SetMovePosition(workPosition, () =>
            {
                _unitAnim.LookAtPostion(task.holePosition);
                _unitAnim.SetUnitAction(UnitAction.Doing, actionDir);
                DoingWork((WorkAmount ) =>
                {
                    task.OnWork(WorkAmount, () =>
                    {
                        // Work is complete
                        _isDoingWork = false;
                        task.OnCompleteTask();
                        state = State.WaitingForNextTask;
                        _unitAnim.SetUnitAction(UnitAction.Nothing, UnitActionDirection.Side);
                        ClearAction();
                    });
                });
            });
        }
        
        private void ExecuteTask_WaterCrop(TaskBase taskbase, Vector2 workPosition, UnitActionDirection actionDir)
        {
            var task = taskbase as FarmingTask.WaterCrop;
            currentAction = task.TaskAction;
            currentActionRequestorUID = task.RequestorUID;
            task.OnTaskAccepted(task.TaskAction);
            task.claimCrop(this);
            workerMover.SetMovePosition(workPosition, () =>
            {
                _unitAnim.LookAtPostion(task.cropPosition);
                _unitAnim.SetUnitAction(UnitAction.Watering, actionDir);
                DoingWork((WorkAmount ) =>
                {
                    task.OnWork(WorkAmount, () =>
                    {
                        // Work is complete
                        _isDoingWork = false;
                        task.OnCompleteTask();
                        state = State.WaitingForNextTask;
                        _unitAnim.SetUnitAction(UnitAction.Nothing, UnitActionDirection.Side);
                        ClearAction();
                    });
                });
            });
        }
        
        private void ExecuteTask_HarvestCrop(TaskBase taskbase, Vector2 workPosition, UnitActionDirection actionDir)
        {
            var task = taskbase as FarmingTask.HarvestCrop;
            currentAction = task.TaskAction;
            currentActionRequestorUID = task.RequestorUID;
            task.OnTaskAccepted(task.TaskAction);
            task.claimCrop(this);
            workerMover.SetMovePosition(workPosition, () =>
            {
                _unitAnim.LookAtPostion(task.cropPosition);
                _unitAnim.SetUnitAction(UnitAction.Digging, actionDir);
                DoingWork((WorkAmount ) =>
                {
                    task.OnWork(WorkAmount, () =>
                    {
                        // Work is complete
                        _isDoingWork = false;
                        task.OnCompleteTask();
                        state = State.WaitingForNextTask;
                        _unitAnim.SetUnitAction(UnitAction.Nothing, UnitActionDirection.Side);
                        ClearAction();
                    });
                });
            });
        }
        
        private void ExecuteTask_SwapCrop(TaskBase taskbase, Vector2 workPosition, UnitActionDirection actionDir)
        {
            var task = taskbase as FarmingTask.SwapCrop;
            currentAction = task.TaskAction;
            currentActionRequestorUID = task.RequestorUID;
            task.OnTaskAccepted(task.TaskAction);
            task.claimCrop(this);
            workerMover.SetMovePosition(workPosition, () =>
            {
                _unitAnim.LookAtPostion(task.cropPosition);
                _unitAnim.SetUnitAction(UnitAction.Digging, actionDir);
                DoingWork((WorkAmount ) =>
                {
                    task.OnWork(WorkAmount, () =>
                    {
                        // Work is complete
                        _isDoingWork = false;
                        task.OnCompleteTask();
                        state = State.WaitingForNextTask;
                        _unitAnim.SetUnitAction(UnitAction.Nothing, UnitActionDirection.Side);
                        ClearAction();
                    });
                });
            });
        }
        
        private void ExecuteTask_ClearCrop(TaskBase taskbase, Vector2 workPosition, UnitActionDirection actionDir)
        {
            var task = taskbase as FarmingTask.ClearCrop;
            currentAction = task.TaskAction;
            currentActionRequestorUID = task.RequestorUID;
            task.OnTaskAccepted(task.TaskAction);
            task.claimCrop(this);
            workerMover.SetMovePosition(workPosition, () =>
            {
                _unitAnim.LookAtPostion(task.cropPosition);
                _unitAnim.SetUnitAction(UnitAction.Digging, actionDir);
                DoingWork((WorkAmount ) =>
                {
                    task.OnWork(WorkAmount, () =>
                    {
                        // Work is complete
                        _isDoingWork = false;
                        task.OnCompleteTask();
                        state = State.WaitingForNextTask;
                        _unitAnim.SetUnitAction(UnitAction.Nothing, UnitActionDirection.Side);
                        ClearAction();
                    });
                });
            });
        }

        private void ExecuteTask_CraftItem_Carpentry(TaskBase taskbase, Vector2 workPosition, UnitActionDirection actionDir)
        {
            var task = taskbase as CarpentryTask.CraftItem;
            currentAction = task.TaskAction;
            currentActionRequestorUID = task.RequestorUID;
            task.OnTaskAccepted(task.TaskAction);
            workerMover.SetMovePosition(workPosition, () =>
            {
                _unitAnim.LookAtPostion(task.craftPosition);
                _unitAnim.SetUnitAction(UnitAction.Doing, actionDir);
                DoingWork((WorkAmount ) =>
                {
                    task.OnWork(WorkAmount, () =>
                    {
                        // Work is complete
                        _isDoingWork = false;
                        task.OnCompleteTask();
                        state = State.WaitingForNextTask;
                        _unitAnim.SetUnitAction(UnitAction.Nothing, UnitActionDirection.Side);
                        ClearAction();
                    });
                });
            });
        }
        
        private void ExecuteTask_GatherResourceForCrafting_Carpentry(TaskBase taskbase, Vector2 workPosition, UnitActionDirection actionDir)
        {
            var task = taskbase as CarpentryTask.GatherResourceForCrafting;
            currentAction = task.TaskAction;
            currentActionRequestorUID = task.RequestorUID;
            task.OnTaskAccepted(task.TaskAction);
            workerMover.SetMovePosition(workPosition, () =>
            {
                task.grabResource(this);
                workerMover.SetMovePosition(task.craftingTable.transform.position, () =>
                {
                    task.useResource(_heldItem);
                    state = State.WaitingForNextTask;
                    ClearAction();
                });
            });
        }

        #endregion
        
        public void CancelTask()
        {
            _isDoingWork = false;
            state = State.WaitingForNextTask;
            StopAllCoroutines();
            workerMover.SetMovePosition(transform.position, () => {});
            ClearAction();
            DropHeldItem();
        }

        private void DropHeldItem()
        {
            if (_heldItem == null) return;

            Spawner.Instance.SpawnItem(_heldItem.GetItemData(), transform.position, true);
            Destroy(_heldItem.gameObject);
            _heldItem = null;
        }
        
        private void DoingWork(Action<float> onWork)
        {
            _onWork = onWork;
            _workTimer = 0;
            _isDoingWork = true;
        }

        private float _workTimer;
        private void DoingWorkSequence()
        {
            if (!_isDoingWork) return;

            _workTimer += TimeManager.Instance.DeltaTime;
            if (_workTimer >= WorkSpeed)
            {
                _workTimer = 0;
                _onWork.Invoke(_unitProductivity);
            }
        }
        
        public UnitTaskData GetSaveData()
        {
            TaskType taskType = TaskType.None;
            string targetUID = "";
            string slotUID = "";
            object heldItemData = null;
            
            if(claimedSlot != null)
            {
                slotUID = claimedSlot.UniqueId;
            }

            if (state == State.ExecutingTask && _curTask != null)
            {
                targetUID = _curTask.TargetUID;
            }

            if (_heldItem != null)
            {
                heldItemData = _heldItem.CaptureState();
            }

            return new UnitTaskData
            {
                WasExecutingTask = state == State.ExecutingTask,
                TargetUID = targetUID,
                ClaimedSlotUID = slotUID,
                HeldItemData = heldItemData,
                ProfessionData = professionData,
                CurrentAction = currentAction,
                CurrentActionRequestorUID = currentActionRequestorUID,
            };
        }

        public void SetLoadData(UnitTaskData taskData)
        {
            professionData = taskData.ProfessionData;
            currentAction = taskData.CurrentAction;
            currentActionRequestorUID = taskData.CurrentActionRequestorUID;
            
            if (!string.IsNullOrEmpty(taskData.ClaimedSlotUID))
            {
                var slotGO = UIDManager.Instance.GetGameObject(taskData.ClaimedSlotUID);
                if (slotGO != null)
                {
                    claimedSlot = slotGO.GetComponent<StorageSlot>();
                }
            }

            // If the unit had a held item, spawn it
            if (taskData.HeldItemData != null)
            {
                var itemData = (Item.Data)taskData.HeldItemData;
                var childObj = Instantiate(_itemPrefab, transform);
                childObj.GetComponent<IPersistent>().RestoreState(itemData);
                _heldItem = childObj.GetComponent<Item>();
            }
            
            ResumeCurrentAction();
        }

        private void ResumeCurrentAction()
        {
            if (currentAction == null) return;
            if (currentAction.id == "Take Item To Slot") return;

            if (currentAction.id == "Take Resource To Blueprint")
            {
                var requestor = UIDManager.Instance.GetGameObject(currentActionRequestorUID).GetComponent<Interactable>();
                var itemToBlueprintAct = currentAction as ActionTakeResourceToBlueprint;
                var task = itemToBlueprintAct.RestoreTask(requestor, claimedSlot, _heldItem);
                
                ExecuteTask(task);
            }
            else
            {
                var requestor = UIDManager.Instance.GetGameObject(currentActionRequestorUID).GetComponent<Interactable>();
                var task = currentAction.RestoreTask(requestor, false);

                ExecuteTask(task);
            }
        }

        public struct UnitTaskData
        {
            public string TargetUID;
            public bool WasExecutingTask;
            public string ClaimedSlotUID;
            public object HeldItemData;
            public ProfessionData ProfessionData;
            
            public ActionBase CurrentAction;
            public string CurrentActionRequestorUID;
        }
    }
}