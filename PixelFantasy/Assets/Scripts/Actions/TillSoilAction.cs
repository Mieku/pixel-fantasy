using Gods;
using SGoap;
using UnityEngine;
using Zones;

namespace Actions
{
    public class TillSoilAction : BasicAction
    {
        public AssignedInteractableSensor AssignedInteractableSensor;

        private Crop _crop;
        private float _timer;

        private const float WORK_SPEED = 1f; // TODO: Get the work speed from the Kinling's stats
        private const float WORK_AMOUNT = 1f; // TODO: Get the amount of work from the Kinling's stats
        
        public override bool PrePerform()
        {
            _crop = AssignedInteractableSensor.GetInteractable() as Crop;
            
            return base.PrePerform();
        }

        public override EActionStatus Perform()
        {
            if (_crop == null)
            {
                Debug.LogError($"{name} failed");
                return EActionStatus.Failed;
            }
            
            AgentData.Animator.SetUnitAction(UnitAction.Digging, AssignedInteractableSensor.GetActionDirection());

            _timer += TimeManager.Instance.DeltaTime;
            if(_timer >= WORK_SPEED) 
            {
                _timer = 0;
                if (_crop.DoTillingWork(WORK_AMOUNT)) 
                {
                    // When work is complete
                    AgentData.Animator.SetUnitAction(UnitAction.Nothing,
                        AssignedInteractableSensor.GetActionDirection());
                    return EActionStatus.Success;
                } 
            }
            
            return EActionStatus.Running;
        }

        public override bool PostPerform()
        {
            return base.PostPerform();
        }
    }
}
