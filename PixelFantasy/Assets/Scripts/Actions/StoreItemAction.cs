
using Items;
using SGoap;
using UnityEngine;
using Zones;

namespace Actions
{
    public class StoreItemAction : BasicAction
    {
        public AssignedInteractableSensor AssignedInteractableSensor;

        private Item _item;
        private StorageSlot _storageSlot;
        private bool _isHoldingItem;
        
        public float DistanceToItem => Vector2.Distance(_item.transform.position, transform.position);
        public float DistanceToSlot => Vector2.Distance(_storageSlot.transform.position, transform.position);

        public override bool PrePerform()
        {
            _isHoldingItem = false;
            _item = AssignedInteractableSensor.GetInteractable() as Item;
            _storageSlot = _item.AssignedStorageSlot;
            
            return base.PrePerform();
        }

        public override EActionStatus Perform()
        {
            if (_storageSlot == null)
            {
                Debug.Log("Failed");
                return EActionStatus.Failed;
            }
            
            // Pick Up Item
            if (DistanceToItem <= 1f && !_isHoldingItem)
            {
                _isHoldingItem = true;
                AgentData.KinlingAgent.HoldItem(_item);
                _item.SetHeld(true);
                
                return EActionStatus.Running;
            } 
            
            // Deposit Item
            if (DistanceToSlot <= 1f && _isHoldingItem)
            {
                _isHoldingItem = false;
                _storageSlot.StoreItem(_item);
                _item.AddItemToSlot();
                
                return EActionStatus.Success;
            }
            
            if (!_isHoldingItem) // Go to the item
            {
                AgentData.NavMeshAgent.SetDestination(_item.transform.position);
                return EActionStatus.Running;
            } 
            
            if (_isHoldingItem) // Go to the slot
            {
                AgentData.NavMeshAgent.SetDestination(_storageSlot.transform.position);
                return EActionStatus.Running;
            }
            
            return EActionStatus.Running;
        }

        public override bool PostPerform()
        {
            
            return base.PostPerform();
        }
    }
}
