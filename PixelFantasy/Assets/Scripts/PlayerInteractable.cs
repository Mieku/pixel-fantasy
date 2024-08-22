using System;
using System.Collections.Generic;
using AI;
using HUD;
using Newtonsoft.Json;
using ScriptableObjects;
using UnityEngine;
using UnityEngine.Serialization;

public abstract class PlayerInteractable : MonoBehaviour
{
    public abstract string UniqueID { get; }
    public abstract string PendingTaskUID { get; set; }

    protected static bool _isQuitting = false;

    [FormerlySerializedAs("_workDisplay")] [SerializeField] private CommandDisplay _commandDisplay;
    
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

    public void AddCommand(string cmdID)
    {
        if (Commands.Exists(c => c.CommandID == cmdID)) return; // dont add twice
        
        Command cmd = GameSettings.Instance.LoadCommand(cmdID);
        Commands.Add(cmd);
    }

    public void RemoveCommand(string cmdID)
    {
        if (Commands.Exists(c => c.CommandID == cmdID))
        {
            Command cmd = GameSettings.Instance.LoadCommand(cmdID);
            Commands.Remove(cmd);
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

    public void AssignSpecificTask(Task task)
    {
        // Only one command can be active
        if (PendingTask != null)
        {
            CancelPendingTask();
        }
        
        AddTaskToPending(task);
        
        RefreshTaskIcon();
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
        
        RefreshTaskIcon();
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
        PendingTaskUID = null;
        
        RefreshTaskIcon();
    }

    public virtual void CancelPendingTask()
    {
        if (PendingTask != null)
        {
            PendingTask.Cancel();
        }

        PendingTaskUID = null;
        RefreshTaskIcon();
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

    public void RefreshTaskIcon()
    {
        _commandDisplay.DisplayCommand(PendingCommand);
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
