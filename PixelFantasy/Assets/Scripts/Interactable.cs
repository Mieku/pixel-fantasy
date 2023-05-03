using System.Collections.Generic;
using Buildings;
using Gods;
using Items;
using TaskSystem;
using UnityEngine;

public abstract class Interactable : UniqueObject
{
    [SerializeField] private SpriteRenderer _icon;
    public List<Command> Commands = new List<Command>();
    public Command PendingCommand;
    
    public void CreateTask(Command command)
    {
        if (IsPending(command)) return;

        var zone = Helper.IsPositionInZone(transform.position);
        Building building = null;
        if (zone != null)
        {
            building = zone.Building;
        }

        // Only one command can be active
        if (PendingCommand != null)
        {
            CancelCommand(PendingCommand, building);
        }

        Task task = new Task
        {
            Category = command.Task.Category,
            TaskId = command.Task.TaskId,
            Requestor = this
        };

        PendingCommand = command;

        if (building == null)
        {
            TaskManager.Instance.AddTask(task);
        }
        else
        {
            building.BuildingTasks.AddTask(task);
        }
        
        DisplayTaskIcon(command.Icon);
    }

    public void CancelCommand(Command command, Building building = null)
    {
        PendingCommand = null;
        
        Task task = new Task
        {
            Category = command.Task.Category,
            TaskId = command.Task.TaskId,
            Requestor = this
        };

        if (building == null)
        {
            TaskManager.Instance.CancelTask(task);
        }
        else
        {
            building.BuildingTasks.CancelTask(task);
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
