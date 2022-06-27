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
        
        public override void OnTaskComplete(Interactable requestor)
        {
            requestor.OnTaskCompleted(this);
            var structure = requestor.GetComponent<Structure>();
            if (structure != null)
            {
                structure.CompleteConstruction();
            }
            var floor = requestor.GetComponent<Floor>();
            if (floor != null)
            {
                floor.CompleteConstruction();
            }
            var furniture = requestor.GetComponent<Furniture>();
            if (furniture != null)
            {
                floor.CompleteConstruction();
            }
        }

        private float GetWorkPerResource(Interactable requestor)
        {
            float workPerResource = 0;
            var structure = requestor.GetComponent<Structure>();
            if (structure != null)
            {
                workPerResource = structure.GetWorkPerResource();
            }
            var floor = requestor.GetComponent<Floor>();
            if (floor != null)
            {
                workPerResource = floor.GetWorkPerResource();
            }
            var furniture = requestor.GetComponent<Furniture>();
            if (furniture != null)
            {
                workPerResource = furniture.GetWorkPerResource();
            }

            return workPerResource;
        }
    }
}
