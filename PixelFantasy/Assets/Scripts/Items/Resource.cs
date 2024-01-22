using System;
using System.Collections.Generic;
using DataPersistence;
using Interfaces;
using Managers;
using ScriptableObjects;
using Systems.Details.Generic_Details.Scripts;
using TaskSystem;
using UnityEngine;

namespace Items
{
    public class Resource : PlayerInteractable, IClickableObject, IPersistent
    {
        public ResourceData ResourceData;
        [SerializeField] protected SpriteRenderer _spriteRenderer;
        [SerializeField] private ClickObject _clickObject;
        [SerializeField] private Command _defaultClearCmd;
        [SerializeField] private BoxCollider2D _obstacleBox;

        protected Spawner spawner => Spawner.Instance;
        protected Task _curTask;
        protected Action _onResourceClearedCallback;

        public float Health;
        
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
            return null;
        }

        public Sprite GetHealthIcon()
        {
            return Librarian.Instance.GetSprite("Health");
        }

        public float GetHealthPercentage()
        {
            return Health / GetWorkAmount();
        }

        public virtual float MinWorkDistance => 1f;

        public virtual string DisplayName => ResourceData.ResourceName;

        protected virtual void Awake()
        {
            _clickObject = GetComponent<ClickObject>();
            Health = GetWorkAmount();
        }

        public bool HasTask => _curTask != null;

        public ResourceData GetResourceData()
        {
            return ResourceData;
        }

        public ClickObject GetClickObject()
        {
            return _clickObject;
        }
        
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
        public virtual bool DoWork(float workAmount)
        {
            Health -= workAmount;
            if (Health <= 0)
            {
                DestroyResource();
                return true;
            }
            
            return false;
        }

        public virtual void ClearResource(Action onResourceCleared)
        {
            _onResourceClearedCallback = onResourceCleared;
            AssignCommand(_defaultClearCmd);
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

        protected virtual void DestroyResource()
        {
            Destroy(gameObject);
            
            if(_onResourceClearedCallback != null) _onResourceClearedCallback.Invoke();
        }

        public virtual UnitAction GetExtractActionAnim()
        {
            return UnitAction.Doing;
        }

        public bool IsClickDisabled { get; set; }
        public bool IsAllowed { get; set; }
        public virtual void ToggleAllowed(bool isAllowed)
        {
            
        }

        public virtual List<Command> GetCommands()
        {
            return new List<Command>(Commands);
        }
        
        public virtual object CaptureState()
        {
            return new Data
            {
                UID = UniqueId,
                Position = transform.position,
                ResourceData = ResourceData,
                IsAllowed = this.IsAllowed,
                IsClickDisabled = this.IsClickDisabled,
            };
        }

        public virtual void RestoreState(object data)
        {
            var stateData = (Data)data;
            UniqueId = stateData.UID;
            transform.position = stateData.Position;
            ResourceData = stateData.ResourceData;
            IsAllowed = stateData.IsAllowed;
            IsClickDisabled = stateData.IsClickDisabled;
        }

        public struct Data
        {
            public string UID;
            public GameObject Prefab;
            public Vector3 Position;
            public ResourceData ResourceData;
            public bool IsAllowed;
            public bool IsClickDisabled;

            public GrowingResource.GrowingData GrowingData;
        }
    }
}
