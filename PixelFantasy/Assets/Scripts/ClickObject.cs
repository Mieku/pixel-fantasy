using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Characters;
using Controllers;
using Interfaces;
using Items;
using ScriptableObjects;
using Systems.CursorHandler.Scripts;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(BoxCollider2D))]
public class ClickObject : MonoBehaviour
{
    [SerializeField] private ObjectType _objectType;
    [SerializeField] private GameObject _selectedIcon;
    [SerializeField] private SpriteRenderer _objectRenderer;
    [SerializeField] private bool _usesTintedSelection;

    //private ItemSettings _itemSettings;
    //private ConstructionSettings _structureSettings;
    private Kinling _kinling;

    private IClickableObject _clickableObject; // Cache
    public IClickableObject Owner
    {
        get
        {
            if (_clickableObject == null)
            {
                _clickableObject = gameObject.GetComponent<IClickableObject>();
            }

            return _clickableObject;
        }
    }

    public bool IsSelected;

    private void Initialize()
    {
        switch (_objectType)
        {
            case ObjectType.Item:
                //_itemSettings = GetComponent<Item>().GetItemData();
                break;
            case ObjectType.Structure:
                //_structureSettings = GetComponent<Construction>().GetConstructionData();
                break;
            case ObjectType.Kinling:
                _kinling = GetComponent<Kinling>();
                break;
            case ObjectType.Resource:
            case ObjectType.Mountain:
                //_growingResourceSettings = GetComponent<BasicResource>().GetResourceData();
                break;
            case ObjectType.Floor:
                //_floorData = GetComponent<Floor>().FloorData;
                break;
            case ObjectType.Furniture:
                //_furnitureData = GetComponent<Furniture>().FurnitureData;
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }
    
    private void Start()
    {
        Initialize();
        UnselectObject();

        GameEvents.RefreshSelection += RefreshSelection;
    }

    private void OnDestroy()
    {
        UnselectObject();
        PlayerInputController.Instance.OnClickObjectDestroy(this);
        
        GameEvents.RefreshSelection -= RefreshSelection;

        if (IsSelected)
        {
            PlayerInputController.Instance.ClearStoredData();
        }
    }

    private void RefreshSelection()
    {
        if (IsSelected)
        {
            PlayerInputController.Instance.SelectObject(this);
        }
    }

    public void SelectObject()
    {
        if (_selectedIcon != null)
        {
            _selectedIcon.SetActive(true);
        }
        
        IsSelected = true;

        if (_kinling != null)
        {
            _kinling.KinlingAgent.SetPathVisibility(true);
        }

        if (_objectRenderer != null)
        {
            //_selectedIcon.GetComponent<SpriteRenderer>().size = _objectRenderer.bounds.size;
            //_selectedIcon.transform.position = _objectRenderer.bounds.center;
        }

        if (_usesTintedSelection)
        {
            gameObject.GetComponent<IClickableTile>().TintTile();
        }
    }

    public void AreaSelectObject(Command pendingCommand)
    {
        if (ObjectValidForSelection(pendingCommand))
        {
            SelectObject();
        }
    }

    public void UnselectAreaSelection()
    {
        UnselectObject();
    }

    public bool ObjectValidForSelection(Command pendingCommand)
    {
        var clickableObject = GetComponent<IClickableObject>();
        if (clickableObject != null)
        {
            if (pendingCommand.Name == "Cancel Command")
            {
                return clickableObject.GetPlayerInteractable().PendingCommand != null;
            }
            else
            {
                var possibleCommand = clickableObject.GetCommands();
                return possibleCommand.Any(cmd => cmd == pendingCommand);
            }
        }
        
        return false;
    }

    public void UnselectObject()
    {
        if (_selectedIcon != null)
        {
            _selectedIcon.SetActive(false);
        }
        
        if (_kinling != null)
        {
            _kinling.KinlingAgent.SetPathVisibility(false);
        }
        
        if (_usesTintedSelection)
        {
            gameObject.GetComponent<IClickableTile>().UnTintTile();
        }
        
        IsSelected = false;
    }

    public void TriggerSelected(bool ignoreUICheck = false)
    {
        if (!ignoreUICheck)
        {
            var isOverUI = EventSystem.current.IsPointerOverGameObject();
            if(isOverUI) return;
        }
        
        var clickableObj = GetComponent<IClickableObject>();
        if (clickableObj != null)
        {
            if (clickableObj.IsClickDisabled) return;
        }
        
        if (PlayerInputController.Instance.GetCurrentState() == PlayerInputState.None)
        {
            if (_objectType == ObjectType.Kinling)
            {
                PlayerInputController.Instance.SelectUnit(_kinling);
            }
            else
            {
                PlayerInputController.Instance.SelectObject(this);
            }
        }
    }

    public void TriggerShowCommands(Kinling kinlingToHandleCommand)
    {
        CommandController.Instance.ShowCommands(this, kinlingToHandleCommand);
    }
}

public enum ObjectType
{
    Item,
    Structure,
    Kinling,
    Resource,
    Floor,
    Furniture,
    Mountain,
}
