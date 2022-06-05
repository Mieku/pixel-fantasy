using System.Collections.Generic;
using DataPersistence;
using Items;
using UnityEngine;

namespace Handlers
{
    public class ItemsHandler : Saveable
    {
        protected override string StateName => "Items";
        public override int LoadOrder => 1;

        [SerializeField] private GameObject _itemPrefab;

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
                var data = (Item.Data)childState;
                var childObj = Instantiate(_itemPrefab, transform);
                childObj.GetComponent<IPersistent>().RestoreState(data);
            }
        }
        
        public Item GetItemByGUID(string guid)
        {
            var children = GetPersistentChildren();
            foreach (var child in children)
            {
                var item = child.GetComponent<Item>();
                if (item != null && item.GUID == guid)
                {
                    return item;
                }
            }
            
            Debug.LogError($"Slot with GUID {guid} not found!");
            return null;
        }
    }
}
