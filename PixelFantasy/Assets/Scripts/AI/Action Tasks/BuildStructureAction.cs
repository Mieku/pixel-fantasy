using Characters;
using Interfaces;
using Items;
using Managers;
using NodeCanvas.Framework;
using Systems.Appearance.Scripts;
using Systems.Buildings.Scripts;

namespace AI.Action_Tasks
{
    public class BuildStructureAction : KinlingActionTask
    {
        public BBParameter<string> RequesterUID;
        public BBParameter<string> KinlingUID;
        
        private float _timer;
        private Kinling _kinling;
        private IConstructable _construction;

        protected override void OnExecute()
        {
            _kinling = KinlingsDatabase.Instance.GetKinling(KinlingUID.value);
            PlayerInteractable requester = PlayerInteractableDatabase.Instance.Query(RequesterUID.value);
            _construction = (IConstructable) requester;
            _kinling.Avatar.SetUnitAction(UnitAction.Swinging, _kinling.TaskHandler.GetActionDirection(requester.transform.position));
        }

        protected override void OnUpdate()
        {
            _timer += TimeManager.Instance.DeltaTime;
            if(_timer >= 1) 
            {   
                _timer = 0;
                if (_construction.DoConstruction(_kinling.RuntimeData.Stats)) 
                {
                    // When work is complete
                    EndAction(true);
                } 
            }
        }

        protected override void OnStopInternal(bool interrupt)
        {
            _kinling.Avatar.SetUnitAction(UnitAction.Nothing);
            _timer = 0;
            _kinling = null;
            _construction = null;
        }
    }
}
