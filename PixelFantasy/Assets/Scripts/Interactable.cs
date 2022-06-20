using System;
using System.Collections;
using System.Collections.Generic;
using Actions;
using UnityEngine;

public class Interactable : UniqueObject
{
    [SerializeField] private SpriteRenderer _icon;

    public List<ActionBase> AvailableActions;
    
    public List<ActionBase> PendingTasks = new List<ActionBase>();
    public List<ActionBase> InProgressTasks = new List<ActionBase>();

    public void OnTaskAccepted(ActionBase task)
    {
        SetTaskToAccepted(task);
    }

    public void OnTaskCompleted(ActionBase task)
    {
        InProgressTasks.Remove(task);
    }
    
    public void CreateTask(ActionBase taskAction)
    {
        taskAction.CreateTask(this);
        SetTaskToPending(taskAction);
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
            pendingTask.RestoreTask(this);
        }
    }

    public virtual int GetWorkAmount()
    {
        return 0;
    }

    public void DisplayTaskIcon(Sprite icon)
    {
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
}
