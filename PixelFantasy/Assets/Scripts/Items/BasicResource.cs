using System;
using System.Collections.Generic;
using System.Linq;
using Characters;
using DataPersistence;
using Handlers;
using Systems.Appearance.Scripts;
using UnityEngine;

namespace Items
{
    public class BasicResource : PlayerInteractable
    {
        [SerializeField] protected SpriteRenderer _spriteRenderer;
        [SerializeField] private Command _defaultExtractCmd;
        [SerializeField] private BoxCollider2D _obstacleBox;
        [SerializeField] private List<Transform> _workPoints;
        
        protected Action _onResourceClearedCallback;

        protected ResourceSettings _settings;
        public BasicResourceData RuntimeData;
        public override string UniqueID => RuntimeData.UniqueID;
        public override List<SpriteRenderer> SpritesToOutline => new List<SpriteRenderer> { _spriteRenderer };

        public override string PendingTaskUID
        {
            get => RuntimeData.PendingTaskUID;
            set => RuntimeData.PendingTaskUID = value;
        }

        public override bool IsSimilar(PlayerInteractable otherPI)
        {
            if (otherPI is BasicResource basicResource)
            {
                return _settings == basicResource._settings;
            }

            return false;
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            ResourcesDatabase.Instance.DeregisterResource(this);
        }

        public virtual void InitializeResource(ResourceSettings settings)
        {
            _settings = settings;
            RuntimeData = new BasicResourceData();
            RuntimeData.InitData(settings);
            RuntimeData.Position = transform.position;
            
            ResourcesDatabase.Instance.RegisterResource(this);
            UpdateSprite();
        }

        public virtual void LoadResource(BasicResourceData data)
        {
            RuntimeData = data;
            _settings = data.Settings;
            ResourcesDatabase.Instance.RegisterResource(this);
            
            UpdateSprite();
            RefreshTaskIcon();
        }
        
        protected virtual void UpdateSprite()
        {
            var spriteIndex = RuntimeData.SpriteIndex;
            _spriteRenderer.sprite = RuntimeData.Settings.GetSprite(spriteIndex);
        }

        public override Vector2? UseagePosition(Vector2 requestorPosition)
        {
            List<(Transform, float)> distances = new List<(Transform, float)>();
            
            if (_workPoints.Count == 0)
            {
                var pathResult = Helper.DoesPathExist(requestorPosition, transform.position);
                if (pathResult.pathExists)
                {
                    return transform.position;
                }
            }
            
            foreach (var workPoint in _workPoints)
            {
                var pathResult = Helper.DoesPathExist(requestorPosition, workPoint.position);
                if (pathResult.pathExists)
                {
                    float distance = Helper.GetPathLength(pathResult.navMeshPath);
                    distances.Add((workPoint, distance));
                }
            }
            
            if (distances.Count == 0)
            {
                return null;
            }
            
            // Compile the positions that pass the above tests and sort them by distance
            var sortedDistances = distances.OrderBy(x => x.Item2).Select(x => x.Item1).ToList();
            var selectedDistance = sortedDistances[0];
            
            Vector2 result = selectedDistance.position;
            return result;
        }
        
        public override void AssignCommand(Command command)
        {
            CreateTask(command);
        }

        public virtual HarvestableItems GetHarvestableItems()
        {
            return _settings.HarvestableItems;
        }

        public override string DisplayName => _settings.ResourceName;

        public void RefreshSelection()
        {
            if (IsSelected)
            {
                GameEvents.Trigger_RefreshSelection();
            }
        }

        /// <summary>
        /// Work being done by the kinling, (example a swing of axe)
        /// </summary>
        /// <returns>If the work is complete</returns>
        public virtual bool DoExtractionWork(StatsData stats, out float progress)
        {
            var workAmount = stats.GetActionSpeedForSkill(RuntimeData.Settings.ExtractionSkillType, transform);
            RuntimeData.Health -= workAmount;

            if (RuntimeData.Health <= 0)
            {
                ExtractResource(stats);
                progress = 1;
                return true;
            }
            else
            {
                //Update progress
                progress = RuntimeData.HealthPercent;
                return false;
            }
        }

        public virtual void ClearResource(Action onResourceCleared)
        {
            _onResourceClearedCallback = onResourceCleared;
            AssignCommand(_defaultExtractCmd);
        }

        public bool IsGridInObstacleArea(Vector2 gridPos)
        {
            Vector2[] corners = new Vector2[]
            {
                new Vector2(gridPos.x, gridPos.y),
                new Vector2(gridPos.x + 0.5f, gridPos.y + 0.5f),
                new Vector2(gridPos.x - 0.5f, gridPos.y - 0.5f),
                new Vector2(gridPos.x + 0.5f, gridPos.y - 0.5f),
                new Vector2(gridPos.x - 0.5f, gridPos.y + 0.5f),
            };

            foreach (var corner in corners)
            {
                if (_obstacleBox.OverlapPoint(corner))
                {
                    return true;
                }
            }

            return false;
        }

        protected virtual void ExtractResource(StatsData stats)
        {
            Destroy(gameObject);
            
            var resources = RuntimeData.Settings.HarvestableItems.GetItemDrop();
            var spawnPos = Helper.SnapToGridPos(transform.position);
            foreach (var resource in resources)
            {
                int amount = stats.DetermineAmountYielded(RuntimeData.Settings.ExtractionSkillType, resource.Quantity);
                for (int i = 0; i < amount; i++)
                {
                    var data = resource.Item.CreateItemData(spawnPos);
                    ItemsDatabase.Instance.CreateItemObject(data, spawnPos);
                }
            }
            
            RefreshSelection();
            
            if(_onResourceClearedCallback != null) _onResourceClearedCallback.Invoke();
        }

        public virtual UnitAction GetExtractActionAnim()
        {
            return UnitAction.Doing;
        }
        
        public virtual void ToggleAllowed(bool isAllowed)
        {
            
        }
        
        public virtual float GetCurrentHealth()
        {
            if (RuntimeData != null)
            {
                return RuntimeData.Health;
            }
            else
            {
                return _settings.MaxHealth;
            }
        }

        public virtual float GetMaxHealth()
        {
            return _settings.MaxHealth;
        }
    }
}
