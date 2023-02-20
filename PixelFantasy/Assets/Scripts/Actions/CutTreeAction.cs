using Items;
using SGoap;
using UnityEngine;

namespace Actions
{
    public class CutTreeAction : BasicAction
    {
        public AssignedInteractableSensor AssignedInteractableSensor;
        
        private GrowingResource _resource;
        

        public override bool PrePerform()
        {
            _resource = AssignedInteractableSensor.GetInteractable() as GrowingResource;
            
            return base.PrePerform();
        }

        public override EActionStatus Perform()
        {
            if (_resource == null)
            {
                return EActionStatus.Failed;
            }
            
            Debug.Log($"Chop chop! {_resource.name}");
            
            return base.Perform();
        }
    }
}
