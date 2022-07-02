using Characters;
using Items;
using Tasks;
using UnityEngine;

namespace Actions
{
    [CreateAssetMenu(fileName = "ActionDeconstruct", menuName ="Actions/Deconstruct", order = 50)]
    public class ActionDeconstruct : ActionBase
    {
        public override TaskBase CreateTask(Interactable requestor, bool autoAssign = true)
        {
            requestor.DisplayTaskIcon(Icon);
            var construction = requestor.GetComponent<Construction>();
            construction.IsDeconstructing = true;
            
            var task = new ConstructionTask.DeconstructStructure()
            {
                RequestorUID = requestor.UniqueId,
                TaskAction = this,
                OnTaskAccepted = requestor.OnTaskAccepted,
                claimStructure = (UnitTaskAI unitTaskAI) =>
                {
                    construction.IncomingUnit = unitTaskAI;
                },
                structurePosition = requestor.transform.position,
                workAmount = construction.GetWorkPerResource(),
                OnCompleteTask = () =>
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
            var construction = requestor.GetComponent<Construction>();
            construction.CompleteDeconstruction();
        }

        public override bool IsTaskAvailable(Interactable requestor)
        {
            var construction = requestor.GetComponent<Construction>();
            return construction.IsBuilt;
        }
    }
}
