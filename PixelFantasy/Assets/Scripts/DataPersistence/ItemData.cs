using System;
using System.Collections.Generic;
using System.ComponentModel;
using Items;
using Managers;
using TaskSystem;
using UnityEngine;

[Serializable]
public class ItemData
{
    public string UniqueID;
    public int Durability;
    public EItemQuality Quality;
    public bool IsAllowed;
    public Task CurrentTask;
    public string CarryingKinlingUID;
    public IStorage AssignedStorage;
    public Vector2 Position;
    public Items.Item LinkedItem;
    

    public ItemSettings Settings;
    public string ItemName => Settings.ItemName;

    public virtual void InitData(ItemSettings settings)
    {
        Settings = settings;
        UniqueID = CreateUID();
        Durability = Settings.MaxDurability;
        IsAllowed = true;
        Quality = settings.DefaultQuality;
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