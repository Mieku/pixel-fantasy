using System;
using System.Collections.Generic;
using DataPersistence;
using Items;
using UnityEngine;

namespace Handlers
{
    public class ResourcesHandler : Saveable
    {
        protected override string StateName => "Resources";
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
                var resourceData = (Resource.Data)childState;
                var childObj = Instantiate(resourceData.Prefab, transform);
                childObj.GetComponent<IPersistent>().RestoreState(resourceData);
            }
        }
    }
}
