using Characters;
using Managers;
using ScriptableObjects;
using TaskSystem;
using UnityEngine;

[CreateAssetMenu(fileName = "Command", menuName ="Commands/Create Command")]
public class Command : ScriptableObject
{
    public string Name;
    public Sprite Icon;
    public Task Task;
    public JobData Job => Task.Job;
    public EToolType RequiredToolType => Task.RequiredToolType;

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
                bool foundTool = false;
                        
                if (kinling.AssignedWorkplace != null)
                {
                    foundTool = InventoryManager.Instance.HasToolTypeBuilding(Task.RequiredToolType,
                        kinling.AssignedWorkplace);
                }
                        
                if (!foundTool && kinling.AssignedHome != null)
                {
                    foundTool = InventoryManager.Instance.HasToolTypeBuilding(Task.RequiredToolType,
                        kinling.AssignedHome);
                }

                if (!foundTool)
                {
                    foundTool = InventoryManager.Instance.HasToolTypeGlobal(Task.RequiredToolType);
                }

                if (!foundTool)
                {
                    return false;
                }
            }
        }

        var usagePos = interactable.UseagePosition(kinling.transform.position);
        if (usagePos == null) return false;

        return true;
    }
}
