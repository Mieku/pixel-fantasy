using System;
using System.Collections.Generic;
using Actions;
using DataPersistence;
using Gods;
using Interfaces;
using ScriptableObjects;
using UnityEngine;

namespace Items
{
    public class Resource : Interactable, IClickableObject, IPersistent
    {
        public GameObject Prefab;
        
        [SerializeField] public GrowingResourceData growingResourceData;
        [SerializeField] protected SpriteRenderer _spriteRenderer;
        [SerializeField] private ClickObject _clickObject;
        
        protected Spawner spawner => Spawner.Instance;
        public float Health;

        private void Awake()
        {
            Health = GetWorkAmount();
        }

        public GrowingResourceData GetResourceData()
        {
            return growingResourceData;
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
            return Commands;
        }

        public virtual List<ActionBase> GetActions()
        {
            return AvailableActions;
        }

        public List<ActionBase> GetCancellableActions()
        {
            return CancellableActions();
        }

        public virtual void AssignOrder(ActionBase orderToAssign)
        {
            CreateTask(orderToAssign);
        }
        
        public virtual object CaptureState()
        {
            return new Data
            {
                UID = UniqueId,
                Prefab = Prefab,
                Position = transform.position,
                GrowingResourceData = growingResourceData,
                IsAllowed = this.IsAllowed,
                IsClickDisabled = this.IsClickDisabled,
                PendingTasks = PendingTasks,
            };
        }

        public virtual void RestoreState(object data)
        {
            var stateData = (Data)data;
            UniqueId = stateData.UID;
            Prefab = stateData.Prefab;
            transform.position = stateData.Position;
            growingResourceData = stateData.GrowingResourceData;
            IsAllowed = stateData.IsAllowed;
            IsClickDisabled = stateData.IsClickDisabled;
            
            RestoreTasks(stateData.PendingTasks);
        }

        public struct Data
        {
            public string UID;
            public GameObject Prefab;
            public Vector3 Position;
            public GrowingResourceData GrowingResourceData;
            public bool IsAllowed;
            public bool IsClickDisabled;
            public List<ActionBase> PendingTasks;

            public GrowingResource.GrowingData GrowingData;
        }
    }
}
