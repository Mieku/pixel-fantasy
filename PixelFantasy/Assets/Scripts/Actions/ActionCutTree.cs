using System;
using System.Xml;
using Characters;
using Gods;
using Items;
using Tasks;
using UnityEngine;

namespace Actions
{
    [CreateAssetMenu(fileName = "Action", menuName ="Actions/CutTree", order = 50)]
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
                    // treeRequestor._incomingUnit = unitTaskAI;
                    // treeRequestor.PendingTask = TaskType.None;
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
                TaskMaster.Instance.FellingTaskSystem.AddTask(task);
            }

            return task;
        }

        public void OnTaskComplete(Interactable requestor)
        {
            requestor.OnTaskCompleted(this);
            var growingResource = requestor.GetComponent<GrowingResource>();
            growingResource.CutDownPlant();
            requestor.DisplayTaskIcon(null);
        }

        public override TaskBase RestoreTask(Interactable requestor, bool autoAssign = true)
        {
            return CreateTask(requestor, autoAssign);
        }
    }
}
