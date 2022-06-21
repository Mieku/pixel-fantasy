using System;
using System.Collections;
using System.Collections.Generic;
using Actions;
using Gods;
using Items;
using ScriptableObjects;
using UnityEngine;

public class Interactable : UniqueObject
{
    [SerializeField] private SpriteRenderer _icon;
    
    public List<ActionBase> PendingTasks = new List<ActionBase>();
    public List<ActionBase> InProgressTasks = new List<ActionBase>();

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
                    _potentialActions = resource.GetResourceData().AvailableActions;
                }

                var item = GetComponent<Item>();
                if (item != null)
                {
                    _potentialActions = item.GetItemData().AvailableActions;
                }
            }

            var availableActions = FilterAvailableActions(_potentialActions);

            return availableActions;
        }
    }

    private List<ActionBase> FilterAvailableActions(List<ActionBase> potentialActions)
    {
        List<ActionBase> result = new List<ActionBase>();
        foreach (var potentialAction in potentialActions)
        {
            if (potentialAction.IsTaskAvailable(this))
            {
                result.Add(potentialAction);
            }
        }

        return result;
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
            SetTaskToPending(pendingTask);
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
