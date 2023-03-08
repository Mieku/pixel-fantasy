using System;
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
                taskMaster.ConstructionTaskSystem.AddTask(task);
            }

            return task;
        }

        public override bool IsTaskAvailable(Interactable requestor)
        {
            // This is an automatic task
            return false;
        }
        
        public override void CancelTask(Interactable requestor)
        {
            taskMaster.ConstructionTaskSystem.CancelTask(requestor.UniqueId);
        }

        public void OnTaskComplete(Interactable requestor, Action onWorkCompleted)
        {
            requestor.OnTaskCompleted(this);
            var construction = requestor.GetComponent<Construction>();
            if (construction != null)
            {
                construction.CompleteConstruction();
            }
            onWorkCompleted.Invoke();
        }
        
        public void OnWorkDone(Interactable requestor, float amount, Action onWorkCompleted)
        {
            var construction = requestor.GetComponent<Construction>();
            if (construction != null)
            {
                float remainingWork = construction.WorkDone(amount);
                if (remainingWork <= 0)
                {
                    OnTaskComplete(requestor, onWorkCompleted);
                }
            }
        }
    }
}
