using System;
using System.Collections.Generic;
using Gods;
using ScriptableObjects;
using Tasks;
using Unit;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Items
{
    public class TreeResource : Resource
    {
        [SerializeField] private SpriteRenderer _spriteRenderer;
        [SerializeField] private SpriteRenderer _icon;
        [SerializeField] private bool _overrideFullGrowth;
        
        private TaskMaster taskMaster => TaskMaster.Instance;
        private int _growthIndex;
        private float _ageSec;
        private float _ageForNextGrowth;
        private bool _fullyGrown;
        private float _reproductionTimer;
        private bool _queuedToCut;
        private UnitTaskAI _incomingUnit;
        private List<int> _assignedTaskRefs = new List<int>();

        private void Start()
        {
            if (_overrideFullGrowth)
            {
                _fullyGrown = true;
                _growthIndex = _resourceData.GrowthStages.Count - 1;
            }
            
            var stage = _resourceData.GetGrowthStage(_growthIndex);
            _ageForNextGrowth += stage.SecsInStage;
            _reproductionTimer = _resourceData.ReproductiveRateSec;

            UpdateSprite();
        }

        private void Update()
        {
            GrowthCheck();
            ReproductionCheck();
        }

        private void GrowthCheck()
        {
            if (!_fullyGrown)
            {
                _ageSec += Time.deltaTime;
                if (_ageSec >= _ageForNextGrowth)
                {
                    _growthIndex++;

                    if (_growthIndex < _resourceData.GrowthStages.Count)
                    {
                        var stage = _resourceData.GetGrowthStage(_growthIndex);
                        _ageForNextGrowth += stage.SecsInStage;
                    }
                    else
                    {
                        _fullyGrown = true;
                    }
                    
                    UpdateSprite();
                }
            }
        }

        private void ReproductionCheck()
        {
            if (_fullyGrown && _resourceData.Reproduces)
            {
                _reproductionTimer -= Time.deltaTime;
                if (_reproductionTimer < 0)
                {
                    _reproductionTimer = _resourceData.ReproductiveRateSec;
                    AttemptReproduction();
                }
            }
        }

        private void UpdateSprite()
        {
            var stage = _resourceData.GetGrowthStage(_growthIndex);
            _spriteRenderer.sprite = stage.GrowthSprite;
        }

        private void AttemptReproduction()
        {
            var pos = _resourceData.GetReproductionPos(transform.position);
            var valid = _resourceData.IsReproductionPosValid(pos);
            if (valid)
            {
                Spawner.Instance.SpawnTree(pos);
            }
        }

        public bool QueuedToCut => _queuedToCut;

        public void CreateCutTreeTask()
        {
            _queuedToCut = true;
            SetIcon("Axe");
            
            // Choose a random side of the tree
            var sideMod = 1;
            var rand = Random.Range(0, 2);
            if (rand == 1)
            {
                sideMod *= -1;
            }

            var cutPos = transform.position;
            cutPos.x += sideMod;

            var task = new FellingTask.CutTree()
            {
                claimTree = (UnitTaskAI unitTaskAI) =>
                {
                    _incomingUnit = unitTaskAI;
                },
                treePosition = cutPos,
                workAmount = _resourceData.GetWorkToCut(_growthIndex),
                completeWork = HarvestTree
            };
            
            _assignedTaskRefs.Add(task.GetHashCode());
            taskMaster.FellingTaskSystem.AddTask(task);
        }

        public void CancelCutTreeTask()
        {
            _queuedToCut = false;
            SetIcon(null);
            CancelTasks();
        }
        
        private void CancelTasks()
        {
            if (_assignedTaskRefs == null || _assignedTaskRefs.Count == 0) return;

            foreach (var taskRef in _assignedTaskRefs)
            {
                taskMaster.FellingTaskSystem.CancelTask(taskRef);
            }
            _assignedTaskRefs.Clear();
            
            if (_incomingUnit != null)
            {
                _incomingUnit.CancelTask();
            }
        }
        
        private void SetIcon(string iconName)
        {
            if (string.IsNullOrEmpty(iconName))
            {
                _icon.sprite = null;
                _icon.gameObject.SetActive(false);
            }
            else
            {
                _icon.sprite = Librarian.Instance.GetSprite(iconName);
                _icon.gameObject.SetActive(true);
            }
        }

        private void HarvestTree()
        {
            var resources = _resourceData.GetGrowthStage(_growthIndex).HarvestableItems.GetItemDrop();
            foreach (var resource in resources)
            {
                for (int i = 0; i < resource.Quantity; i++)
                {
                    Spawner.Instance.SpawnItem(resource.Item, transform.position, true);
                }
            }
            
            Destroy(gameObject);
        }
    }
}
