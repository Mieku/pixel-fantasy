using System;
using System.Collections.Generic;
using Characters;
using Items;
using Managers;
using ScriptableObjects;
using UnityEngine;
using UnityEngine.Serialization;

    public class FurnitureData : CraftedItemData
    {
        public EFurnitureState State;
        public float RemainingWork;
        public PlacementDirection Direction;
        public bool InUse;
        public Furniture LinkedFurniture;
        public DyeSettings DyeOverride;
        public KinlingData[] Owners;
        public bool HasUseBlockingCommand;
        
        public List<ItemAmount> RemainingMaterialCosts;
        public List<ItemAmount> PendingResourceCosts = new List<ItemAmount>(); // Claimed by a task but not used yet
        public List<ItemAmount> IncomingResourceCosts = new List<ItemAmount>(); // The item is on its way
        public List<ItemData> IncomingItems = new List<ItemData>();

        public FurnitureSettings FurnitureSettings;

        public virtual void InitData(FurnitureSettings furnitureSettings)
        {
            FurnitureSettings = furnitureSettings;
            RemainingWork = furnitureSettings.CraftRequirements.WorkCost;
            RemainingMaterialCosts = furnitureSettings.CraftRequirements.GetMaterialCosts();
            Direction = furnitureSettings.DefaultDirection;
            Durability = furnitureSettings.MaxDurability;
            IsAllowed = true;

            if (furnitureSettings.NumberOfPossibleOwners > 0)
            {
                Owners = new KinlingData[furnitureSettings.NumberOfPossibleOwners];
            }
            else Owners = null;
        }
        
        protected void OnChanged()
        {
            LinkedFurniture?.OnChanged?.Invoke();
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
                Owners[0] = kinlingData;
                if (kinlingData.Partner != SecondaryOwner)
                {
                    SetSecondaryOwner(kinlingData.Partner);
                }
            }
            
            OnChanged();
        }
        
        public void SetSecondaryOwner(KinlingData kinlingData)
        {
            if (FurnitureSettings.NumberOfPossibleOwners >= 2)
            {
                Owners[1] = kinlingData;
            }
            
            OnChanged();
        }

        public KinlingData PrimaryOwner
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
        
        public KinlingData SecondaryOwner
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
        
        public float ConstructionPercent
        {
            get
            {
                if (State != EFurnitureState.Built)
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
        
        public void AddToPendingResourceCosts(ItemSettings itemSettings, int quantity = 1)
        {
            PendingResourceCosts ??= new List<ItemAmount>();

            foreach (var cost in PendingResourceCosts)
            {
                if (cost.Item == itemSettings)
                {
                    cost.Quantity += quantity;
                    return;
                }
            }
            
            PendingResourceCosts.Add(new ItemAmount
            {
                Item = itemSettings,
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
            IncomingItems ??= new List<ItemData>();
            IncomingItems.Add(itemData);
            
            IncomingResourceCosts ??= new List<ItemAmount>();

            foreach (var cost in IncomingResourceCosts)
            {
                if (cost.Item == itemData.Settings)
                {
                    cost.Quantity += 1;
                    return;
                }
            }
            
            IncomingResourceCosts.Add(new ItemAmount
            {
                Item = itemData.Settings,
                Quantity = 1
            });
        }
        
        public void RemoveFromIncomingItems(ItemData item)
        {
            IncomingItems ??= new List<ItemData>();
            IncomingItems.Remove(item);
            
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
        
        // public Color32 GetQualityColour()
        // {
        //     switch (Quality)
        //     {
        //         case EItemQuality.Poor:
        //             return Librarian.Instance.GetColour("Poor Quality");
        //         case EItemQuality.Common:
        //             return Librarian.Instance.GetColour("Common Quality");
        //         case EItemQuality.Remarkable:
        //             return Librarian.Instance.GetColour("Remarkable Quality");
        //         case EItemQuality.Excellent:
        //             return Librarian.Instance.GetColour("Excellent Quality");
        //         case EItemQuality.Mythical:
        //             return Librarian.Instance.GetColour("Mythical Quality");
        //         default:
        //             throw new ArgumentOutOfRangeException();
        //     }
        // }
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
