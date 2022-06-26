using System.Collections.Generic;
using DataPersistence;
using Items;
using UnityEngine;

namespace Handlers
{
    public class StructuresHandler : Saveable
    {
        protected override string StateName => "Structures";
        public override int LoadOrder => 1;

        [SerializeField] private GameObject _structurePrefab;

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
                var data = (Structure.Data)childState;
                var childObj = Instantiate(_structurePrefab, transform);
                childObj.GetComponent<IPersistent>().RestoreState(data);
            }
        }
    }
}
