using Characters;
using Items;
using Tasks;
using UnityEngine;

namespace Actions
{
    [CreateAssetMenu(fileName = "ActionHarvestFruit", menuName ="Actions/HarvestFruit", order = 50)]
    public class ActionHarvestFruit : ActionBase
    {
        public override TaskBase CreateTask(Interactable requestor, bool autoAssign = true)
        {
            requestor.DisplayTaskIcon(Icon);

            var task = new FarmingTask.HarvestFruit()
            {
                RequestorUID = requestor.UniqueId,
                TaskAction = this,
                OnTaskAccepted = requestor.OnTaskAccepted,
                claimPlant = (UnitTaskAI unitTaskAI) =>
                {
                    requestor.IncomingUnitUID = unitTaskAI.UniqueId;
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

        /// <summary>
        /// If the task should be shown to player, for example can't harvest fruit if no fruit
        /// </summary>
        public override bool IsTaskAvailable(Interactable requestor)
        {
            var growingResource = requestor.GetComponent<GrowingResource>();
            return growingResource.HasFruitAvailable;
        }
        
        public override void OnTaskComplete(Interactable requestor)
        {
            requestor.IncomingUnitUID = "";
            requestor.OnTaskCompleted(this);
            var growingResource = requestor.GetComponent<GrowingResource>();
            growingResource.HarvestFruit();
            requestor.DisplayTaskIcon(null);
        }
        
        public override void CancelTask(Interactable requestor)
        {
            taskMaster.FarmingTaskSystem.CancelTask(requestor.UniqueId);
        }
    }
}
