using System;
using System.Collections.Generic;
using Characters;
using Handlers;
using Items;
using Managers;
using Newtonsoft.Json;
using ScriptableObjects;
using Systems.Input_Management;
using UnityEngine;
using UnityEngine.Serialization;

[Serializable]
public class FurnitureData
{
    public string UniqueID;
    public string PendingTaskUID;
    public EFurnitureState FurnitureState;
    public float RemainingWork;
    public PlacementDirection Direction;
    public bool InUse;
    //public DyeSettings DyeOverride;
    public string[] Owners;
    public bool HasUseBlockingCommand;
    public string SettingsID;
    public int Durability;
    public bool IsAllowed;
    public string CraftersUID;
    public EItemQuality Quality;
    
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
    
    public List<CostData> RemainingMaterialCosts;
    public List<CostData> PendingResourceCosts = new List<CostData>(); // Claimed by a task but not used yet
    public List<CostData> IncomingResourceCosts = new List<CostData>(); // The item is on its way
    public List<string> IncomingItemsUIDs = new List<string>();

    [JsonIgnore] public FurnitureSettings FurnitureSettings => GameSettings.Instance.LoadFurnitureSettings(SettingsID);
    [JsonIgnore] public string ItemName => FurnitureSettings.ItemName;
    
    public virtual void InitData(FurnitureSettings furnitureSettings)
    {
        SettingsID = furnitureSettings.name;
        UniqueID = CreateUID();
        RemainingWork = furnitureSettings.CraftRequirements.WorkCost;
        RemainingMaterialCosts = furnitureSettings.CraftRequirements.GetMaterialCosts();
        Direction = furnitureSettings.DefaultDirection;
        Durability = furnitureSettings.MaxDurability;
        IsAllowed = true;

        if (furnitureSettings.NumberOfPossibleOwners > 0)
        {
            Owners = new string[furnitureSettings.NumberOfPossibleOwners];
        }
        else Owners = null;
        
        FurnitureDatabase.Instance.RegisterFurniture(this);
    }

    public Furniture GetLinkedFurniture()
    {
        return FurnitureDatabase.Instance.FindFurnitureObject(UniqueID);
    }
    
    protected void OnChanged()
    {
        GetLinkedFurniture().InformChanged();
    }

    public void SetPrimaryOwner(KinlingData kinlingData)
    {
        if (kinlingData == null)
        {
            var secondaryOwner = SecondaryOwner;
            if (secondaryOwner != null)
            {
                Owners[0] = secondaryOwner;
                SetSecondaryOwner(null);
            }
            else
            {
                Owners[0] = null;
            }
        }
        else
        {
            Owners[0] = kinlingData.UniqueID;
            if (kinlingData.PartnerUID != SecondaryOwner)
            {
                var partner = KinlingsDatabase.Instance.Query(kinlingData.PartnerUID);
                SetSecondaryOwner(partner);
            }
        }
        
        OnChanged();
    }
    
    public void SetSecondaryOwner(KinlingData kinlingData)
    {
        if (FurnitureSettings.NumberOfPossibleOwners >= 2)
        {
            Owners[1] = kinlingData.UniqueID;
        }
        
        OnChanged();
    }

    [JsonIgnore]
    public string PrimaryOwner
    {
        get
        {
            if (Owners != null)
            {
                return Owners[0];
            }

            return null;
        }
    }
    
    [JsonIgnore]
    public string SecondaryOwner
    {
        get
        {
            if (Owners != null && FurnitureSettings.NumberOfPossibleOwners >= 2)
            {
                return Owners[1];
            }

            return null;
        }
    }
    
    [JsonIgnore]
    public float ConstructionPercent
    {
        get
        {
            if (FurnitureState != EFurnitureState.Built)
            {
                return 1 - (RemainingWork / FurnitureSettings.CraftRequirements.WorkCost);
            }
            else
            {
                return 1f;
            }
        }
    }
    
    public void DeductFromMaterialCosts(ItemSettings itemSettings)
    {
        foreach (var cost in RemainingMaterialCosts)
        {
            if (cost.Item == itemSettings && cost.Quantity > 0)
            {
                cost.Quantity--;
                if (cost.Quantity <= 0)
                {
                    RemainingMaterialCosts.Remove(cost);
                }

                break;
            }
        }
    }

    public bool HasAllMaterials()
    {
        return RemainingMaterialCosts.Count == 0;
    }
    
    public void AddToPendingResourceCosts(ItemSettings itemSettings, int quantity = 1)
    {
        PendingResourceCosts ??= new List<CostData>();

        foreach (var cost in PendingResourceCosts)
        {
            if (cost.Item == itemSettings)
            {
                cost.Quantity += quantity;
                return;
            }
        }
        
        PendingResourceCosts.Add(new CostData(itemSettings)
        {
            Quantity = quantity
        });
    }
    
    public void RemoveFromPendingResourceCosts(ItemSettings itemSettings, int quantity = 1)
    {
        foreach (var cost in PendingResourceCosts)
        {
            if (cost.Item == itemSettings)
            {
                cost.Quantity -= quantity;
                if (cost.Quantity <= 0)
                {
                    PendingResourceCosts.Remove(cost);
                }

                return;
            }
        }
    }
    
    public void AddToIncomingItems(ItemData itemData)
    {
        IncomingItemsUIDs ??= new List<string>();
        IncomingItemsUIDs.Add(itemData.UniqueID);
        
        IncomingResourceCosts ??= new List<CostData>();

        foreach (var cost in IncomingResourceCosts)
        {
            if (cost.Item == itemData.Settings)
            {
                cost.Quantity += 1;
                return;
            }
        }
        
        IncomingResourceCosts.Add(new CostData(itemData.Settings)
        {
            Quantity = 1
        });
    }
    
    public void RemoveFromIncomingItems(ItemData item)
    {
        IncomingItemsUIDs ??= new List<string>();
        IncomingItemsUIDs.Remove(item.UniqueID);
        
        foreach (var cost in IncomingResourceCosts)
        {
            if (cost.Item == item.Settings)
            {
                cost.Quantity -= 1;
                if (cost.Quantity <= 0)
                {
                    IncomingResourceCosts.Remove(cost);
                }

                return;
            }
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
    
    public List<DetailsText> GetDetailsTexts()
    {
        var results = new List<DetailsText>();

        if (!string.IsNullOrEmpty(CraftersUID))
        {
            // Add crafter
            var crafter = KinlingsDatabase.Instance.GetKinling(CraftersUID);
            results.Add(new DetailsText("Crafted By:", crafter.FullName));
        }

        return results;
    }
    
    protected string CreateUID()
    {
        return $"{ItemName}_{Guid.NewGuid()}";
    }
}


[Serializable]
public enum EFurnitureState
{
    InProduction,
    Built,
}

[Serializable]
public class FurnitureVariant
{
    [SerializeField] private Sprite _materialSelectIcon;
    [FormerlySerializedAs("_furnitureData")] [SerializeField] private FurnitureSettings _furnitureSettings;

    public Sprite MaterialSelectIcon => _materialSelectIcon; // Typically the icon of the material change
    public FurnitureSettings FurnitureSettings => _furnitureSettings;
}

[Serializable]
public class ColourOptions
{
    public string ColourOptionsHeader;
    public List<DyeSettings> DyePalettes;
}