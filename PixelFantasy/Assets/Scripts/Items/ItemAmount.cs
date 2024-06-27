using System;
using Managers;

[Serializable]
public class ItemAmount
{
    public ItemSettings Item;
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