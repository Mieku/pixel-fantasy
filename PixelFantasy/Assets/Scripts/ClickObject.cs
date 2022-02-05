using System;
using System.Collections;
using System.Collections.Generic;
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
    private bool _isMouseOver;

    private void Initialize()
    {
        // Prevents wierd selection bug
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
            default:
                throw new ArgumentOutOfRangeException();
        }
    }
    
    private void Start()
    {
        Initialize();
        UnselectObject();
    }

    public void SelectObject()
    {
        _selectedIcon.SetActive(true);

        if (_objectRenderer != null)
        {
            _selectedIcon.GetComponent<SpriteRenderer>().size = _objectRenderer.bounds.size;
            _selectedIcon.transform.position = _objectRenderer.bounds.center;
        }
    }

    public void UnselectObject()
    {
        _selectedIcon.SetActive(false);
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
            default:
                throw new ArgumentOutOfRangeException();
        }
    }
    
    private SelectionData GetSelectionData(ItemData itemData)
    {
        SelectionData result = new SelectionData
        {
            ItemName = itemData.ItemName,
            Options = itemData.Options,
            Owner = gameObject,
        };
        
        return result;
    }
    
    private SelectionData GetSelectionData(StructureData structureData)
    {
        SelectionData result = new SelectionData
        {
            ItemName = structureData.StructureName,
            Options = structureData.Options,
            Owner = gameObject,
        };

        return result;
    }
    
    private SelectionData GetSelectionData(GrowingResourceData growingResourceData)
    {
        SelectionData result = new SelectionData
        {
            ItemName = growingResourceData.ResourceName,
            Options = growingResourceData.Options,
            Owner = gameObject,
        };

        return result;
    }
    
    private SelectionData GetSelectionData(FloorData floorData)
    {
        SelectionData result = new SelectionData
        {
            ItemName = floorData.FloorName,
            Options = floorData.Options,
            Owner = gameObject,
        };

        return result;
    }
}

public class SelectionData
{
    public string ItemName;
    public List<Option> Options;
    public GameObject Owner;
}

public enum ObjectType
{
    Item,
    Structure,
    Unit,
    Resource,
    Floor
}
