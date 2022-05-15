using System.Collections.Generic;
using Gods;
using Interfaces;
using ScriptableObjects;
using Unit;
using UnityEngine;

namespace Items
{
    public class Resource : MonoBehaviour
    {
        [SerializeField] protected GrowingResourceData _growingResourceData;
        [SerializeField] protected SpriteRenderer _spriteRenderer;
        [SerializeField] protected SpriteRenderer _icon;
        [SerializeField] private ClickObject _clickObject;
        
        protected TaskMaster taskMaster => TaskMaster.Instance;
        protected Spawner spawner => Spawner.Instance;
        protected List<int> _assignedTaskRefs = new List<int>();
        protected bool _queuedToCut;
        protected UnitTaskAI _incomingUnit;

        public GrowingResourceData GetResourceData()
        {
            return _growingResourceData;
        }
        
        public bool QueuedToCut => _queuedToCut;
        
        
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
    }
}
