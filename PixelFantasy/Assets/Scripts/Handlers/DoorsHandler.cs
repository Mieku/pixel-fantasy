using System.Collections.Generic;
using DataPersistence;
using Items;
using UnityEngine;

namespace Handlers
{
    public class DoorsHandler : Saveable
    {
        protected override string StateName => "Doors";
        public override int LoadOrder => 1;
        
        [SerializeField] private GameObject _doorPrefab;

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
                var structureData = (Door.Data)childState;
                var childObj = Instantiate(_doorPrefab, transform);
                childObj.GetComponent<IPersistent>().RestoreState(structureData);
            }
        }
    }
}
