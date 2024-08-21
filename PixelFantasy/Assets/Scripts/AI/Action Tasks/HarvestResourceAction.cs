using Characters;
using Items;
using Managers;
using NodeCanvas.Framework;
using Systems.Appearance.Scripts;

namespace AI.Action_Tasks
{
    public class HarvestResourceAction : KinlingActionTask
    {
        public BBParameter<string> RequesterUID;
        public BBParameter<string> KinlingUID;
        
        private float _timer;
        private Kinling _kinling;
        private GrowingResource _resource;

        protected override void OnExecute()
        {
            _kinling = KinlingsDatabase.Instance.GetKinling(KinlingUID.value);
            PlayerInteractable requester = PlayerInteractableDatabase.Instance.Query(RequesterUID.value);
            _resource = (GrowingResource) requester;
            _kinling.Avatar.SetUnitAction(UnitAction.Doing, _kinling.TaskHandler.GetActionDirection(_resource.transform.position));
        }

        protected override void OnUpdate()
        {
            _timer += TimeManager.Instance.DeltaTime;
            if(_timer >= 1) 
            {   
                _timer = 0;
                if (_resource.DoHarvest(_kinling.RuntimeData.Stats, out float progress)) 
                {
                    // When work is complete
                    EndAction(true);
                }
                else
                {
                    _kinling.DisplayWorkProgress(progress);
                }
            }
        }

        protected override void OnStopInternal(bool interrupt)
        {
            _kinling.Avatar.SetUnitAction(UnitAction.Nothing);
            _timer = 0;
            _kinling.HideWorkProgress();
            _kinling = null;
            _resource = null;
        }
    }
}
