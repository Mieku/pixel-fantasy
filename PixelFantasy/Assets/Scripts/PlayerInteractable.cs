using System;
using System.Collections.Generic;
using AI;
using HUD;
using Newtonsoft.Json;
using UnityEngine;

public abstract class PlayerInteractable : MonoBehaviour
{
    public abstract string UniqueID { get; }
    public abstract string PendingTaskUID { get; set; }

    protected static bool _isQuitting = false;

    [SerializeField] private WorkDisplay _workDisplay;
    
    public List<Command> Commands = new List<Command>();
    
    [JsonIgnore] public Task PendingTask => TasksDatabase.Instance.QueryTask(PendingTaskUID);

    [JsonIgnore]
    public Command PendingCommand
    {
        get
        {
            if (string.IsNullOrEmpty(PendingTaskUID)) return null;
            
            return Commands.Find(c => c.TaskID == PendingTask.TaskID);
        }
    }

    public void CancelPlayerCommand(Command command = null)
    {
        if (command != null)
        {
            if (!IsPending(command)) return;
        }
        
        CancelPendingTask();
    }
    
    public virtual void AssignCommand(Command command)
    {
        if (command.CommandID == "Cancel Command")
        {
            CancelPendingTask();
            return;
        }
        
        if (IsPending(command)) return;

        // Only one command can be active
        if (PendingTask != null)
        {
            CancelPendingTask();
        }

        CreateTask(command);
    }

    public void CreateTask(Command command)
    {
        if (command.CommandID == "Cancel Command")
        {
            CancelPendingTask();
            return;
        }
        
        if (IsPending(command)) return;
        
        // Only one command can be active
        if (PendingTask != null)
        {
            CancelPendingTask();
        }

        Task task = new Task(command.TaskID, command.DisplayName, command.TaskType, this);
        
        TasksDatabase.Instance.AddTask(task);
        AddTaskToPending(task);
        
        RefreshTaskIcon(0);
    }

    public void AddTaskToPending(Task task)
    {
        PendingTaskUID = task.UniqueID;
    }

    public virtual void OnTaskComplete(Task task, bool success)
    {
        if (success)
        {
            PendingTaskUID = null;
        }
    }

    public virtual void OnTaskCancelled(Task task)
    {
        RefreshTaskIcon(0);
    }

    public virtual void CancelPendingTask()
    {
        var task = TasksDatabase.Instance.QueryTask(PendingTaskUID);
        if (task != null)
        {
            task.Cancel();
        }

        PendingTaskUID = null;
        RefreshTaskIcon(0);
    }

    public bool IsPending(Command command)
    {
        if (PendingTask == null) return false;
        
        return PendingTask.TaskID == command.TaskID;
    }
    
    public virtual float GetWorkAmount()
    {
        return 1;
    }

    public void RefreshTaskIcon(float progress)
    {
        _workDisplay.DisplayCommand(PendingCommand);
        _workDisplay.SetProgress(progress);
    }

    public virtual void ReceiveItem(ItemData item)
    {
        Debug.LogError($"Item unexpectedly received: {item.Settings.ItemName}");
    }

    public abstract Vector2? UseagePosition(Vector2 requestorPosition);

    private void OnApplicationQuit()
    {
        _isQuitting = true;
    }
}
