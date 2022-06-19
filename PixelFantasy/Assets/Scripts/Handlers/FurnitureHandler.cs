using System.Collections.Generic;
using DataPersistence;
using UnityEngine;

namespace Handlers
{
    public class FurnitureHandler : Saveable
    {
        protected override string StateName => "Furniture";
        public override int LoadOrder => 1;
        
        [SerializeField] private GameObject _furniturePrefab;
        [SerializeField] private GameObject _craftingTablePrefab;
        
        protected override void SetChildStates(List<object> childrenStates)
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

            // Instantiate all the children in data, Trigger RestoreState with their state data
            foreach (var childState in childrenStates)
            {
                // var data = (Item.Data)childState;
                // var childObj = Instantiate(_itemPrefab, transform);
                // childObj.GetComponent<IPersistent>().RestoreState(data);
            }
        }
    }
}
