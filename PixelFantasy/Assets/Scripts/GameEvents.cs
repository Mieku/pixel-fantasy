using System;
using Characters;
using Controllers;
using UnityEngine;
using Action = System.Action;

public class GameEvents : MonoBehaviour
{
    public static event Action<Vector3,PlayerInputState, bool> OnLeftClickDown;
    public static void Trigger_OnLeftClickDown( Vector3 mousePos, PlayerInputState inputState, bool isOverUI )
    {
        if( OnLeftClickDown != null ) OnLeftClickDown( mousePos, inputState, isOverUI );
    }
    
    public static event Action<Vector3,PlayerInputState, bool> OnRightClickDown;
    public static void Trigger_OnRightClickDown( Vector3 mousePos, PlayerInputState inputState, bool isOverUI )
    {
        if( OnRightClickDown != null ) OnRightClickDown( mousePos, inputState, isOverUI );
    }
    
    public static event Action<Vector3,PlayerInputState, bool> OnLeftClickHeld;
    public static void Trigger_OnLeftClickHeld( Vector3 mousePos, PlayerInputState inputState, bool isOverUI )
    {
        if( OnLeftClickHeld != null ) OnLeftClickHeld( mousePos, inputState, isOverUI );
    }
    
    public static event Action<Vector3,PlayerInputState, bool> OnRightClickHeld;
    public static void Trigger_OnRightClickHeld( Vector3 mousePos, PlayerInputState inputState, bool isOverUI )
    {
        if( OnRightClickHeld != null ) OnRightClickHeld( mousePos, inputState, isOverUI );
    }
    
    public static event Action<Vector3,PlayerInputState, bool> OnLeftClickUp;
    public static void Trigger_OnLeftClickUp( Vector3 mousePos, PlayerInputState inputState, bool isOverUI )
    {
        if( OnLeftClickUp != null ) OnLeftClickUp( mousePos, inputState, isOverUI );
    }
    
    public static event Action<Vector3,PlayerInputState, bool> OnRightClickUp;
    public static void Trigger_OnRightClickUp( Vector3 mousePos, PlayerInputState inputState, bool isOverUI )
    {
        if( OnRightClickUp != null ) OnRightClickUp( mousePos, inputState, isOverUI );
    }
    
    public static event Action RefreshInventoryDisplay;
    public static void Trigger_RefreshInventoryDisplay()
    {
        if (RefreshInventoryDisplay != null) RefreshInventoryDisplay();
    }

    public static event Action OnInventoryAvailabilityChanged;
    public static void Trigger_OnInventoryAvailabilityChanged()
    {
        if (OnInventoryAvailabilityChanged != null) OnInventoryAvailabilityChanged();
    }

    public static event Action RefreshSelection;
    public static void Trigger_RefreshSelection()
    {
        if (RefreshSelection != null) RefreshSelection();
    }

    public static event Action<float> OnGameSpeedChanged;
    public static void Trigger_OnGameSpeedChanged(float speedMod)
    {
        if (OnGameSpeedChanged != null) OnGameSpeedChanged(speedMod);
    }
    
    public static event Action<string, PlayerInteractable> OnTaskCancelled;
    public static void Trigger_OnTaskCancelled(string taskID, PlayerInteractable requestor)
    {
        if (OnTaskCancelled != null) OnTaskCancelled(taskID, requestor);
    }
 
    public static event Action MinuteTick;
    public static void Trigger_MinuteTick()
    {
        if (MinuteTick != null) MinuteTick();
    }

    public static event Action<int> HourTick;
    public static void Trigger_HourTick(int currentHour)
    {
        if (HourTick != null) HourTick(currentHour);
    }

    public static event Action DayTick;
    public static void Trigger_DayTick()
    {
        if (DayTick != null) DayTick();
    }

    public static event Action<KinlingData> OnKinlingChanged;
    public static void Trigger_OnKinlingChanged(KinlingData kinling)
    {
        if (OnKinlingChanged != null) OnKinlingChanged(kinling);
    }

    public static event Action OnConfigClipboardChanged;
    public static void Trigger_OnConfigClipboardChanged()
    {
        if (OnConfigClipboardChanged != null) OnConfigClipboardChanged();
    }

    public static event Action OnGameLoadStart;
    public static void Trigger_OnGameLoadStart()
    {
        if (OnGameLoadStart != null) OnGameLoadStart();
    }

    public static event Action<float> OnCameraZoomChanged;
    public static void Trigger_OnCameraZoomChanged(float zoomAmount)
    {
        if (OnCameraZoomChanged != null) OnCameraZoomChanged(zoomAmount);
    }
}
