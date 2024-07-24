using System.Collections.Generic;
using System.Linq;
using AI;
using TaskSystem;
using UnityEngine;
using Task = TaskSystem.Task;

public abstract class PlayerInteractable : MonoBehaviour
{
    public abstract string UniqueID { get; }
    
    [SerializeField] private SpriteRenderer _icon;
    public List<Command> Commands = new List<Command>();
    public Command PendingCommand;

    private string _requestedTaskID;

    public void CancelPlayerCommand(Command command = null)
    {
        if (command != null)
        {
            if (!IsPending(command)) return;
        }
        
        CancelPending();
        DisplayTaskIcon(null);
    }
    
    public void AssignPlayerCommand(Command command, object payload = null)
    {
        if (command.Name == "Cancel Command")
        {
            CancelPending();
            return;
        }
        
        if (IsPending(command)) return;

        // Only one command can be active
        if (PendingCommand != null)
        {
            CancelCommand(PendingCommand);
        }
        
        PendingCommand = command;
        
        DisplayTaskIcon(command.Icon);
    }
    
    public void CreateTask(Command command, object payload = null)
    {
        if (command.Name == "Cancel Command")
        {
            CancelPending();
            return;
        }
        
        if (IsPending(command)) return;
        
        // Only one command can be active
        if (PendingCommand != null)
        {
            CancelCommand(PendingCommand);
        }
        
        PendingCommand = command;

        AI.Task task = new AI.Task(command.TaskID, command.TaskType, this);
        
        TasksDatabase.Instance.AddTask(task);
        AddTaskToRequested(task);
        
        DisplayTaskIcon(command.Icon);
    }

    public void AddTaskToRequested(AI.Task task)
    {
        _requestedTaskID = task.UniqueID;
        task.OnCompletedCallback += OnTaskComplete;
    }

    public void OnTaskComplete(AI.Task task, bool success)
    {
        if (success)
        {
            _requestedTaskID = null;
        }
    }

    public void CancelRequestorTasks()
    {
        var task = TasksDatabase.Instance.QueryTask(_requestedTaskID);
        if (task != null)
        {
            task.Cancel(false);
        }

        _requestedTaskID = null;
    }

    public virtual void CancelCommand(Command command)
    {
        PendingCommand = null;
        
        
        var task = TasksDatabase.Instance.QueryTask(_requestedTaskID);
        if (task != null)
        {
            task.Cancel(false);
        }

        DisplayTaskIcon(null);
    }

    public bool IsPending(Command command)
    {
        if (PendingCommand == null) return false;
        
        return PendingCommand == command;
    }

    public void CancelPending()
    {
        if (PendingCommand != null)
        {
            CancelCommand(PendingCommand);
        }

        PendingCommand = null;
    }

    public virtual float GetWorkAmount()
    {
        return 1;
    }

    public void DisplayTaskIcon(Sprite icon)
    {
        if (_icon == null) return;
        
        if (icon == null)
        {
            _icon.sprite = null;
            _icon.gameObject.SetActive(false);
        }
        else
        {
            _icon.sprite = icon;
            _icon.gameObject.SetActive(true);
        }
    }

    public virtual void ReceiveItem(ItemData item)
    {
        Debug.LogError($"Item unexpectely received: {item.Settings.ItemName}");
    }

    public abstract Vector2? UseagePosition(Vector2 requestorPosition);
}
