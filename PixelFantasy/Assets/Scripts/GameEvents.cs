using System;
using System.Collections;
using System.Collections.Generic;
using Characters;
using Controllers;
using ScriptableObjects;
using TaskSystem;
using UnityEngine;
using Zones;
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

    public static event Action OnLoadingGameBeginning;
    public static void Trigger_OnLoadingGameBeginning()
    {
        if (OnLoadingGameBeginning != null) OnLoadingGameBeginning();
    }
    
    public static event Action OnLoadingGameEnd;
    public static void Trigger_OnLoadingGameEnd()
    {
        if (OnLoadingGameEnd != null) OnLoadingGameEnd();
    }
    
    public static event Action OnSavingGameBeginning;
    public static void Trigger_OnSavingGameBeginning()
    {
        if (OnSavingGameBeginning != null) OnSavingGameBeginning();
    }
    
    public static event Action OnSavingGameEnd;
    public static void Trigger_OnSavingGameEnd()
    {
        if (OnSavingGameEnd != null) OnSavingGameEnd();
    }

    public static event Action<float> OnGameSpeedChanged;
    public static void Trigger_OnGameSpeedChanged(float speedMod)
    {
        if (OnGameSpeedChanged != null) OnGameSpeedChanged(speedMod);
    }

    public static event Action<string> OnUnitStatsChanged;
    public static void Trigger_OnUnitStatsChanged(string unitUID)
    {
        if (OnUnitStatsChanged != null) OnUnitStatsChanged(unitUID);
    }

    public static event Action<bool> OnZoneDisplayChanged;
    public static void Trigger_OnZoneDisplayChanged(bool zonesVisible)
    {
        if (OnZoneDisplayChanged != null) OnZoneDisplayChanged(zonesVisible);
    }
    
    public static event Action<Task> OnTaskCancelled;
    public static void Trigger_OnTaskCancelled(Task task)
    {
        if (OnTaskCancelled != null) OnTaskCancelled(task);
    }

    public static event Action<UnitState> OnUnitOccupationChanged;
    public static void Trigger_OnUnitOccupationChanged(UnitState unit)
    {
        if (OnUnitOccupationChanged != null) OnUnitOccupationChanged(unit);
    }
    
    public static event Action<bool> OnStructureGuideToggled;
    public static void Trigger_OnStructureGuideToggled(bool shouldEnable)
    {
        if (OnStructureGuideToggled != null) OnStructureGuideToggled(shouldEnable);
    }
    
    public static event Action<bool> OnRoofGuideToggled;
    public static void Trigger_OnRoofGuideToggled(bool shouldEnable)
    {
        if (OnRoofGuideToggled != null) OnRoofGuideToggled(shouldEnable);
    }

    public static event Action<Vector2> OnRoofRefresh;
    public static void Trigger_OnRoofRefresh(Vector2 callerPos)
    {
        if (OnRoofRefresh != null) OnRoofRefresh(callerPos);
    }

    public static event Action<bool> OnHideRoofsToggled;
    public static void Trigger_OnHideRoofsToggled(bool hideRoofs)
    {
        if (OnHideRoofsToggled != null) OnHideRoofsToggled(hideRoofs);
    }

    public static event Action MinuteTick;
    public static void Trigger_MinuteTick()
    {
        if (MinuteTick != null) MinuteTick();
    }

    public static event Action<Unit> OnKinlingChanged;
    public static void Trigger_OnKinlingChanged(Unit unit)
    {
        if (OnKinlingChanged != null) OnKinlingChanged(unit);
    }

    public static event Action OnGlimraDue;
    public static void Trigger_OnGlimraDue()
    {
        if (OnGlimraDue != null) OnGlimraDue();
    }

    public static event Action<int> OnGlimraTotalChanged;
    public static void Trigger_OnGlimraTotalChanged(int totalGlimra)
    {
        if (OnGlimraTotalChanged != null) OnGlimraTotalChanged(totalGlimra);
    }
}
