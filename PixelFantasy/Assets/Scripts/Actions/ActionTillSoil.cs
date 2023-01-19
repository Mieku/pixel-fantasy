using System;
using Characters;
using Items;
using Tasks;
using UnityEngine;
using Zones;

namespace Actions
{
    [CreateAssetMenu(fileName = "ActionTillSoil", menuName ="Actions/TillSoil", order = 50)]
    public class ActionTillSoil : ActionBase
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
                    requestor.IncomingUnitUID = unitTaskAI.UniqueId;
                },
                grassPosition = requestor.transform.position,
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
            requestor.IncomingUnitUID = "";
            requestor.OnTaskCompleted(this);
            var crop = requestor.GetComponent<Crop>();
            crop.OnDirtDug();
            onWorkCompleted.Invoke();
        }
        
        public void OnWorkDone(Interactable requestor, float amount, Action onWorkCompleted)
        {
            var crop = requestor.GetComponent<Crop>();
            if (crop != null)
            {
                float remainingWork = crop.TillWorkDone(amount);
                if (remainingWork <= 0)
                {
                    OnTaskComplete(requestor, onWorkCompleted);
                }
            }
        }
    }
}
