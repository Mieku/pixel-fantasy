using System.Collections.Generic;
using DataPersistence;
using Items;
using UnityEngine;

namespace Handlers
{
    public class StorageHandler : Saveable
    {
        protected override string StateName => "Storage";
        public override int LoadOrder => 1;

        [SerializeField] private GameObject _storageSlotPrefab;
        
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
                var data = (StorageSlot.Data)childState;
                var childObj = Instantiate(_storageSlotPrefab, transform);
                childObj.GetComponent<IPersistent>().RestoreState(data);
            }
        }

        public StorageSlot GetStorageSlotByGUID(string guid)
        {
            var children = GetPersistentChildren();
            foreach (var child in children)
            {
                var slot = child.GetComponent<StorageSlot>();
                if (slot != null && slot.GUID == guid)
                {
                    return slot;
                }
            }
            
            Debug.LogError($"Slot with GUID {guid} not found!");
            return null;
        }
    }
}
