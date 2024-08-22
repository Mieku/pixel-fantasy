using AI;
using Characters;
using TaskSystem;
using UnityEngine;

[CreateAssetMenu(fileName = "Command", menuName ="Commands/Create Command")]
public class Command : ScriptableObject
{
    public string CommandID;
    public string DisplayName;
    public Sprite Icon;

    public string TaskID;
    public ETaskType TaskType;

    public AI.Task Task => TasksDatabase.Instance.QueryTask(TaskID);

    public bool CanDoCommand(Kinling kinling, PlayerInteractable interactable)
    {
        TaskHandler taskHandler = kinling.TaskHandler;

        if (!kinling.RuntimeData.Stats.CanDoTaskType(TaskType) ||
            kinling.RuntimeData.TaskPriorities.GetPriorityByTaskType(TaskType).Priority == ETaskPriority.Ignore)
        {
            return false;
        }
        
        // Has the Behaviour Tree?
        var hasBT = taskHandler.HasTreeForTask(TaskID);
        if (!hasBT) return false;

        var usagePos = interactable.UseagePosition(kinling.transform.position);
        if (usagePos == null) return false;

        return true;
    }
}
