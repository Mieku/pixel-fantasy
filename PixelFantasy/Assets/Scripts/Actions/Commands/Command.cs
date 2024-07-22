using AI;
using Characters;
using Managers;
using TaskSystem;
using UnityEngine;
using Task = TaskSystem.Task;

[CreateAssetMenu(fileName = "Command", menuName ="Commands/Create Command")]
public class Command : ScriptableObject
{
    public string Name;
    public Sprite Icon;
    public Task Task; // TODO: Old remove
    public EToolType RequiredToolType => Task.RequiredToolType; // TODO: old remove

    public TaskSettings TaskSettings;

    public bool CanDoCommand(Kinling kinling, PlayerInteractable interactable)
    {
        TaskAI taskAI = kinling.TaskAI;
        
        // Has the action?
        var action = taskAI.FindTaskActionFor(Task);
        if (action == null) return false;
        
        // Check if there is a possible tool to use, if needed and not held
        if (Task.RequiredToolType != EToolType.None)
        {
            if (!taskAI.HasToolTypeEquipped(Task.RequiredToolType))
            {
                bool foundTool = InventoryManager.Instance.HasToolType(Task.RequiredToolType);
                return foundTool;
            }
        }

        var usagePos = interactable.UseagePosition(kinling.transform.position);
        if (usagePos == null) return false;

        return true;
    }
}
