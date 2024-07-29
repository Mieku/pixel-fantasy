using System;
using Managers;
using Newtonsoft.Json;
using ScriptableObjects;

[Serializable]
public class CostSettings
{
    public ItemSettings Item;
    public int Quantity;
    
    public bool CanAfford()
    {
        return InventoryManager.Instance.CanAfford(Item, Quantity);
    }

    public CostSettings Clone()
    {
        return new CostSettings
        {
            Item = this.Item,
            Quantity = this.Quantity
        };
    }
}

[Serializable]
public class CostData
{
    public string ItemSettingsID;
    public int Quantity;

    public CostData()
    {
    }

    public CostData(ItemSettings itemSettings)
    {
        ItemSettingsID = itemSettings.name;
    }
    
    public CostData(CostSettings setting)
    {
        ItemSettingsID = setting.Item.name;
        Quantity = setting.Quantity;
    }

    [JsonIgnore]
    public ItemSettings Item
    {
        get => GameSettings.Instance.LoadItemSettings(ItemSettingsID);
        set => ItemSettingsID = value.name;
    }
    
    public bool CanAfford()
    {
        return InventoryManager.Instance.CanAfford(Item, Quantity);
    }
}