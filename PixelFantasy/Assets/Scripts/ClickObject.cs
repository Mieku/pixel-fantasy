using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Characters;
using Controllers;
using Interfaces;
using Items;
using ScriptableObjects;
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

    private ItemData _itemData;
    private ConstructionData _structureData;
    private ResourceData growingResourceData;
    private Unit _unit;

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
                _itemData = GetComponent<Item>().GetItemData();
                break;
            case ObjectType.Structure:
                _structureData = GetComponent<Construction>().GetConstructionData();
                break;
            case ObjectType.Unit:
                _unit = GetComponent<Unit>();
                break;
            case ObjectType.Resource:
            case ObjectType.Mountain:
                growingResourceData = GetComponent<Resource>().GetResourceData();
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
            if (_objectType == ObjectType.Unit)
            {
                PlayerInputController.Instance.SelectUnit(this, _unit);
            }
            else
            {
                PlayerInputController.Instance.SelectObject(this);
            }
        }
    }

    // private SelectionData GetSelectionData()
    // {
    //     switch (_objectType)
    //     {
    //         case ObjectType.Item:
    //             return GetSelectionData(_itemData);
    //         case ObjectType.Structure:
    //             return GetSelectionData(_structureData);
    //         case ObjectType.Resource:
    //         case ObjectType.Mountain:
    //             return GetSelectionData(growingResourceData);
    //         case ObjectType.Floor:
    //             return GetSelectionData(_floorData);
    //         default:
    //             throw new ArgumentOutOfRangeException();
    //     }
    // }
    
    // private SelectionData GetSelectionData(ConstructionData itemData)
    // {
    //     SelectionData result = new SelectionData
    //     {
    //         ItemName = itemData.ConstructionName,
    //         ClickObject = this,
    //         Requestor = GetComponent<PlayerInteractable>(),
    //     };
    //
    //     return result;
    // }
    
    // private SelectionData GetSelectionData(ItemData itemData)
    // {
    //     SelectionData result = new SelectionData
    //     {
    //         ItemName = itemData.ItemName,
    //         ClickObject = this,
    //         Requestor = GetComponent<PlayerInteractable>(),
    //     };
    //
    //     return result;
    // }
    
    // private SelectionData GetSelectionData(StructureData structureData)
    // {
    //     SelectionData result = new SelectionData
    //     {
    //         ItemName = structureData.ConstructionName,
    //         ClickObject = this,
    //         Requestor = GetComponent<PlayerInteractable>(),
    //     };
    //
    //     return result;
    // }
    
    // private SelectionData GetSelectionData(ResourceData growingResourceData)
    // {
    //     var commands = Owner.GetCommands();
    //     
    //     SelectionData result = new SelectionData
    //     {
    //         ItemName = growingResourceData.ResourceName,
    //         Commands = commands,
    //         ClickObject = this,
    //         Requestor = GetComponent<PlayerInteractable>(),
    //     };
    //
    //     return result;
    // }
    
    // private SelectionData GetSelectionData(FloorData floorData)
    // {
    //     SelectionData result = new SelectionData
    //     {
    //         ItemName = floorData.ConstructionName,
    //         ClickObject = this,
    //         Requestor = GetComponent<PlayerInteractable>(),
    //     };
    //
    //     return result;
    // }
}

public class SelectionData // TODO: Get rid of this!
{
    public string ItemName;
    public List<Command> Commands;
    public ClickObject ClickObject;
    public PlayerInteractable Requestor;
}

public enum ObjectType
{
    Item,
    Structure,
    Unit,
    Resource,
    Floor,
    Furniture,
    Mountain,
}
