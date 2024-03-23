using System;
using System.Collections.Generic;
using Data.Item;
using Databrain;
using Databrain.Attributes;
using ScriptableObjects;
using UnityEngine;

namespace Data.Structure
{
    [Serializable]
    public enum EConstructionState
    {
        Planning,
        Blueprint,
        Built,
    }
    
    [DataObjectAddToRuntimeLibrary]
    public class ConstructionData : DataObject
    {
        [ExposeToInspector, DatabrainSerialize] 
        public float Durability;
        
        [ExposeToInspector, DatabrainSerialize] 
        public Vector2 Position;
        
        [ExposeToInspector, DatabrainSerialize] 
        public EConstructionState State;
        
        [ExposeToInspector, DatabrainSerialize] 
        public float RemainingWork;
        
        [ExposeToInspector, DatabrainSerialize] 
        public List<ItemAmount> RemainingMaterialCosts;
        
        [ExposeToInspector, DatabrainSerialize] 
        public List<ItemAmount> PendingResourceCosts = new List<ItemAmount>(); // Claimed by a task but not used yet
        
        [ExposeToInspector, DatabrainSerialize] 
        public List<ItemAmount> IncomingResourceCosts = new List<ItemAmount>(); // The item is on its way
        
        [ExposeToInspector, DatabrainSerialize] 
        public List<ItemData> IncomingItems = new List<ItemData>();

        [ExposeToInspector, DatabrainSerialize] 
        public CraftRequirements CraftRequirements;

        [ExposeToInspector, DatabrainSerialize]
        public float MaxDurability;
        
        public void InitData()
        {
            RemainingMaterialCosts = CraftRequirements.GetMaterialCosts();
            RemainingWork = CraftRequirements.WorkCost;
            Durability = MaxDurability;
        }
        
        
        public void DeductFromMaterialCosts(ItemDataSettings itemData)
        {
            foreach (var cost in RemainingMaterialCosts)
            {
                if (cost.Item.initialGuid == itemData.initialGuid && cost.Quantity > 0)
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
        
        public void AddToPendingResourceCosts(ItemDataSettings itemData, int quantity = 1)
        {
            PendingResourceCosts ??= new List<ItemAmount>();

            foreach (var cost in PendingResourceCosts)
            {
                if (cost.Item.initialGuid == itemData.initialGuid)
                {
                    cost.Quantity += quantity;
                    return;
                }
            }
            
            PendingResourceCosts.Add(new ItemAmount
            {
                Item = itemData,
                Quantity = quantity
            });
        }
        
        public void RemoveFromPendingResourceCosts(ItemDataSettings itemData, int quantity = 1)
        {
            foreach (var cost in PendingResourceCosts)
            {
                if (cost.Item.initialGuid == itemData.initialGuid)
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
                if (cost.Item.initialGuid == itemData.initialGuid)
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
                if (cost.Item.initialGuid == item.initialGuid)
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
    }
}
