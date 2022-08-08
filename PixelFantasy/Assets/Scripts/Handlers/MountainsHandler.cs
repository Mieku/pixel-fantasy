using System.Collections.Generic;
using DataPersistence;
using Items;
using UnityEngine;

namespace Handlers
{
    public class MountainsHandler : Saveable
    {
        [SerializeField] private GameObject _mountainPrefab;
        
        protected override string StateName => "Mountains";
        public override int LoadOrder => 1;

        protected override void ClearChildStates(List<object> childrenStates)
        {
            // Delete current persistent children
            var currentChildren = GetPersistentChildren();
            foreach (var child in currentChildren)
            {
                child.GetComponent<UID>().RemoveUID();
            }
            
            foreach (var child in currentChildren)
            {
                Destroy(child);
            }
            currentChildren.Clear();
        }
        
        protected override void SetChildStates(List<object> childrenStates)
        {
            // Instantiate all the children in data, Trigger RestoreState with their state data
            foreach (var childState in childrenStates)
            {
                var resourceData = (Mountain.State)childState;
                var childObj = Instantiate(_mountainPrefab, transform);
                childObj.GetComponent<IPersistent>().RestoreState(resourceData);
            }
        }
    }
}
