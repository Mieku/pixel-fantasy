using System;
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
                    requestor.IncomingUnitUID = unitTaskAI.UniqueId;
                },
                plantPosition = requestor.transform.position,
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
        
        public override void CancelTask(Interactable requestor)
        {
            taskMaster.FarmingTaskSystem.CancelTask(requestor.UniqueId);
        }
        
        public void OnTaskComplete(Interactable requestor, Action onWorkCompleted)
        {
            requestor.OnTaskCompleted(this);
            var growingResource = requestor.GetComponent<GrowingResource>();
            growingResource.CutDownPlant();
            requestor.DisplayTaskIcon(null);
            onWorkCompleted.Invoke();
        }
        
        public void OnWorkDone(Interactable requestor, float amount, Action onWorkCompleted)
        {
            var growingResource = requestor.GetComponent<GrowingResource>();
            if (growingResource != null)
            {
                float remainingWork = growingResource.CutWorkDone(amount);
                if (remainingWork <= 0)
                {
                    OnTaskComplete(requestor, onWorkCompleted);
                }
            }
        }
    }
}
