using System;
using System.Collections;
using System.Collections.Generic;
using Actions;
using Characters;
using Gods;
using Items;
using ScriptableObjects;
using SGoap;
using TaskSystem;
using UnityEngine;

public abstract class Interactable : UniqueObject
{
    [SerializeField] private SpriteRenderer _icon;
    public List<Command> Commands = new List<Command>();
    public Command PendingCommand;

// Old
    public List<int> QueuedTaskRefs = new List<int>();
    public List<ActionBase> PendingTasks = new List<ActionBase>();
    public List<ActionBase> InProgressTasks = new List<ActionBase>();
    public string IncomingUnitUID;

    private List<ActionBase> _potentialActions;
    
    public List<ActionBase> AvailableActions
    {
        get
        {
            if (_potentialActions == null)
            {
                var resource = GetComponent<Resource>();
                if (resource != null)
                {
                    //_potentialActions = resource.GetResourceData().AvailableActions;
                }

                var item = GetComponent<Item>();
                if (item != null)
                {
                    _potentialActions = item.GetItemData().AvailableActions;
                }
                
                var structure = GetComponent<Structure>();
                if (structure != null)
                {
                    _potentialActions = structure.GetConstructionData().AvailableActions;
                }
                
                var floor = GetComponent<Floor>();
                if (floor != null)
                {
                    _potentialActions = floor.FloorData.AvailableActions;
                }
                
                var furniture = GetComponent<Furniture>();
                if (furniture != null)
                {
                    _potentialActions = furniture.FurnitureData.AvailableActions;
                }
                
                var door = GetComponent<Door>();
                if (door != null)
                {
                    _potentialActions = door.GetConstructionData().AvailableActions;
                }

                var mountain = GetComponent<Mountain>();
                if (mountain != null)
                {
                    _potentialActions = mountain.GetMountainData().AvailableActions;
                }
            }

            var availableActions = FilterAvailableActions(_potentialActions);

            return availableActions;
        }
    }

    public List<ActionBase> CancellableActions()
    {
        var results = new List<ActionBase>();
        foreach (var pendingTask in PendingTasks)
        {
            results.Add(pendingTask);
        }

        foreach (var inProgressTask in InProgressTasks)
        {
            results.Add(inProgressTask);
        }

        return results;
    }

    private List<ActionBase> FilterAvailableActions(List<ActionBase> potentialActions)
    {
        // List<ActionBase> result = new List<ActionBase>();
        // foreach (var potentialAction in potentialActions)
        // {
        //     if (potentialAction.IsTaskAvailable(this))
        //     {
        //         result.Add(potentialAction);
        //     }
        // }
        //
        // return result;
        return null;
    }

    public void OnTaskAccepted(ActionBase task)
    {
        SetTaskToAccepted(task);
    }

    public void OnTaskCompleted(ActionBase task)
    {
        InProgressTasks.Remove(task);
    }

    public void CreateTaskById(string actionId)
    {
        var action = Librarian.Instance.GetAction(actionId);
        action.CreateTask(this);
        SetTaskToPending(action);
    }
    
    public void CreateTask(ActionBase taskAction)
    {
        CancelAllTasks();
        taskAction.CreateTask(this);
        SetTaskToPending(taskAction);
    }

    public void CreateTask(Command command)
    {
        // Only one command can be active
        if (PendingCommand != null)
        {
            CancelCommand(PendingCommand);
        }

        Task task = new Task
        {
            Category = command.Task.Category,
            TaskId = command.Task.TaskId,
            Requestor = this
        };

        PendingCommand = command;
        TaskManager.Instance.AddTask(task);
        DisplayTaskIcon(command.Icon);
    }

    public void CancelCommand(Command command)
    {
        PendingCommand = null;
        
        Task task = new Task
        {
            Category = command.Task.Category,
            TaskId = command.Task.TaskId,
            Requestor = this
        };
        
        TaskManager.Instance.CancelTask(task);
        DisplayTaskIcon(null);
    }

    public bool IsPending(Command command)
    {
        if (PendingCommand == null) return false;
        
        return PendingCommand == command;
    }
    
    public void CancelAllTasks()
    {
        List<ActionBase> tasksToCancel = new List<ActionBase>();
        foreach (var pendingTask in PendingTasks)
        {
            tasksToCancel.Add(pendingTask);
        }
        foreach (var inProgressTask in InProgressTasks)
        {
            tasksToCancel.Add(inProgressTask);
        }

        foreach (var taskToCancel in tasksToCancel)
        {
            CancelTask(taskToCancel);
        }

        foreach (var queuedTaskRef in QueuedTaskRefs)
        {
            CancelQueuedTask(queuedTaskRef);
        }
    }

    public void CancelQueuedTask(int taskRef)
    {
        TaskMaster.Instance.CancelQueuedTask(taskRef);
    }

    public void CancelTask(ActionBase taskAction)
    {
        taskAction.CancelTask(this);
        
        PendingTasks.Clear();
        InProgressTasks.Clear();

        if (!string.IsNullOrEmpty(IncomingUnitUID))
        {
            var unitObj = UIDManager.Instance.GetGameObject(IncomingUnitUID);
            var unit = unitObj.GetComponent<UnitTaskAI>();
            unit.CancelTask();
        }
        
        DisplayTaskIcon(null);
    }

    public void SetTaskToPending(ActionBase task)
    {
        PendingTasks.Add(task);
    }
        
    public void SetTaskToAccepted(ActionBase task)
    {
        PendingTasks.Remove(task);
        InProgressTasks.Add(task);
    }

    public bool IsActionActive(ActionBase action)
    {
        if (PendingTasks.Contains(action))
        {
            return true;
        }

        if (InProgressTasks.Contains(action))
        {
            return true;
        }

        return false;
    }

    protected void RestoreTasks(List<ActionBase> pendingTasks)
    {
        foreach (var pendingTask in pendingTasks)
        {
            SetTaskToPending(pendingTask);
            pendingTask.RestoreTask(this);
        }
    }

    public virtual int GetWorkAmount()
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

    public virtual void ReceiveItem(Item item)
    {
        Debug.LogError($"Item unexpectely received: {item.name}");
    }
}
