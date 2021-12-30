using System;
using System.Collections;
using System.Collections.Generic;
using Controllers;
using UnityEngine;

public class GameEvents : MonoBehaviour
{
    public static event Action<Vector3,PlayerInputState> OnLeftClickDown;
    public static void Trigger_OnLeftClickDown( Vector3 mousePos, PlayerInputState inputState )
    {
        if( OnLeftClickDown != null ) OnLeftClickDown( mousePos, inputState );
    }
    
    public static event Action<Vector3,PlayerInputState> OnRightClickDown;
    public static void Trigger_OnRightClickDown( Vector3 mousePos, PlayerInputState inputState )
    {
        if( OnRightClickDown != null ) OnRightClickDown( mousePos, inputState );
    }
    
    public static event Action<Vector3,PlayerInputState> OnLeftClickHeld;
    public static void Trigger_OnLeftClickHeld( Vector3 mousePos, PlayerInputState inputState )
    {
        if( OnLeftClickHeld != null ) OnLeftClickHeld( mousePos, inputState );
    }
    
    public static event Action<Vector3,PlayerInputState> OnRightClickHeld;
    public static void Trigger_OnRightClickHeld( Vector3 mousePos, PlayerInputState inputState )
    {
        if( OnRightClickHeld != null ) OnRightClickHeld( mousePos, inputState );
    }
    
    public static event Action<Vector3,PlayerInputState> OnLeftClickUp;
    public static void Trigger_OnLeftClickUp( Vector3 mousePos, PlayerInputState inputState )
    {
        if( OnLeftClickUp != null ) OnLeftClickUp( mousePos, inputState );
    }
    
    public static event Action<Vector3,PlayerInputState> OnRightClickUp;
    public static void Trigger_OnRightClickUp( Vector3 mousePos, PlayerInputState inputState )
    {
        if( OnRightClickUp != null ) OnRightClickUp( mousePos, inputState );
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
}
