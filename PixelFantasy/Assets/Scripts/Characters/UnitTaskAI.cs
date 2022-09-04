using System;
using System.Collections;
using Actions;
using Characters.Interfaces;
using DataPersistence;
using Gods;
using Items;
using Tasks;
using UnityEngine;
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

        /// <summary>
        /// Gets a position next to the workPosition so unit isn't standing on their target
        /// </summary>
        private Vector2 GetAdjacentPosition(Vector2 workPosition, float distanceAway = 1f)
        {
            // Choose a random side of the object
            // TODO: Add in a check to ensure the location can be reached
            var sideMod = distanceAway;
            var rand = Random.Range(0, 2);
            if (rand == 1)
            {
                sideMod *= -1;
            }

            var standPos = workPosition;
            standPos.x += sideMod;

            return standPos;
        }

        public void AssignHeldItem(Item itemToHold)
        {
            _heldItem = itemToHold;
            itemToHold.transform.SetParent(transform);
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
                _unitAnim.SetUnitAction(UnitAction.Nothing);
            }
            else
            {
                ExecuteTask(task);
            }
        }

        public void ExecuteTask(TaskBase task)
        {
            if (task == null) return;
            
            _curTask = task;
            state = State.ExecutingTask;

            // Move to location
            if (task is EmergencyTask.MoveToPosition)
            {
                ExecuteTask_MoveToPosition(task as EmergencyTask.MoveToPosition);
                return;
            }
            
            // Clean up garbage
            if (task is CleaningTask.GarbageCleanup)
            {
                ExecuteTask_CleanUpGarbage(task as CleaningTask.GarbageCleanup);
                return;
            }
            
            // Pick up item and move to slot
            if (task is HaulingTask.TakeItemToItemSlot)
            {
                ExecuteTask_TakeItemToItemSlot(task as HaulingTask.TakeItemToItemSlot);
                return;
            }
            
            if (task is HaulingTask.TakeResourceToBlueprint)
            {
                ExecuteTask_TakeResourceToBlueprint(task as HaulingTask.TakeResourceToBlueprint);
                return;
            }
            
            if (task is ConstructionTask.ConstructStructure)
            {
                ExecuteTask_ConstructStructure(task as ConstructionTask.ConstructStructure);
                return;
            }

            if (task is ConstructionTask.DeconstructStructure)
            {
                ExecuteTask_DeconstructStructure(task as ConstructionTask.DeconstructStructure);
                return;
            }

            if (task is FellingTask.CutTree)
            {
                ExecuteTask_CutTree(task as FellingTask.CutTree);
                return;
            }

            if (task is FarmingTask.CutPlant)
            {
                ExecuteTask_CutPlant(task as FarmingTask.CutPlant);
                return;
            }

            if (task is FarmingTask.HarvestFruit)
            {
                ExecuteTask_HarvestFruit(task as FarmingTask.HarvestFruit);
                return;
            }

            if (task is FarmingTask.ClearGrass)
            {
                ExecuteTask_ClearGrass(task as FarmingTask.ClearGrass);
                return;
            }

            if (task is CarpentryTask.CraftItem)
            {
                ExecuteTask_CraftItem_Carpentry(task as CarpentryTask.CraftItem);
                return;
            }
            
            if (task is CarpentryTask.GatherResourceForCrafting)
            {
                ExecuteTask_GatherResourceForCrafting_Carpentry(task as CarpentryTask.GatherResourceForCrafting);
                return;
            }

            if (task is FarmingTask.DigHole)
            {
                ExecuteTask_DigHole(task as FarmingTask.DigHole);
                return;
            }
            
            if (task is FarmingTask.HarvestCrop)
            {
                ExecuteTask_HarvestCrop(task as FarmingTask.HarvestCrop);
                return;
            }
            
            if (task is FarmingTask.PlantCrop)
            {
                ExecuteTask_PlantCrop(task as FarmingTask.PlantCrop);
                return;
            }
            
            if (task is FarmingTask.WaterCrop)
            {
                ExecuteTask_WaterCrop(task as FarmingTask.WaterCrop);
                return;
            }

            if (task is MiningTask.Mine)
            {
                ExecuteTask_Mine(task as MiningTask.Mine);
                return;
            }
            
            // Other task types go here
        }

        private void ClearAction()
        {
            claimedSlot = null;
            _curTask = null;
            currentActionRequestorUID = "";
            currentAction = null;
        }

        #region Execute Tasks

        private void ExecuteTask_MoveToPosition(EmergencyTask.MoveToPosition task)
        {
            workerMover.SetMovePosition(task.targetPosition, () =>
            {
                state = State.WaitingForNextTask;
                ClearAction();
            });
        }

        private void ExecuteTask_CleanUpGarbage(CleaningTask.GarbageCleanup cleanupTask)
        {
            workerMover.SetMovePosition(cleanupTask.targetPosition, () =>
            {
                cleanupTask.cleanUpAction();
                state = State.WaitingForNextTask;
                ClearAction();
            });
        }

        public StorageSlot claimedSlot;
        private void ExecuteTask_TakeItemToItemSlot(HaulingTask.TakeItemToItemSlot task)
        {
            currentAction = task.TaskAction;
            currentActionRequestorUID = task.RequestorUID;
            task.OnTaskAccepted(task.TaskAction);
            task.claimItemSlot(this);
            workerMover.SetMovePosition(task.itemPosition, () =>
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

        private void ExecuteTask_TakeResourceToBlueprint(HaulingTask.TakeResourceToBlueprint task)
        {
            currentAction = task.TaskAction;
            currentActionRequestorUID = task.RequestorUID;
            task.OnTaskAccepted(task.TaskAction);
            workerMover.SetMovePosition(task.resourcePosition, () =>
            {
                task.grabResource(this);
                workerMover.SetMovePosition(task.blueprintPosition, () =>
                {
                    task.useResource(_heldItem);
                    state = State.WaitingForNextTask;
                    ClearAction();
                });
            });
        }

        private void ExecuteTask_ConstructStructure(ConstructionTask.ConstructStructure task)
        {
            currentAction = task.TaskAction;
            currentActionRequestorUID = task.RequestorUID;
            task.OnTaskAccepted(task.TaskAction);
            workerMover.SetMovePosition(GetAdjacentPosition(task.structurePosition), () =>
            {
                _unitAnim.LookAtPostion(task.structurePosition);
                _unitAnim.SetUnitAction(UnitAction.Building);
                DoWork(task.workAmount, () =>
                {
                    task.completeWork();
                    state = State.WaitingForNextTask;
                    _unitAnim.SetUnitAction(UnitAction.Nothing);
                    ClearAction();
                });
            });
        }

        private void ExecuteTask_DeconstructStructure(ConstructionTask.DeconstructStructure task)
        {
            currentAction = task.TaskAction;
            currentActionRequestorUID = task.RequestorUID;
            task.OnTaskAccepted(task.TaskAction);
            task.claimStructure(this);
            workerMover.SetMovePosition(GetAdjacentPosition(task.structurePosition), () =>
            {
                _unitAnim.LookAtPostion(task.structurePosition);
                _unitAnim.SetUnitAction(UnitAction.Building);
                DoWork(task.workAmount, () =>
                {
                    task.OnCompleteTask();
                    state = State.WaitingForNextTask;
                    _unitAnim.SetUnitAction(UnitAction.Nothing);
                    ClearAction();
                });
            });
        }

        private void ExecuteTask_CutTree(FellingTask.CutTree task)
        {
            currentAction = task.TaskAction;
            currentActionRequestorUID = task.RequestorUID;
            task.OnTaskAccepted(task.TaskAction);
            task.claimTree(this);
            workerMover.SetMovePosition(GetAdjacentPosition(task.treePosition), () =>
            {
                _unitAnim.LookAtPostion(task.treePosition);
                _unitAnim.SetUnitAction(UnitAction.Axe);
                DoingWork((WorkAmount ) =>
                {
                    task.OnWork(WorkAmount, () =>
                    {
                        // Work is complete
                        _isDoingWork = false;
                        task.OnCompleteTask();
                        state = State.WaitingForNextTask;
                        _unitAnim.SetUnitAction(UnitAction.Nothing);
                        ClearAction();
                    });
                });
            });
        }
        
        private void ExecuteTask_Mine(MiningTask.Mine task)
        {
            currentAction = task.TaskAction;
            currentActionRequestorUID = task.RequestorUID;
            task.OnTaskAccepted(task.TaskAction);
            task.claimMountain(this);
            workerMover.SetMovePosition(GetAdjacentPosition(task.mountainPosition), () =>
            {
                _unitAnim.LookAtPostion(task.mountainPosition);
                _unitAnim.SetUnitAction(UnitAction.Mining);
                DoWork(task.workAmount, () =>
                {
                    task.OnCompleteTask();
                    state = State.WaitingForNextTask;
                    _unitAnim.SetUnitAction(UnitAction.Nothing);
                    ClearAction();
                });
            });
        }
        
        private void ExecuteTask_CutPlant(FarmingTask.CutPlant task)
        {
            currentAction = task.TaskAction;
            currentActionRequestorUID = task.RequestorUID;
            task.OnTaskAccepted(task.TaskAction);
            task.claimPlant(this);
            workerMover.SetMovePosition(GetAdjacentPosition(task.plantPosition, 0.5f), () =>
            {
                _unitAnim.LookAtPostion(task.plantPosition);
                _unitAnim.SetUnitAction(UnitAction.Doing);
                DoWork(task.workAmount, () =>
                {
                    task.OnCompleteTask();
                    state = State.WaitingForNextTask;
                    _unitAnim.SetUnitAction(UnitAction.Nothing);
                    ClearAction();
                });
            });
        }
        
        private void ExecuteTask_HarvestFruit(FarmingTask.HarvestFruit task)
        {
            currentAction = task.TaskAction;
            currentActionRequestorUID = task.RequestorUID;
            task.OnTaskAccepted(task.TaskAction);
            task.claimPlant(this);
            workerMover.SetMovePosition(GetAdjacentPosition(task.plantPosition, 0.5f), () =>
            {
                _unitAnim.LookAtPostion(task.plantPosition);
                _unitAnim.SetUnitAction(UnitAction.Doing);
                DoWork(task.workAmount, () =>
                {
                    task.OnCompleteTask();
                    state = State.WaitingForNextTask;
                    _unitAnim.SetUnitAction(UnitAction.Nothing);
                    ClearAction();
                });
            });
        }
        
        private void ExecuteTask_ClearGrass(FarmingTask.ClearGrass task)
        {
            currentAction = task.TaskAction;
            currentActionRequestorUID = task.RequestorUID;
            task.OnTaskAccepted(task.TaskAction);
            task.claimDirt(this);
            workerMover.SetMovePosition(GetAdjacentPosition(task.grassPosition), () =>
            {
                _unitAnim.LookAtPostion(task.grassPosition);
                _unitAnim.SetUnitAction(UnitAction.Digging);
                DoWork(task.workAmount, () =>
                {
                    task.OnCompleteTask();
                    state = State.WaitingForNextTask;
                    _unitAnim.SetUnitAction(UnitAction.Nothing);
                    ClearAction();
                });
            });
        }

        private void ExecuteTask_DigHole(FarmingTask.DigHole task)
        {
            currentAction = task.TaskAction;
            currentActionRequestorUID = task.RequestorUID;
            task.OnTaskAccepted(task.TaskAction);
            task.claimHole(this);
            workerMover.SetMovePosition(GetAdjacentPosition(task.holePosition), () =>
            {
                _unitAnim.LookAtPostion(task.holePosition);
                _unitAnim.SetUnitAction(UnitAction.Digging);
                DoWork(task.workAmount, () =>
                {
                    task.OnCompleteTask();
                    state = State.WaitingForNextTask;
                    _unitAnim.SetUnitAction(UnitAction.Nothing);
                    ClearAction();
                });
            });
        }
        
        private void ExecuteTask_PlantCrop(FarmingTask.PlantCrop task)
        {
            currentAction = task.TaskAction;
            currentActionRequestorUID = task.RequestorUID;
            task.OnTaskAccepted(task.TaskAction);
            task.claimHole(this);
            workerMover.SetMovePosition(GetAdjacentPosition(task.holePosition), () =>
            {
                _unitAnim.LookAtPostion(task.holePosition);
                _unitAnim.SetUnitAction(UnitAction.Doing);
                DoWork(task.workAmount, () =>
                {
                    task.OnCompleteTask();
                    state = State.WaitingForNextTask;
                    _unitAnim.SetUnitAction(UnitAction.Nothing);
                    ClearAction();
                });
            });
        }
        
        private void ExecuteTask_WaterCrop(FarmingTask.WaterCrop task)
        {
            currentAction = task.TaskAction;
            currentActionRequestorUID = task.RequestorUID;
            task.OnTaskAccepted(task.TaskAction);
            task.claimCrop(this);
            workerMover.SetMovePosition(GetAdjacentPosition(task.cropPosition), () =>
            {
                _unitAnim.LookAtPostion(task.cropPosition);
                _unitAnim.SetUnitAction(UnitAction.Watering);
                DoWork(task.workAmount, () =>
                {
                    task.OnCompleteTask();
                    state = State.WaitingForNextTask;
                    _unitAnim.SetUnitAction(UnitAction.Nothing);
                    ClearAction();
                });
            });
        }
        
        private void ExecuteTask_HarvestCrop(FarmingTask.HarvestCrop task)
        {
            currentAction = task.TaskAction;
            currentActionRequestorUID = task.RequestorUID;
            task.OnTaskAccepted(task.TaskAction);
            task.claimCrop(this);
            workerMover.SetMovePosition(GetAdjacentPosition(task.cropPosition, 0.5f), () =>
            {
                _unitAnim.LookAtPostion(task.cropPosition);
                _unitAnim.SetUnitAction(UnitAction.Digging);
                DoWork(task.workAmount, () =>
                {
                    task.OnCompleteTask();
                    state = State.WaitingForNextTask;
                    _unitAnim.SetUnitAction(UnitAction.Nothing);
                    ClearAction();
                });
            });
        }

        private void ExecuteTask_CraftItem_Carpentry(CarpentryTask.CraftItem task)
        {
            currentAction = task.TaskAction;
            currentActionRequestorUID = task.RequestorUID;
            task.OnTaskAccepted(task.TaskAction);
            workerMover.SetMovePosition(task.craftPosition, () =>
            {
                _unitAnim.LookAtPostion(task.craftPosition);
                _unitAnim.SetUnitAction(UnitAction.Doing);
                DoWork(task.workAmount, () =>
                {
                    task.completeWork();
                    state = State.WaitingForNextTask;
                    _unitAnim.SetUnitAction(UnitAction.Nothing);
                    ClearAction();
                });
            });
        }
        
        private void ExecuteTask_GatherResourceForCrafting_Carpentry(CarpentryTask.GatherResourceForCrafting task)
        {
            currentAction = task.TaskAction;
            currentActionRequestorUID = task.RequestorUID;
            task.OnTaskAccepted(task.TaskAction);
            workerMover.SetMovePosition(task.resourcePosition, () =>
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

        private void DoWork(float workAmount, Action onWorkComplete)
        {
            // TODO: Change this do toggle doing work based on the unit's work speed and work amount
            _onWorkComplete = onWorkComplete;
            StartCoroutine(WorkSequence(workAmount));
        }

        private IEnumerator WorkSequence(float workAmount)
        {
            float waitTimeS = workAmount * _workSpeed;
            yield return new WaitForSeconds(waitTimeS);
            
            _onWorkComplete.Invoke();
        }

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