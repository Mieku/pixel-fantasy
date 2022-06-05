using System.Collections.Generic;
using DataPersistence;
using Items;
using UnityEngine;

namespace Handlers
{
    public class FlooringHandler : Saveable
    {
        protected override string StateName => "Flooring";
        public override int LoadOrder => 1;
        
        [SerializeField] private GameObject _flooringPrefab;
        
        protected override void SetChildStates(List<object> childrenStates)
        {
            // Delete current persistent children
            var currentChildren = GetPersistentChildren();
            foreach (var child in currentChildren)
            {
                Destroy(child);
            }
            currentChildren.Clear();

            // Instantiate all the children in data, Trigger RestoreState with their state data
            foreach (var childState in childrenStates)
            {
                var data = (Floor.Data)childState;
                var childObj = Instantiate(_flooringPrefab, transform);
                childObj.GetComponent<IPersistent>().RestoreState(data);
            }
        }
    }
}
