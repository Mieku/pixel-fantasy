using System.Xml;
using Characters;
using Items;
using Tasks;
using UnityEngine;

namespace Actions
{
    [CreateAssetMenu(fileName = "ActionCutPlant", menuName ="Actions/CutPlant", order = 50)]
    public class ActionCutPlant : ActionBase
    {
        public override TaskBase CreateTask(Interactable requestor, bool autoAssign = true)
        {
            requestor.DisplayTaskIcon(Icon);

            var task = new FarmingTask.CutPlant()
            {
                RequestorUID = requestor.UniqueId,
                TaskAction = this,
                OnTaskAccepted = requestor.OnTaskAccepted,
                claimPlant = (UnitTaskAI unitTaskAI) =>
                {
                    //_incomingUnit = unitTaskAI;
                },
                plantPosition = requestor.transform.position,
                workAmount = requestor.GetWorkAmount(),
                OnCompleteTask = () =>
                {
                    OnTaskComplete(requestor);
                }
            };

            if (autoAssign)
            {
                taskMaster.FarmingTaskSystem.AddTask(task);
            }

            return task;
        }
        
        public override void OnTaskComplete(Interactable requestor)
        {
            requestor.OnTaskCompleted(this);
            var growingResource = requestor.GetComponent<GrowingResource>();
            growingResource.CutDownPlant();
            requestor.DisplayTaskIcon(null);
        }
    }
}
