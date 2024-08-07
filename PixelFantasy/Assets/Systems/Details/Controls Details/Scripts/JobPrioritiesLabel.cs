using TaskSystem;
using UnityEngine;

namespace Systems.Details.Controls_Details.Scripts
{
    public class JobPrioritiesLabel : MonoBehaviour
    {
        public ETaskType TaskType;
        public JobPrioritiesMenu Menu;

        public void OnHoverStart()
        {
            Menu.HighlightTaskType(TaskType, true);
        }

        public void OnHoverEnd()
        {
            Menu.HighlightTaskType(TaskType, false);
        }
    }
}
