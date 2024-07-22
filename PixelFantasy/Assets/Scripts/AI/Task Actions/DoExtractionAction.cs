using UnityEngine;

namespace AI.Task_Actions
{
    [CreateAssetMenu(fileName = "DoExtraction", menuName = "TaskActions/DoExtraction")]
    public class DoExtractionAction : TaskAction
    {
        public override bool IsActionPossible(string kinlingID, Task task)
        {
            throw new System.NotImplementedException();
        }

        public override void ExecuteAction(string kinlingID, Task task)
        {
            throw new System.NotImplementedException();
        }

        public override void CancelAction(string kinlingID, Task task)
        {
            throw new System.NotImplementedException();
        }
    }
}
