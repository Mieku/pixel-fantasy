using Characters;
using Items;
using Tasks;
using UnityEngine;

namespace Actions
{
    [CreateAssetMenu(fileName = "ActionHarvestCrop", menuName ="Actions/HarvestCrop", order = 50)]
    public class ActionHarvestCrop : ActionBase
    {
        public override TaskBase CreateTask(Interactable requestor, bool autoAssign = true)
        {
            var task = new FarmingTask.HarvestCrop()
            {
                RequestorUID = requestor.UniqueId,
                TaskAction = this,
                OnTaskAccepted = requestor.OnTaskAccepted,
                claimCrop = (UnitTaskAI unitTaskAI) =>
                {
                    requestor.IncomingUnitUID = unitTaskAI.UniqueId;
                },
                cropPosition = Helper.ConvertMousePosToGridPos(requestor.transform.position),
                workAmount = 8,
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
            requestor.IncomingUnitUID = "";
            var crop = requestor.GetComponent<Crop>();
            crop.CropHarvested();
        }
        
        public override void CancelTask(Interactable requestor)
        {
            taskMaster.FarmingTaskSystem.CancelTask(requestor.UniqueId);
        }
    }
}
