using System;
using Newtonsoft.Json;
using ScriptableObjects;
using UnityEngine;
using UnityEngine.InputSystem;

[Serializable]
public class KeyBindData
{
    public SavedKeyBind SetGameSpeed_Pause = new SavedKeyBind();
    public SavedKeyBind SetGameSpeed_Normal = new SavedKeyBind();
    public SavedKeyBind SetGameSpeed_Fast = new SavedKeyBind();
    public SavedKeyBind SetGameSpeed_VeryFast = new SavedKeyBind();
    
    public SavedKeyBind MoveCamera_Up = new SavedKeyBind();
    public SavedKeyBind MoveCamera_Down = new SavedKeyBind();
    public SavedKeyBind MoveCamera_Left = new SavedKeyBind();
    public SavedKeyBind MoveCamera_Right = new SavedKeyBind();

    public SavedKeyBind Cancel = new SavedKeyBind();
    
    public SavedKeyBind RotateClockwise = new SavedKeyBind();
    public SavedKeyBind RotateCounterClockwise = new SavedKeyBind();
    public SavedKeyBind HideUI = new SavedKeyBind();
    public SavedKeyBind TakeScreenshot = new SavedKeyBind();
    
    private InputActionRebindingExtensions.RebindingOperation _currentRebindingOperation;

    public void Init()
    {
        SetGameSpeed_Pause ??= new SavedKeyBind();
        SetGameSpeed_Normal ??= new SavedKeyBind();
        SetGameSpeed_Fast ??= new SavedKeyBind();
        SetGameSpeed_VeryFast ??= new SavedKeyBind();
        MoveCamera_Up ??= new SavedKeyBind();
        MoveCamera_Down ??= new SavedKeyBind();
        MoveCamera_Left ??= new SavedKeyBind();
        MoveCamera_Right ??= new SavedKeyBind();
        RotateClockwise ??= new SavedKeyBind();
        RotateCounterClockwise ??= new SavedKeyBind();
        Cancel ??= new SavedKeyBind();
        HideUI ??= new SavedKeyBind();
        TakeScreenshot ??= new SavedKeyBind();
    }

    public void LoadSavedKeyBinds()
    {
        SetGameSpeed_Pause.ApplyBinding("Gameplay/SetGameSpeed_Paused");
        SetGameSpeed_Normal.ApplyBinding("Gameplay/SetGameSpeed_Play");
        SetGameSpeed_Fast.ApplyBinding("Gameplay/SetGameSpeed_Fast");
        SetGameSpeed_VeryFast.ApplyBinding("Gameplay/SetGameSpeed_Fastest");
        
        MoveCamera_Up.ApplyBinding("Gameplay/MoveCameraUp");
        MoveCamera_Down.ApplyBinding("Gameplay/MoveCameraDown");
        MoveCamera_Left.ApplyBinding("Gameplay/MoveCameraLeft");
        MoveCamera_Right.ApplyBinding("Gameplay/MoveCameraRight");
        
        RotateClockwise.ApplyBinding("Gameplay/RotateClockwise");
        RotateCounterClockwise.ApplyBinding("Gameplay/RotateCounterClockwise");
        Cancel.ApplyBinding("Gameplay/Cancel");
        
        HideUI.ApplyBinding("Gameplay/HideUI");
        TakeScreenshot.ApplyBinding("Gameplay/TakeScreenshot");
        
        RefreshInputActions();
    }

    public void ResetKeyBinds()
    {
        SetGameSpeed_Pause.ResetBinding();
        SetGameSpeed_Normal.ResetBinding();
        SetGameSpeed_Fast.ResetBinding();
        SetGameSpeed_VeryFast.ResetBinding();
        
        MoveCamera_Up.ResetBinding();
        MoveCamera_Down.ResetBinding();
        MoveCamera_Left.ResetBinding();
        MoveCamera_Right.ResetBinding();
        
        Cancel.ResetBinding();
        RotateClockwise.ResetBinding();
        RotateCounterClockwise.ResetBinding();
        
        HideUI.ResetBinding();
        TakeScreenshot.ResetBinding();
        
        RefreshInputActions();
    }
    
    private void RefreshInputActions()
    {
        var actions = GameSettings.Instance.InputActions;
        actions.Disable(); // Disable all actions to reset them
        actions.Enable();  // Re-enable all actions to apply changes
    }

    private SavedKeyBind GetSavedKeyBind(EKeyBindAction bindAction)
    {
        switch (bindAction)
        {
            case EKeyBindAction.SetGameSpeed_Paused:
                return SetGameSpeed_Pause;
            case EKeyBindAction.SetGameSpeed_Normal:
                return SetGameSpeed_Normal;
            case EKeyBindAction.SetGameSpeed_Fast:
                return SetGameSpeed_Fast;
            case EKeyBindAction.SetGameSpeed_VeryFast:
                return SetGameSpeed_VeryFast;
            case EKeyBindAction.MoveCamera_Up:
                return MoveCamera_Up;
            case EKeyBindAction.MoveCamera_Down:
                return MoveCamera_Down;
            case EKeyBindAction.MoveCamera_Left:
                return MoveCamera_Left;
            case EKeyBindAction.MoveCamera_Right:
                return MoveCamera_Right;
            case EKeyBindAction.Cancel:
                return Cancel;
            case EKeyBindAction.RotateClockwise:
                return RotateClockwise;
            case EKeyBindAction.RotateCounterClockwise:
                return RotateCounterClockwise;
            case EKeyBindAction.HideUI:
                return HideUI;
            case EKeyBindAction.TakeScreenshot:
                return TakeScreenshot;
            default:
                throw new ArgumentOutOfRangeException(nameof(bindAction), bindAction, null);
        }
    }
    
    private void SetKeyBindPath(EKeyBindAction bindAction, bool isAlternate, string path)
    {
        var bind = GetSavedKeyBind(bindAction);
        bind.SetBinding(path, isAlternate);
    }

    public string GetKeyBindText(EKeyBindAction bindAction, bool isAlternate)
    {
        var action = GetSavedKeyBind(bindAction).InputAction;
        if(action == null) return "None";

        if (isAlternate)
        {
            if (action.bindings.Count > 1)
            {
                var result = action.GetBindingDisplayString(1);
                if (string.IsNullOrEmpty(result)) return "None";
                
                return action.GetBindingDisplayString(1);
            }
                
            return "None";
        }
            
        return action.GetBindingDisplayString(0);
    }

    public void CancelKeyBindListening()
    {
        if (_currentRebindingOperation != null)
        {
            _currentRebindingOperation.Cancel(); // Trigger the OnCancel callback
            _currentRebindingOperation = null; // Clear the reference
        }
    }
    
    public void BeginListeningForKeyBind(EKeyBindAction bindAction, bool isAlternate,
        Action<InputActionRebindingExtensions.RebindingOperation> onComplete, Action<InputActionRebindingExtensions.RebindingOperation> onCancel, Action<InputAction> onKeyBindExists)
    {
        CancelKeyBindListening();
        
        var cancelPath = Cancel.InputAction.bindings[0].effectivePath;
        var action = GetSavedKeyBind(bindAction).InputAction;
        int index = 0;
        if (isAlternate) index = 1;
        
        action.Disable();
        _currentRebindingOperation = action.PerformInteractiveRebinding(index)
            .WithControlsExcluding("<Pointer>/press")
            .WithControlsExcluding("<Mouse>/leftButton")
            .WithControlsExcluding("<Mouse>/position") // Exclude mouse position if desired
            .WithCancelingThrough(cancelPath)
            .OnMatchWaitForAnother(0.1f) // Wait a moment for a complete match
            .OnComplete((operation) =>
            {
                _currentRebindingOperation = null;
                action.Enable();
                var newPath = action.bindings[index].effectivePath;
                
                var (isInUse, conflictingAction) = IsKeyBindInUse(newPath, action);
                if (isInUse)
                {
                    onKeyBindExists?.Invoke(conflictingAction);
                    
                    // Cancel the Key Bind
                    action.RemoveBindingOverride(index);
                    operation.Dispose();
                    BeginListeningForKeyBind(bindAction, isAlternate, onComplete, onCancel, onKeyBindExists);
                    return;
                }
                
                SetKeyBindPath(bindAction, isAlternate, newPath);
                RefreshInputActions();
                onComplete?.Invoke(operation);
            })
            .OnCancel(operation =>
            {
                _currentRebindingOperation = null;
                operation.Dispose();
                onCancel?.Invoke(operation);
            })
            .Start();
    }
    
    private (bool, InputAction) IsKeyBindInUse(string newPath, InputAction currentAction)
    {
        var map = currentAction.actionMap;
        
        foreach (var action in map.actions)
        {
            // Skip checking the current action to avoid self-conflict
            if (action == currentAction)
                continue;

            // Check each binding in the action
            foreach (var binding in action.bindings)
            {
                if (binding.effectivePath == newPath)
                {
                    return (true, action); // Conflict found, return the conflicting action's name
                }
            }
        }
        
        return (false, null); // No conflicts found
    }
}

[Serializable]
public class SavedKeyBind
{
    public string ActionName;
    public string BindingPath;
    public string AltBindingPath;
    
    [JsonIgnore]
    public InputAction InputAction => GameSettings.Instance.InputActions.FindAction(ActionName);

    private bool HasSavedKeyBinds()
    {
        if (string.IsNullOrEmpty(BindingPath) && string.IsNullOrEmpty(AltBindingPath))
        {
            return false;
        }

        return true;
    }

    public void ApplyBinding(string defaultActionName)
    {
        if (string.IsNullOrEmpty(ActionName))
        {
            ActionName = defaultActionName;
        }
        
        if(!HasSavedKeyBinds()) return;

        if (!string.IsNullOrEmpty(BindingPath))
        {
            InputAction.ApplyBindingOverride(0, BindingPath);
        }
            
        if (!string.IsNullOrEmpty(AltBindingPath))
        {
            InputAction.ApplyBindingOverride(1, AltBindingPath);
        }
    }

    public void ResetBinding()
    {
        BindingPath = null;
        AltBindingPath = null;
        InputAction.RemoveAllBindingOverrides();
    }

    public void SetBinding(string path, bool isAlternate)
    {
        if (isAlternate)
        {
            AltBindingPath = path;
        }
        else
        {
            BindingPath = path;
        }
    }
}

[Serializable]
public enum EKeyBindAction
{
    SetGameSpeed_Paused,
    SetGameSpeed_Normal,
    SetGameSpeed_Fast,
    SetGameSpeed_VeryFast,
    MoveCamera_Up,
    MoveCamera_Down,
    MoveCamera_Left,
    MoveCamera_Right,
    Cancel,
    RotateClockwise,
    RotateCounterClockwise,
    HideUI,
    TakeScreenshot,
}
