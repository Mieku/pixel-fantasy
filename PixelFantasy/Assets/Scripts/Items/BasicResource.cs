using System;
using System.Collections.Generic;
using System.Linq;
using Characters;
using DataPersistence;
using Interfaces;
using Managers;
using Newtonsoft.Json;
using Systems.Appearance.Scripts;
using TaskSystem;
using UnityEngine;
using UnityEngine.Serialization;

namespace Items
{
    public class BasicResource : PlayerInteractable, IClickableObject
    {
        [SerializeField] protected SpriteRenderer _spriteRenderer;
        [SerializeField] private ClickObject _clickObject;
        [FormerlySerializedAs("_defaultClearCmd")] [SerializeField] private Command _defaultExtractCmd;
        [SerializeField] private BoxCollider2D _obstacleBox;
        [SerializeField] private List<Transform> _workPoints;
        [SerializeField] private List<string> _invalidPlacementTags = new List<string>() { "Water", "Wall", "Obstacle", "Nature", "Structure"};

        protected Spawner spawner => Spawner.Instance;
        protected Action _onResourceClearedCallback;

        protected ResourceSettings _settings;
        public BasicResourceData RuntimeData;
        
        public Action OnChanged { get; set; }
        
        private void OnEnable()
        {
            ResourcesDatabase.Instance?.RegisterResource(this);
        }

        private void OnDisable()
        {
            ResourcesDatabase.Instance?.DeregisterResource(this);
        }
        
        public virtual void InitializeResource(ResourceSettings settings)
        {
            _settings = settings;
            RuntimeData = new BasicResourceData();
            RuntimeData.InitData(settings);
            RuntimeData.Position = transform.position;
            
            UpdateSprite();
        }

        public virtual void LoadResource(BasicResourceData data)
        {
            RuntimeData = data;
            _settings = data.Settings;
            
            UpdateSprite();
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
        
        public void AssignCommand(Command command, object payload = null)
        {
            CreateTask(command, payload);
        }

        public PlayerInteractable GetPlayerInteractable()
        {
            return this;
        }

        public virtual HarvestableItems GetHarvestableItems()
        {
            return _settings.HarvestableItems;
        }

        public virtual string DisplayName => _settings.ResourceName;

        protected virtual void Awake()
        {
            _clickObject = GetComponent<ClickObject>();
        }
        
        public ClickObject GetClickObject()
        {
            return _clickObject;
        }

        public bool IsClickDisabled { get; set; }

        public void RefreshSelection()
        {
            if (_clickObject.IsSelected)
            {
                GameEvents.Trigger_RefreshSelection();
            }
        }

        /// <summary>
        /// Work being done by the kinling, (example a swing of axe)
        /// </summary>
        /// <param name="workAmount"></param>
        /// <returns>If the work is complete</returns>
        public virtual bool DoExtractionWork(StatsData stats)
        {
            var workAmount = stats.GetActionSpeedForSkill(RuntimeData.Settings.ExtractionSkillType, transform);
            RuntimeData.Health -= workAmount;
            if (RuntimeData.Health <= 0)
            {
                ExtractResource(stats);
                return true;
            }
            
            return false;
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
            var resources = RuntimeData.Settings.HarvestableItems.GetItemDrop();
            foreach (var resource in resources)
            {
                int amount = stats.DetermineAmountYielded(RuntimeData.Settings.ExtractionSkillType, resource.Quantity);
                for (int i = 0; i < amount; i++)
                {
                    spawner.SpawnItem(resource.Item, transform.position, true);
                }
            }
            
            Destroy(gameObject);
            
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

        public virtual List<Command> GetCommands()
        {
            return new List<Command>(Commands);
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
