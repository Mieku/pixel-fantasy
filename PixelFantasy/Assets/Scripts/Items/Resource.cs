using System.Collections.Generic;
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
    public class Resource : MonoBehaviour, IClickableObject, IPersistent
    {
        public GameObject Prefab;
        
        [SerializeField] protected GrowingResourceData _growingResourceData;
        [SerializeField] protected SpriteRenderer _spriteRenderer;
        [SerializeField] protected SpriteRenderer _icon;
        [SerializeField] private ClickObject _clickObject;
        
        protected TaskMaster taskMaster => TaskMaster.Instance;
        protected Spawner spawner => Spawner.Instance;
        protected List<int> _assignedTaskRefs = new List<int>();
        protected bool _queuedToCut;
        protected UnitTaskAI _incomingUnit;

        private string _guid;

        [Button("Assign GUID")]
        private void SetGUID()
        {
            if (_guid == "")
            {
                _guid = Guid.NewGuid().ToString();
            }
        }
        
        public GrowingResourceData GetResourceData()
        {
            return _growingResourceData;
        }
        
        public bool QueuedToCut => _queuedToCut;

        public ClickObject GetClickObject()
        {
            return _clickObject;
        }

        protected virtual void CancelTasks()
        {
            if (_assignedTaskRefs == null || _assignedTaskRefs.Count == 0) return;

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
        
        protected void SetIcon(string iconName)
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
        
        protected void RefreshSelection()
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

        public virtual object CaptureState()
        {
            return new Data
            {
                GUID = _guid,
                Prefab = Prefab,
                Position = transform.position,
                GrowingResourceData = _growingResourceData,
                AssignedTaskRefs = _assignedTaskRefs,
                QueuedToCut = _queuedToCut,
                IncomingUnit = _incomingUnit,
                IsAllowed = this.IsAllowed,
                IsClickDisabled = this.IsClickDisabled,
            };
        }

        public virtual void RestoreState(object data)
        {
            var stateData = (Data)data;
            _guid = stateData.GUID;
            Prefab = stateData.Prefab;
            transform.position = stateData.Position;
            _growingResourceData = stateData.GrowingResourceData;
            _assignedTaskRefs = stateData.AssignedTaskRefs;
            _queuedToCut = stateData.QueuedToCut; 
            _incomingUnit = stateData.IncomingUnit;
            IsAllowed = stateData.IsAllowed;
            IsClickDisabled = stateData.IsClickDisabled;
        }

        public struct Data
        {
            public string GUID;
            public GameObject Prefab;
            public Vector3 Position;
            public GrowingResourceData GrowingResourceData;
            public List<int> AssignedTaskRefs;
            public bool QueuedToCut;
            public UnitTaskAI IncomingUnit;
            public bool IsAllowed;
            public bool IsClickDisabled;

            public GrowingResource.GrowingData GrowingData;
        }
    }
}
