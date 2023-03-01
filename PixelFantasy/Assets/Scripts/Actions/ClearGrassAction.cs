using Gods;
using SGoap;

namespace Actions
{
    public class ClearGrassAction : BasicAction
    {
        public AssignedInteractableSensor AssignedInteractableSensor;
        
        private DirtTile _dirtTile;
        private float _timer;

        private const float WORK_SPEED = 1f; // TODO: Get the work speed from the Kinling's stats
        private const float WORK_AMOUNT = 1f; // TODO: Get the amount of work from the Kinling's stats

        public override bool PrePerform()
        {
            _dirtTile = AssignedInteractableSensor.GetInteractable() as DirtTile;

            return base.PrePerform();
        }

        public override EActionStatus Perform()
        {
            if (_dirtTile == null)
            {
                return EActionStatus.Failed;
            }
            
            AgentData.Animator.SetUnitAction(UnitAction.Digging, AssignedInteractableSensor.GetActionDirection());

            _timer += TimeManager.Instance.DeltaTime;
            if(_timer >= WORK_SPEED) 
            {
                _timer = 0;
                if (_dirtTile.DoWork(WORK_AMOUNT)) 
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
