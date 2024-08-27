using AI;
using Characters;
using UnityEngine;

[CreateAssetMenu(fileName = "Command", menuName ="Commands/Create Command")]
public class Command : ScriptableObject
{
    public string CommandID;
    public string DisplayName;
    public Sprite Icon;
    public Color IconColour = new Color(0.996f, 0.784f, 0.141f, 1f);

    public string TaskID;
    public ETaskType TaskType;

    public Task Task => TasksDatabase.Instance.QueryTask(TaskID);

    public Color GetFadedColour()
    {
        return new Color(IconColour.r, IconColour.g, IconColour.b, 0.4f);
    }

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
