using System;
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
        
        public void OnTaskComplete(Interactable requestor, Action onWorkCompleted)
        {
            requestor.OnTaskCompleted(this);
            var construction = requestor.GetComponent<Construction>();
            construction.CompleteDeconstruction();
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

        public override bool IsTaskAvailable(Interactable requestor)
        {
            var construction = requestor.GetComponent<Construction>();
            return construction.IsBuilt;
        }
        
        public override void CancelTask(Interactable requestor)
        {
            taskMaster.ConstructionTaskSystem.CancelTask(requestor.UniqueId);
        }
    }
}
