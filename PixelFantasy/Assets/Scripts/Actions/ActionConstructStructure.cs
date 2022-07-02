using Items;
using Tasks;
using UnityEngine;

namespace Actions
{
    [CreateAssetMenu(fileName = "ActionConstructStructure", menuName ="Actions/ConstructStructure", order = 50)]
    public class ActionConstructStructure : ActionBase
    {
        public override TaskBase CreateTask(Interactable requestor, bool autoAssign = true)
        {
            requestor.DisplayTaskIcon(Icon);

            var task = new ConstructionTask.ConstructStructure
            {
                RequestorUID = requestor.UniqueId,
                TaskAction = this,
                OnTaskAccepted = requestor.OnTaskAccepted,
                structurePosition = requestor.transform.position,
                workAmount = GetWorkPerResource(requestor),
                completeWork = () =>
                {
                    OnTaskComplete(requestor);
                }
            };

            if (autoAssign)
            {
                taskMaster.ConstructionTaskSystem.AddTask(task);
            }

            return task;
        }

        public override bool IsTaskAvailable(Interactable requestor)
        {
            // This is an automatic task
            return false;
        }

        public override void OnTaskComplete(Interactable requestor)
        {
            requestor.OnTaskCompleted(this);
            var construction = requestor.GetComponent<Construction>();
            if (construction != null)
            {
                construction.CompleteConstruction();
            }
        }

        private float GetWorkPerResource(Interactable requestor)
        {
            float workPerResource = 0;
            var construction = requestor.GetComponent<Construction>();
            if (construction != null)
            {
                workPerResource = construction.GetWorkPerResource();
            }

            return workPerResource;
        }
    }
}
