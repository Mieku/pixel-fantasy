using Characters;
using Items;
using Managers;
using NodeCanvas.Framework;
using Systems.Appearance.Scripts;

namespace AI.Action_Tasks
{
    public class ExtractResourceAction : ActionTask
    {
        public BBParameter<PlayerInteractable> Requester;
        public BBParameter<string> KinlingUID;
        
        private float _timer;
        private Kinling kinling;
        private BasicResource resource;

        protected override void OnExecute()
        {
            kinling = KinlingsDatabase.Instance.GetKinling(KinlingUID.value);
            resource = (BasicResource) Requester.value;
            kinling.Avatar.SetUnitAction(resource.GetExtractActionAnim(), kinling.TaskHandler.GetActionDirection(resource.transform.position));
        }

        protected override void OnUpdate()
        {
            _timer += TimeManager.Instance.DeltaTime;
            if(_timer >= 1) 
            {   
                _timer = 0;
                if (resource.DoExtractionWork(kinling.RuntimeData.Stats)) 
                {
                    // When work is complete
                    EndAction(true);
                } 
            }
        }

        protected override void OnStop()
        {
            kinling.Avatar.SetUnitAction(UnitAction.Nothing);
            _timer = 0;
            kinling = null;
            resource = null;
        }
    }
}
