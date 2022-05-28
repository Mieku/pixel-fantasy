using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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

    private ItemData _itemData;
    private StructureData _structureData;
    private GrowingResourceData _growingResourceData;
    private FloorData _floorData;
    private FurnitureData _furnitureData;
    private bool _isMouseOver;

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
                _structureData = GetComponent<Structure>().GetStructureData();
                break;
            case ObjectType.Unit:
                // TODO: Build me!
                Debug.LogError("Unit select not built yet!");
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
        _selectedIcon.SetActive(true);
        IsSelected = true;

        if (_objectRenderer != null)
        {
            //_selectedIcon.GetComponent<SpriteRenderer>().size = _objectRenderer.bounds.size;
            //_selectedIcon.transform.position = _objectRenderer.bounds.center;
        }
    }

    public void AreaSelectObject(Order orderForSelection)
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

    public bool ObjectValidForSelection(Order orderForSelection)
    {
        var clickableObject = GetComponent<IClickableObject>();
        if (clickableObject != null)
        {
            var possibleOrders = clickableObject.GetOrders();
            return possibleOrders.Any(possibleOrder => possibleOrder == orderForSelection);
        }
        
        return false;
    }

    public void UnselectObject()
    {
        _selectedIcon.SetActive(false);
        IsSelected = false;
    }

    private void OnMouseOver()
    {
        _isMouseOver = true;
    }

    private void OnMouseExit()
    {
        _isMouseOver = false;
    }

    private void OnMouseUp()
    {
        if (!_isMouseOver) return;
        
        var isOverUI = EventSystem.current.IsPointerOverGameObject();
        if(isOverUI) return;
        
        var clickableObj = GetComponent<IClickableObject>();
        if (clickableObj != null)
        {
            if (clickableObj.IsClickDisabled) return;
        }
        
        if (PlayerInputController.Instance.GetCurrentState() == PlayerInputState.None)
        {
            SelectionData data = GetSelectionData();
            PlayerInputController.Instance.SelectObject(this, data);
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
            case ObjectType.Unit:
                // TODO: Build me!
                return null;
            case ObjectType.Resource:
                return GetSelectionData(_growingResourceData);
            case ObjectType.Floor:
                return GetSelectionData(_floorData);
            case ObjectType.Furniture:
                return GetSelectionData(_furnitureData);
            default:
                throw new ArgumentOutOfRangeException();
        }
    }
    
    private SelectionData GetSelectionData(ItemData itemData)
    {
        var orders = Owner.GetOrders();

        SelectionData result = new SelectionData
        {
            ItemName = itemData.ItemName,
            ClickObject = this,
            Orders = orders
        };

        return result;
    }
    
    private SelectionData GetSelectionData(StructureData structureData)
    {
        SelectionData result = new SelectionData
        {
            ItemName = structureData.StructureName,
            Orders = structureData.Options,
            ClickObject = this,
        };

        return result;
    }
    
    private SelectionData GetSelectionData(GrowingResourceData growingResourceData)
    {
        var orders = Owner.GetOrders();
        
        SelectionData result = new SelectionData
        {
            ItemName = growingResourceData.ResourceName,
            Orders = orders,
            ClickObject = this,
        };

        return result;
    }
    
    private SelectionData GetSelectionData(FloorData floorData)
    {
        SelectionData result = new SelectionData
        {
            ItemName = floorData.FloorName,
            Orders = floorData.Options,
            ClickObject = this,
        };

        return result;
    }
    
    private SelectionData GetSelectionData(FurnitureData furnitureData)
    {
        SelectionData result = new SelectionData
        {
            ItemName = furnitureData.FurnitureName,
            Orders = furnitureData.Options,
            ClickObject = this,
        };

        return result;
    }

    public bool IsOrderActive(Order order)
    {
        return Owner.IsOrderActive(order);
    }
}

public class SelectionData
{
    public string ItemName;
    public List<Order> Orders;
    public ClickObject ClickObject;
}

public enum ObjectType
{
    Item,
    Structure,
    Unit,
    Resource,
    Floor,
    Furniture
}
