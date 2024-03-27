using System;
using System.Collections;
using System.Collections.Generic;
using Data.Item;
using Databrain.Attributes;
using Managers;
using UnityEngine;

[Serializable]
public class ItemAmount
{
    [DataObjectDropdown("DataLibrary", true)] public ItemSettings Item;
    public int Quantity;

    public bool CanAfford()
    {
        return InventoryManager.Instance.CanAfford(Item, Quantity);
    }

    public ItemAmount Clone()
    {
        return new ItemAmount
        {
            Item = this.Item,
            Quantity = this.Quantity
        };
    }
}