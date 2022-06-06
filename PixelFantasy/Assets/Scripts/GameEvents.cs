using System;
using System.Collections;
using System.Collections.Generic;
using Controllers;
using ScriptableObjects;
using UnityEngine;

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
    
    public static event Action<ItemData,int> OnInventoryAdded;
    public static void Trigger_OnInventoryAdded(ItemData itemData, int totalAmount )
    {
        if( OnInventoryAdded != null ) OnInventoryAdded( itemData, totalAmount );
    }
    
    public static event Action<ItemData,int> OnInventoryRemoved;
    public static void Trigger_OnInventoryRemoved(ItemData itemData, int totalAmount )
    {
        if( OnInventoryRemoved != null ) OnInventoryRemoved( itemData, totalAmount );
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
}
