using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Actions;
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
    private GrowingResourceData _growingResourceData;
    private FloorData _floorData;
    private FurnitureData _furnitureData;
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
        // Prevents weird selection bug
        gameObject.SetActive(false);
        gameObject.SetActive(true);
        
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
                _growingResourceData = GetComponent<Resource>().GetResourceData();
                break;
            case ObjectType.Floor:
                _floorData = GetComponent<Floor>().FloorData;
                break;
            case ObjectType.Furniture:
                _furnitureData = GetComponent<Furniture>().FurnitureData;
                break;
            case ObjectType.Mountain:
                // Nothing needed, other type should be refactored to follow pattern...
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
            var data = GetSelectionData();
            PlayerInputController.Instance.SelectObject(this, data);
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

    public void AreaSelectObject(ActionBase orderForSelection)
    {
        if (ObjectValidForSelection(orderForSelection))
        {
            _selectedIcon.SetActive(true);
        }
    }

    public void UnselectAreaSelection()
    {
        _selectedIcon.SetActive(false);
    }

    public bool ObjectValidForSelection(ActionBase actionForSelection)
    {
        var clickableObject = GetComponent<IClickableObject>();
        if (clickableObject != null)
        {
            var possibleOrders = clickableObject.GetActions();
            return possibleOrders.Any(possibleOrder => possibleOrder == actionForSelection);
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

    public void TriggerSelected()
    {
        var isOverUI = EventSystem.current.IsPointerOverGameObject();
        if(isOverUI) return;
        
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
                SelectionData data = GetSelectionData();
                PlayerInputController.Instance.SelectObject(this, data);
            }
        }
    }

    private SelectionData GetSelectionData()
    {
        switch (_objectType)
        {
            case ObjectType.Item:
                return GetSelectionData(_itemData);
            case ObjectType.Structure:
                return GetSelectionData(_structureData);
            case ObjectType.Resource:
                return GetSelectionData(_growingResourceData);
            case ObjectType.Floor:
                return GetSelectionData(_floorData);
            case ObjectType.Furniture:
                return GetSelectionData(_furnitureData);
            case ObjectType.Mountain:
                return GetComponent<Mountain>().GetSelectionData();
            default:
                throw new ArgumentOutOfRangeException();
        }
    }
    
    private SelectionData GetSelectionData(ConstructionData itemData)
    {
        var actions = Owner.GetActions();
        var cancellableActions = Owner.GetCancellableActions();

        SelectionData result = new SelectionData
        {
            ItemName = itemData.ConstructionName,
            ClickObject = this,
            Actions = actions,
            CancellableActions = cancellableActions,
            Requestor = GetComponent<Interactable>(),
        };

        return result;
    }
    
    private SelectionData GetSelectionData(ItemData itemData)
    {
        var actions = Owner.GetActions();
        var cancellableActions = Owner.GetCancellableActions();

        SelectionData result = new SelectionData
        {
            ItemName = itemData.ItemName,
            ClickObject = this,
            Actions = actions,
            CancellableActions = cancellableActions,
            Requestor = GetComponent<Interactable>(),
        };

        return result;
    }
    
    private SelectionData GetSelectionData(StructureData structureData)
    {
        var actions = Owner.GetActions();
        var cancellableActions = Owner.GetCancellableActions();
        
        SelectionData result = new SelectionData
        {
            ItemName = structureData.ConstructionName,
            Actions = actions,
            CancellableActions = cancellableActions,
            ClickObject = this,
            Requestor = GetComponent<Interactable>(),
        };

        return result;
    }
    
    private SelectionData GetSelectionData(GrowingResourceData growingResourceData)
    {
        var actions = Owner.GetActions();
        var cancellableActions = Owner.GetCancellableActions();
        
        SelectionData result = new SelectionData
        {
            ItemName = growingResourceData.ResourceName,
            Actions = actions,
            CancellableActions = cancellableActions,
            ClickObject = this,
            Requestor = GetComponent<Interactable>(),
        };

        return result;
    }
    
    private SelectionData GetSelectionData(FloorData floorData)
    {
        var actions = Owner.GetActions();
        var cancellableActions = Owner.GetCancellableActions();
        
        SelectionData result = new SelectionData
        {
            ItemName = floorData.ConstructionName,
            Actions = actions,
            CancellableActions = cancellableActions,
            ClickObject = this,
            Requestor = GetComponent<Interactable>(),
        };

        return result;
    }
    
    private SelectionData GetSelectionData(FurnitureData furnitureData)
    {
        var actions = Owner.GetActions();
        var cancellableActions = Owner.GetCancellableActions();
        
        SelectionData result = new SelectionData
        {
            ItemName = furnitureData.FurnitureName,
            Actions = actions,
            CancellableActions = cancellableActions,
            ClickObject = this,
            Requestor = GetComponent<Interactable>(),
        };

        return result;
    }

    public bool IsActionActive(ActionBase action)
    {
        return Owner.IsActionActive(action);
    }
}

public class SelectionData
{
    public string ItemName;
    public List<ActionBase> Actions;
    public List<ActionBase> CancellableActions;
    public ClickObject ClickObject;
    public Interactable Requestor;
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
