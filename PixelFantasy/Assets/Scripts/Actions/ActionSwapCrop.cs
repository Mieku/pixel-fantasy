using System;
using Characters;
using Tasks;
using UnityEngine;
using Zones;

namespace Actions
{
    [CreateAssetMenu(fileName = "ActionSwapCrop", menuName ="Actions/SwapCrop", order = 50)]
    public class ActionSwapCrop : ActionBase
    {
        public override TaskBase CreateTask(Interactable requestor, bool autoAssign = true)
        {
            var task = new FarmingTask.SwapCrop()
            {
                RequestorUID = requestor.UniqueId,
                TaskAction = this,
                OnTaskAccepted = requestor.OnTaskAccepted,
                claimCrop = (UnitTaskAI unitTaskAI) =>
                {
                    requestor.IncomingUnitUID = unitTaskAI.UniqueId;
                },
                cropPosition = Helper.ConvertMousePosToGridPos(requestor.transform.position),
                OnWork = (float amount, Action onWorkCompleted) =>
                {
                    OnWorkDone(requestor, amount, onWorkCompleted);
                },
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
        
        public void OnTaskComplete(Interactable requestor, Action onWorkCompleted)
        {
            requestor.OnTaskCompleted(this);

            requestor.IncomingUnitUID = "";
            var crop = requestor.GetComponent<Crop>();
            crop.CropSwapped();
            onWorkCompleted.Invoke();
        }
        
        public void OnWorkDone(Interactable requestor, float amount, Action onWorkCompleted)
        {
            var crop = requestor.GetComponent<Crop>();
            if (crop != null)
            {
                float remainingWork = crop.PlantingWorkDone(amount);
                if (remainingWork <= 0)
                {
                    OnTaskComplete(requestor, onWorkCompleted);
                }
            }
        }
        
        public override void CancelTask(Interactable requestor)
        {
            taskMaster.FarmingTaskSystem.CancelTask(requestor.UniqueId);
        }
    }
}
