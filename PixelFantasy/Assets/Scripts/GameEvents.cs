using System;
using Characters;
using Controllers;
using UnityEngine;
using Action = System.Action;

public class GameEvents : MonoBehaviour
{
    public static event Action OnInventoryChanged;
    public static void Trigger_OnInventoryChanged()
    {
        if (OnInventoryChanged != null) OnInventoryChanged();
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
