using System;
using System.Collections.Generic;
using Actions;
using DataPersistence;
using Gods;
using Interfaces;
using Pathfinding.Util;
using ScriptableObjects;
using Sirenix.OdinInspector;
using Characters;
using UnityEditor;
using UnityEngine;

namespace Items
{
    [RequireComponent(typeof(Interactable))]
    public class Resource : Interactable, IClickableObject, IPersistent
    {
        public GameObject Prefab;
        
        [SerializeField] public GrowingResourceData _growingResourceData;
        [SerializeField] protected SpriteRenderer _spriteRenderer;
        //[SerializeField] protected SpriteRenderer _icon;
        [SerializeField] private ClickObject _clickObject;
        
        protected TaskMaster taskMaster => TaskMaster.Instance;
        protected Spawner spawner => Spawner.Instance;
        public List<int> _assignedTaskRefs = new List<int>();
        protected bool _queuedToCut;
        public UnitTaskAI _incomingUnit;
        public TaskType PendingTask;
        
        public GrowingResourceData GetResourceData()
        {
            return _growingResourceData;
        }
        
        public bool QueuedToCut
        {
            get => _queuedToCut;
            set => _queuedToCut = value;
        }

        public ClickObject GetClickObject()
        {
            return _clickObject;
        }

        public virtual void CancelTasks()
        {
            if (_assignedTaskRefs == null || _assignedTaskRefs.Count == 0) return;

            PendingTask = TaskType.None;
            foreach (var taskRef in _assignedTaskRefs)
            {
                taskMaster.FellingTaskSystem.CancelTask(taskRef);
                taskMaster.FarmingTaskSystem.CancelTask(taskRef);
            }
            _assignedTaskRefs.Clear();
            
            if (_incomingUnit != null)
            {
                _incomingUnit.CancelTask();
            }

            _queuedToCut = false;
            
            RefreshSelection();
        }

        public void SetIcon(string iconName)
        {
            // if (string.IsNullOrEmpty(iconName))
            // {
            //     _icon.sprite = null;
            //     _icon.gameObject.SetActive(false);
            // }
            // else
            // {
            //     _icon.sprite = Librarian.Instance.GetSprite(iconName);
            //     _icon.gameObject.SetActive(true);
            // }
        }

        public void RefreshSelection()
        {
            if (_clickObject.IsSelected)
            {
                GameEvents.Trigger_RefreshSelection();
            }
        }

        public bool IsClickDisabled { get; set; }
        public bool IsAllowed { get; set; }
        public virtual void ToggleAllowed(bool isAllowed)
        {
            
        }

        public virtual List<ActionBase> GetActions()
        {
            return AvailableActions;
        }

        public virtual List<Order> GetOrders()
        {
            throw new System.NotImplementedException();
        }

        public virtual bool IsOrderActive(Order order)
        {
            throw new System.NotImplementedException();
        }

        public virtual void AssignOrder(Order orderToAssign)
        {
            throw new System.NotImplementedException();
        }

        protected virtual void RestorePendingTask(TaskType pendingTask)
        {
            if(pendingTask == TaskType.None) return;
            
            
        }

        public virtual object CaptureState()
        {
            return new Data
            {
                UID = UniqueId,
                Prefab = Prefab,
                Position = transform.position,
                GrowingResourceData = _growingResourceData,
                IsAllowed = this.IsAllowed,
                IsClickDisabled = this.IsClickDisabled,
                PendingTask = PendingTask,
                PendingTasks = PendingTasks,
            };
        }

        public virtual void RestoreState(object data)
        {
            var stateData = (Data)data;
            UniqueId = stateData.UID;
            Prefab = stateData.Prefab;
            transform.position = stateData.Position;
            _growingResourceData = stateData.GrowingResourceData;
            IsAllowed = stateData.IsAllowed;
            IsClickDisabled = stateData.IsClickDisabled;
            
            RestoreTasks(stateData.PendingTasks);
            
            //RestorePendingTask(stateData.PendingTask);
        }

        public struct Data
        {
            public string UID;
            public GameObject Prefab;
            public Vector3 Position;
            public GrowingResourceData GrowingResourceData;
            public bool IsAllowed;
            public bool IsClickDisabled;
            public TaskType PendingTask;
            public List<ActionBase> PendingTasks;

            public GrowingResource.GrowingData GrowingData;
        }
    }
}
