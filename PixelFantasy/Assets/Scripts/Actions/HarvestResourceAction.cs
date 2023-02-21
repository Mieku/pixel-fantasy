using Gods;
using Items;
using SGoap;

namespace Actions
{
    public class HarvestResourceAction : BasicAction
    {
        public AssignedInteractableSensor AssignedInteractableSensor;
        
        private GrowingResource _resource;
        private float _timer;

        private const float WORK_SPEED = 1f; // TODO: Get the work speed from the Kinling's stats
        private const float WORK_AMOUNT = 1f; // TODO: Get the amount of work from the Kinling's stats

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
            
            AgentData.Animator.SetUnitAction(UnitAction.Doing, AssignedInteractableSensor.GetActionDirection());

            _timer += TimeManager.Instance.DeltaTime;
            if(_timer >= WORK_SPEED) 
            {
                _timer = 0;
                if (_resource.DoHarvest(WORK_AMOUNT)) 
                {
                    // When work is complete
                    AgentData.Animator.SetUnitAction(UnitAction.Nothing,
                        AssignedInteractableSensor.GetActionDirection());
                    return EActionStatus.Success;
                } 
            }
            
            return EActionStatus.Running;
        }
    }
}
