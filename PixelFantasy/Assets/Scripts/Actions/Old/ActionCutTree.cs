using System;
using System.Xml;
using Characters;
using Gods;
using Items;
using Tasks;
using UnityEngine;

namespace Actions
{
    [CreateAssetMenu(fileName = "ActionCutTree", menuName ="Actions/CutTree", order = 50)]
    public class ActionCutTree : ActionBase
    {
        public override TaskBase CreateTask(Interactable requestor, bool autoAssign = true)
        {
            requestor.DisplayTaskIcon(Icon);
            
            var task = new FellingTask.CutTree()
            {
                RequestorUID = requestor.UniqueId,
                TaskAction = this,
                OnTaskAccepted = requestor.OnTaskAccepted,
                claimTree = (UnitTaskAI unitTaskAI) =>
                {
                    requestor.IncomingUnitUID = unitTaskAI.UniqueId;
                },
                treePosition = requestor.transform.position,
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
                taskMaster.FellingTaskSystem.AddTask(task);
            }

            return task;
        }

        public override void CancelTask(Interactable requestor)
        {
            taskMaster.FellingTaskSystem.CancelTask(requestor.UniqueId);
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
            var treeResource = requestor.GetComponent<TreeResource>();
            if (treeResource != null)
            {
                float remainingWork = treeResource.CutWorkDone(amount);
                if (remainingWork <= 0)
                {
                    OnTaskComplete(requestor, onWorkCompleted);
                }
            }
        }
    }
}
