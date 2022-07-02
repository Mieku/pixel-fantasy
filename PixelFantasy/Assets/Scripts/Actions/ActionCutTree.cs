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
                workAmount = requestor.GetWorkAmount(),
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

        public override void OnTaskComplete(Interactable requestor)
        {
            requestor.OnTaskCompleted(this);
            var growingResource = requestor.GetComponent<GrowingResource>();
            growingResource.CutDownPlant();
            requestor.DisplayTaskIcon(null);
        }
    }
}
