using AI;
using Characters;
using ScriptableObjects;
using TaskSystem;
using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(fileName = "Command", menuName ="Commands/Create Command")]
public class Command : ScriptableObject
{
    [FormerlySerializedAs("Name")] public string CommandID;
    public string DisplayName;
    public Sprite Icon;

    public string TaskID;
    public ETaskType TaskType;

    public AI.Task Task => TasksDatabase.Instance.QueryTask(TaskID);

    public bool CanDoCommand(Kinling kinling, PlayerInteractable interactable)
    {
        TaskHandler taskHandler = kinling.TaskHandler;
        
        // Has the Behaviour Tree?
        var hasBT = taskHandler.HasTreeForTask(TaskID);
        if (!hasBT) return false;

        var usagePos = interactable.UseagePosition(kinling.transform.position);
        if (usagePos == null) return false;

        return true;
    }
}
