using System.Xml;
using Characters;
using Tasks;
using UnityEngine;

namespace Actions
{
    [CreateAssetMenu(fileName = "ActionClearGrass", menuName ="Actions/ClearGrass", order = 50)]
    public class ActionClearGrass : ActionBase
    {
        public override TaskBase CreateTask(Interactable requestor, bool autoAssign = true)
        {
            var task = new FarmingTask.ClearGrass()
            {
                RequestorUID = requestor.UniqueId,
                TaskAction = this,
                OnTaskAccepted = requestor.OnTaskAccepted,
                claimDirt = (UnitTaskAI unitTaskAI) =>
                {
                    //_incomingUnit = unitTaskAI;
                },
                grassPosition = requestor.transform.position,
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
            var growingResource = requestor.GetComponent<DirtTile>();
            growingResource.BuiltDirt();
        }
    }
}
