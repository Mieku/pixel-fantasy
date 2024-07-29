using System;
using System.Collections.Generic;
using UnityEngine;

namespace AI.Task_Actions
{
    [CreateAssetMenu(fileName = "LocateAndClaimItems", menuName = "TaskActions/LocateAndClaimItems")]
    public class LocateAndClaimItems : TaskAction
    {
        public override bool IsActionPossible(string kinlingID, Task task)
        {
            // Implement logic to check if resources can be located and claimed
            return true; // Simplified for example
        }

        public override void ExecuteAction(string kinlingID, Task task)
        {
            
        }

        public override void CancelAction(string kinlingID, Task task)
        {
            // Implement any cleanup logic needed when this action is canceled
            Debug.Log("LocateAndClaimResources action canceled.");
        }
    }
}