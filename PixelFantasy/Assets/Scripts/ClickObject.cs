using System;
using System.Collections;
using System.Collections.Generic;
using Controllers;
using Items;
using ScriptableObjects;
using UnityEditor;
using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class ClickObject : MonoBehaviour
{
    [SerializeField] private ObjectType _objectType;
    [SerializeField] private GameObject _selectedIcon;

    private ItemData _itemData;
    private StructureData _structureData;

    private void Initialize()
    {
        switch (_objectType)
        {
            case ObjectType.Resource:
                _itemData = GetComponent<Item>().GetItemData();
                break;
            case ObjectType.Structure:
                _structureData = GetComponent<Structure>().GetStructureData();
                break;
            case ObjectType.Unit:
                // TODO: Build me!
                Debug.LogError("Unit select not built yet!");
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
    }

    public void UnselectObject()
    {
        _selectedIcon.SetActive(false);
    }
    
    private void OnMouseUp()
    {
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
            case ObjectType.Resource:
                return GetSelectionData(_itemData);
            case ObjectType.Structure:
                return GetSelectionData(_structureData);
            case ObjectType.Unit:
                // TODO: Build me!
                return null;
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
}

public class SelectionData
{
    public string ItemName;
    public List<Option> Options;
    public GameObject Owner;
}

public enum ObjectType
{
    Resource,
    Structure,
    Unit
}
