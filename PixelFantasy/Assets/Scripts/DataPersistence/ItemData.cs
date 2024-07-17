using System;
using System.Collections.Generic;
using System.ComponentModel;
using Handlers;
using Items;
using Managers;
using Newtonsoft.Json;
using ScriptableObjects;
using TaskSystem;
using UnityEngine;

[Serializable]
public class ItemData
{
    public string UniqueID;
    public string SettingsID;
    public EItemState State;
    public int Durability;
    public EItemQuality Quality;
    public bool IsAllowed;
    public Task CurrentTask;
    public string CarryingKinlingUID;
    public string AssignedStorageID;
    
    [JsonIgnore] public virtual ItemSettings Settings => GameSettings.Instance.LoadItemSettings(SettingsID);
    [JsonIgnore] public virtual string ItemName => Settings.ItemName;
    [JsonIgnore] public IStorage AssignedStorage => InventoryManager.Instance.GetStorageByID(AssignedStorageID);

    [JsonRequired] private float _posX;
    [JsonRequired] private float _posY;
    
    [JsonIgnore]
    public Vector2 Position
    {
        get => new(_posX, _posY);
        set
        {
            _posX = value.x;
            _posY = value.y;
        }
    }

    public virtual void InitData(ItemSettings settings)
    {
        SettingsID = settings.name;
        UniqueID = CreateUID();
        Durability = Settings.MaxDurability;
        IsAllowed = true;
        Quality = settings.DefaultQuality;
    }

    public virtual void DeleteItemData()
    {
        ItemsDatabase.Instance.DeregisterItem(this);
    }

    public Item GetLinkedItem()
    {
        if (State == EItemState.Carried)
        {
            return KinlingsDatabase.Instance.GetKinlingData(CarryingKinlingUID).HeldItem;
        }
        
        return ItemsDatabase.Instance.FindItemObject(UniqueID);
    }
    
    public void UnclaimItem()
    {
        if (AssignedStorage == null)
        {
            Debug.LogError("Tried to unclaim an item that is not assigned to storage");
            return;
        }
        
        AssignedStorage.RestoreClaimed(this);
    }

    public void ClaimItem()
    {
        if (AssignedStorage == null)
        {
            Debug.LogError("Tried to Claim an item that is not assigned to storage");
            return;
        }

        if (!AssignedStorage.ClaimItem(this))
        {
            Debug.LogError("Failed to claim item");
        }
    }

    public bool Equals(ItemData other)
    {
        return Settings == other.Settings;
    }

    /// <summary>
    /// The percentage of durability remaining ex: 0.5 = 50%
    /// </summary>
    [JsonIgnore]
    public float DurabilityPercent
    {
        get
        {
            var percent = (float)Durability / (float)Settings.MaxDurability;
            return percent;
        }
    }

    public Color32 GetQualityColour()
    {
        switch (Quality)
        {
            case EItemQuality.Poor:
                return Librarian.Instance.GetColour("Poor Quality");
            case EItemQuality.Common:
                return Librarian.Instance.GetColour("Common Quality");
            case EItemQuality.Remarkable:
                return Librarian.Instance.GetColour("Remarkable Quality");
            case EItemQuality.Excellent:
                return Librarian.Instance.GetColour("Excellent Quality");
            case EItemQuality.Mythical:
                return Librarian.Instance.GetColour("Mythical Quality");
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    /// <summary>
    /// Gets the item's text details for the player, for example decay or crafter... etc
    /// </summary>
    public virtual List<DetailsText> GetDetailsTexts()
    {
        List<DetailsText> results = new List<DetailsText>();
        return results;
    }
    
    protected string CreateUID()
    {
        return $"{ItemName}_{Guid.NewGuid()}";
    }
}

    
public class DetailsText
{
    public string Header;
    public string Message;
    public bool HasHeader => !string.IsNullOrEmpty(Header);

    public DetailsText(string header, string message)
    {
        Header = header;
        Message = message;
    }
}

public enum EItemState
{
    Loose,
    Stored,
    Carried,
}

public enum EItemQuality
{
    [Description("Poor")] Poor = 0, // grey
    [Description("Common")] Common = 1, // white
    [Description("Remarkable")] Remarkable = 2, // green
    [Description("Enchanted")] Excellent = 3, // blue
    [Description("Mythical")] Mythical = 4, // gold
}

[Serializable]
public enum EItemCategory
{
    [Description("Materials")] Materials,
    [Description("Tools")] Tool,
    [Description("Clothing")] Clothing,
    [Description("Food")] Food,
    [Description("Furniture")] Furniture,
    [Description("Specific Storage")] SpecificStorage,
    [Description("Bulky Resource")] BulkyResource,
}