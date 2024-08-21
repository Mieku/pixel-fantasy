using Characters;
using Items;
using Managers;
using NodeCanvas.Framework;
using Systems.Appearance.Scripts;

namespace AI.Action_Tasks
{
    public class ExtractResourceAction : KinlingActionTask
    {
        public BBParameter<string> RequesterUID;
        public BBParameter<string> KinlingUID;
        
        private float _timer;
        private Kinling kinling;
        private BasicResource resource;

        protected override void OnExecute()
        {
            kinling = KinlingsDatabase.Instance.GetKinling(KinlingUID.value);
            PlayerInteractable requester = PlayerInteractableDatabase.Instance.Query(RequesterUID.value);
            resource = (BasicResource) requester;
            kinling.Avatar.SetUnitAction(resource.GetExtractActionAnim(), kinling.TaskHandler.GetActionDirection(resource.transform.position));
        }

        protected override void OnUpdate()
        {
            _timer += TimeManager.Instance.DeltaTime;
            if(_timer >= 1) 
            {   
                _timer = 0;
                if (resource.DoExtractionWork(kinling.RuntimeData.Stats, out float progress)) 
                {
                    // When work is complete
                    EndAction(true);
                }
                else
                {
                    kinling.DisplayWorkProgress(progress);
                }
            }
        }

        protected override void OnStopInternal(bool interrupt)
        {
            kinling.Avatar.SetUnitAction(UnitAction.Nothing);
            _timer = 0;
            kinling.HideWorkProgress();
            kinling = null;
            resource = null;
        }
    }
}
