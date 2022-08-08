using Characters;
using Items;
using Tasks;
using UnityEngine;

namespace Actions
{
    [CreateAssetMenu(fileName = "ActionMine", menuName ="Actions/Mine", order = 50)]
    public class ActionMine : ActionBase
    {
        public override TaskBase CreateTask(Interactable requestor, bool autoAssign = true)
        {
            requestor.DisplayTaskIcon(Icon);
            
            var task = new MiningTask.Mine()
            {
                RequestorUID = requestor.UniqueId,
                TaskAction = this,
                OnTaskAccepted = requestor.OnTaskAccepted,
                claimMountain = (UnitTaskAI unitTaskAI) =>
                {
                    requestor.IncomingUnitUID = unitTaskAI.UniqueId;
                },
                mountainPosition = requestor.transform.position,
                workAmount = requestor.GetWorkAmount(),
                OnCompleteTask = () =>
                {
                    OnTaskComplete(requestor);
                }
            };

            if (autoAssign)
            {
                taskMaster.MiningTaskSystem.AddTask(task);
            }

            return task;
        }

        public override void CancelTask(Interactable requestor)
        {
            taskMaster.MiningTaskSystem.CancelTask(requestor.UniqueId);
        }

        public override void OnTaskComplete(Interactable requestor)
        {
            requestor.OnTaskCompleted(this);
            var mountain = requestor.GetComponent<Mountain>();
            mountain.MineMountain();
            requestor.DisplayTaskIcon(null);
        }
    }
}
