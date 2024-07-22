using System;
using UnityEngine;

namespace AI.Task_Actions
{
    [CreateAssetMenu(fileName = "GoTo", menuName = "TaskActions/GoTo")]
    public class GoToAction : TaskAction
    {
        public override bool IsActionPossible(string kinlingID, Task task)
        {
            // Check for path

            return true;
        }

        public override void ExecuteAction(string kinlingID, Task task)
        {
            
        }

        public override void CancelAction(string kinlingID, Task task)
        {
            // Stop moving
        }
    }
}
