using System;
using Newtonsoft.Json;
using ScriptableObjects;
using UnityEngine.InputSystem;

[Serializable]
public class KeyBindData
{
    public SavedKeyBind SetGameSpeed_Pause = new SavedKeyBind("Player/SetGameSpeed_Paused");
    public SavedKeyBind SetGameSpeed_Normal = new SavedKeyBind("Player/SetGameSpeed_Play");
    public SavedKeyBind SetGameSpeed_Fast = new SavedKeyBind("Player/SetGameSpeed_Fast");
    public SavedKeyBind SetGameSpeed_VeryFast = new SavedKeyBind("Player/SetGameSpeed_Fastest");
    
    public SavedKeyBind MoveCamera_Up = new SavedKeyBind("Player/Move Camera Up");
    public SavedKeyBind MoveCamera_Down = new SavedKeyBind("Player/Move Camera Down");
    public SavedKeyBind MoveCamera_Left = new SavedKeyBind("Player/Move Camera Left");
    public SavedKeyBind MoveCamera_Right = new SavedKeyBind("Player/Move Camera Right");

    public void LoadSavedKeyBinds()
    {
        SetGameSpeed_Pause.ApplyBinding();
        SetGameSpeed_Normal.ApplyBinding();
        SetGameSpeed_Fast.ApplyBinding();
        SetGameSpeed_VeryFast.ApplyBinding();
        
        MoveCamera_Up.ApplyBinding();
        MoveCamera_Down.ApplyBinding();
        MoveCamera_Left.ApplyBinding();
        MoveCamera_Right.ApplyBinding();
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
            default:
                throw new ArgumentOutOfRangeException(nameof(bindAction), bindAction, null);
        }
    }
    
    private void SetKeyBindPath(EKeyBindAction bindAction, bool isAlternate, string path)
    {
        switch (bindAction)
        {
            case EKeyBindAction.SetGameSpeed_Paused:
                SetGameSpeed_Pause.SetBinding(path, isAlternate);
                break;
            case EKeyBindAction.SetGameSpeed_Normal:
                SetGameSpeed_Normal.SetBinding(path, isAlternate);
                break;
            case EKeyBindAction.SetGameSpeed_Fast:
                SetGameSpeed_Fast.SetBinding(path, isAlternate);
                break;
            case EKeyBindAction.SetGameSpeed_VeryFast:
                SetGameSpeed_VeryFast.SetBinding(path, isAlternate);
                break;
            case EKeyBindAction.MoveCamera_Up:
                MoveCamera_Up.SetBinding(path, isAlternate);
                break;
            case EKeyBindAction.MoveCamera_Down:
                MoveCamera_Down.SetBinding(path, isAlternate);
                break;
            case EKeyBindAction.MoveCamera_Left:
                MoveCamera_Left.SetBinding(path, isAlternate);
                break;
            case EKeyBindAction.MoveCamera_Right:
                MoveCamera_Right.SetBinding(path, isAlternate);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(bindAction), bindAction, null);
        }
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

    public void BeginListeningForKeyBind(EKeyBindAction bindAction, bool isAlternate,
        Action<InputActionRebindingExtensions.RebindingOperation> onComplete)
    {
        var action = GetSavedKeyBind(bindAction).InputAction;
        int index = 0;
        if (isAlternate) index = 1;
        
        action.Disable();
        action.PerformInteractiveRebinding(index)
            .WithControlsExcluding("<Mouse>/position") // Exclude mouse position if desired
            .OnMatchWaitForAnother(0.1f) // Wait a moment for a complete match
            .OnComplete((operation) =>
            {
                action.Enable();
                var newPath = action.bindings[index].effectivePath;
                SetKeyBindPath(bindAction, isAlternate, newPath);
                onComplete.Invoke(operation);
            })
            .Start();
    }
}

[Serializable]
public class SavedKeyBind
{
    public string ActionName;
    public string BindingPath;
    public string AltBindingPath;

    public SavedKeyBind(string actionName)
    {
        ActionName = actionName;
    }

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

    public void ApplyBinding()
    {
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
}
