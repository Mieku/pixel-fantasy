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
    [SerializeField] private GameObject _selectedIcon;
    
    protected bool _isSelected;
    
    public abstract List<SpriteRenderer> SpritesToOutline { get; }
    public abstract string UniqueID { get; }
    public abstract string DisplayName { get; }
    public abstract string PendingTaskUID { get; set; }
    
    /// <summary>
    /// This is for situations where you want to check if a pi is similar, for example when double-clicking to select all similar
    /// </summary>
    public abstract bool IsSimilar(PlayerInteractable otherPI);

    protected static bool _isQuitting = false;

    [FormerlySerializedAs("_workDisplay")] [SerializeField] private CommandDisplay _commandDisplay;
    
    [FormerlySerializedAs("Commands")] [SerializeField] private List<Command> _commands = new List<Command>();
    
    [JsonIgnore] public Task PendingTask => TasksDatabase.Instance.QueryTask(PendingTaskUID);

    [JsonIgnore]
    public Command PendingCommand
    {
        get
        {
            if (string.IsNullOrEmpty(PendingTaskUID)) return null;
            
            return _commands.Find(c => c.TaskID == PendingTask.TaskID);
        }
    }

    public void AddCommand(string cmdID, bool firstOnList = false)
    {
         if (_commands.Exists(c => c.CommandID == cmdID)) return; // dont add twice
        
        Command cmd = GameSettings.Instance.LoadCommand(cmdID);
        if (cmd != null)
        {
            if (firstOnList)
            {
                _commands.Insert(0, cmd);
            }
            else
            {
                _commands.Add(cmd);
            }
        }
    }

    public void RemoveCommand(string cmdID)
    {
        if (_commands.Exists(c => c.CommandID == cmdID))
        {
            Command cmd = GameSettings.Instance.LoadCommand(cmdID);
            if(cmd != null) _commands.Remove(cmd);
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

    public void RefreshTaskIcon()
    {
        _commandDisplay.DisplayCommand(PendingCommand);
    }

    protected void AssignTaskIcon(Command command)
    {
        if (command != null)
        {
            _commandDisplay.DisplayCommand(command);
        }
        else
        {
            _commandDisplay.DisplayCommand(null);
        }
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

    public List<Command> GetCommands()
    {
        return new List<Command>(_commands);
    }

    public Action OnChanged;
    public Action<PlayerInteractable> OnDestroyed;

    public void InformChanged()
    {
        OnChanged?.Invoke();
    }

    protected virtual void OnDestroy()
    {
        if (!_isQuitting)
        {
            OnDestroyed?.Invoke(this);
        }
    }

    public bool IsSelected
    {
        get => _isSelected;
        set
        {
            _isSelected = value;

            if (value)
            {
                OnSelection();
            }
            else
            {
                OnDeselection();
            }
        }
    }

    protected virtual void OnSelection()
    {
        if (_selectedIcon != null)
        {
            //_selectedIcon.SetActive(true);
        }

        var colour = GameSettings.Instance.OutlineColour;
        ShowOutline(colour);
    }

    protected virtual void OnDeselection()
    {
        if (_selectedIcon != null)
        {
            //_selectedIcon.SetActive(false);
        }
        
        HideOutline();
    }

    public virtual bool ObjectValidForCommandSelection(Command command)
    {
        return _commands.Contains(command);
    }

    public virtual int GetStackSize()
    {
        return 1;
    }

    public virtual bool IsForbidden()
    {
        return false;
    }
    
    public void CancelRequesterTasks(bool requeueTask)
    {
        TasksDatabase.Instance.CancelRequesterTasks(this, requeueTask);
    }

    public void ShowOutline(Color outlineColour)
    {
        if(SpritesToOutline == null) return;
        
        MaterialPropertyBlock mpb = new MaterialPropertyBlock();
        mpb.SetColor("_OutlineColour", outlineColour);
        mpb.SetInt("_ShowOutline", 1);

        foreach (var sprite in SpritesToOutline)
        {
            mpb.SetTexture("_MainTex", sprite.sprite.texture);
            sprite.SetPropertyBlock(mpb);
        }
    }

    public void HideOutline()
    {
        if(SpritesToOutline == null) return;
        
        MaterialPropertyBlock mpb = new MaterialPropertyBlock();
        mpb.SetInt("_ShowOutline", 0);
        
        foreach (var sprite in SpritesToOutline)
        {
            if(sprite == null) continue;
            
            mpb.SetTexture("_MainTex", sprite.sprite.texture);
            sprite.SetPropertyBlock(mpb);
        }
    }
}
