using System.Collections.Generic;
using DataPersistence;
using Items;
using UnityEngine;

namespace Handlers
{
    public class FurnitureHandler : Saveable
    {
        protected override string StateName => "Furniture";
        public override int LoadOrder => 1;
        
        [SerializeField] private GameObject _furniturePrefab;
        [SerializeField] private GameObject _craftingTablePrefab;

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
                var data = (Furniture.Data)childState;

                if (data.IsCraftingTable)
                {
                    var childObj = Instantiate(_craftingTablePrefab, transform);
                    childObj.GetComponent<IPersistent>().RestoreState(data);
                }
                else
                {
                    var childObj = Instantiate(_furniturePrefab, transform);
                    childObj.GetComponent<IPersistent>().RestoreState(data);
                }
                
            }
        }
    }
}
