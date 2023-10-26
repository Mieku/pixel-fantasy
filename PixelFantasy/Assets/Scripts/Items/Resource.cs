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
        public GameObject Prefab;
        
        public ResourceData ResourceData;
        [SerializeField] protected SpriteRenderer _spriteRenderer;
        [SerializeField] private ClickObject _clickObject;

        protected Spawner spawner => Spawner.Instance;
        protected Task _curTask;

        public float Health;

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

        public string DisplayName => ResourceData.ResourceName;

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

        protected virtual void DestroyResource()
        {
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
                Prefab = Prefab,
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
            Prefab = stateData.Prefab;
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
