using Items;
using SGoap;
using UnityEngine;
using Zones;

namespace Actions
{
    public class WithdrawItemAction : BasicAction
    {
        public ColonyInventorySensor ColonyInventorySensor;
        public AssignedInteractableSensor AssignedInteractableSensor;

        private StorageSlot _storageSlot;
        private Interactable _requestor;
        private bool _isHoldingItem;
        private Item _item;
        
        public float DistanceToRequestor => Vector2.Distance(_requestor.transform.position, transform.position);
        public float DistanceToSlot => Vector2.Distance(_storageSlot.transform.position, transform.position);
        
        // Needs to know the requestor (Building)
        // Needs to know the Storage the item is
        // Needs to know the Item - Can get from the Storage Slot with GetItem();

        public override bool IsUsable()
        {
            var payload = AssignedInteractableSensor.GetPayload();
            if (_storageSlot != null || string.IsNullOrEmpty(payload))
            {
                return base.IsUsable();
            }

            _storageSlot = ColonyInventorySensor.FindItem(payload);
            return _storageSlot != null && base.IsUsable();

        }

        public override bool PrePerform()
        {
            _requestor = AssignedInteractableSensor.GetInteractable();
            _isHoldingItem = false;
            
            return base.PrePerform();
        }

        public override EActionStatus Perform()
        {
            // Pick Up Item
            if (!_isHoldingItem && DistanceToSlot <= 1f)
            {
                _isHoldingItem = true;
                _item = _storageSlot.GetItem();
                AgentData.KinlingAgent.HoldItem(_item);
                _item.SetHeld(true);
                
                return EActionStatus.Running;
            }
            
            // Drop Item Off
            if (_isHoldingItem && DistanceToRequestor <= 1f)
            {
                _isHoldingItem = false;
                
                _requestor.ReceiveItem(_item);

                return EActionStatus.Success;
            }
            
            // Move to Item
            if (!_isHoldingItem)
            {
                AgentData.NavMeshAgent.SetDestination(_storageSlot.transform.position);
                return EActionStatus.Running;
            }
            
            // Move to Requestor
            if (_isHoldingItem)
            {
                AgentData.NavMeshAgent.SetDestination(_requestor.transform.position);
                return EActionStatus.Running;
            }
            
            
            return base.Perform();
        }

        public override bool PostPerform()
        {
            _storageSlot = null;
            _requestor = null;
            _isHoldingItem = false;
            _item = null;
            
            return base.PostPerform();
        }
    }
}
