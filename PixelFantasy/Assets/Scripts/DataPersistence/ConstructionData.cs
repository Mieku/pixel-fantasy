using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using ScriptableObjects;
using Systems.Buildings.Scripts;
using UnityEngine;

[Serializable]
    public enum EConstructionState
    {
        Planning,
        Blueprint,
        Built,
    }
    
    [Serializable]
    public class ConstructionData
    {
        public string SettingsID;
        public float Durability;
        public EConstructionState State;
        public float RemainingWork;
        public List<ItemAmount> RemainingMaterialCosts;
        public List<ItemAmount> PendingResourceCosts = new List<ItemAmount>(); // Claimed by a task but not used yet
        public List<ItemAmount> IncomingResourceCosts = new List<ItemAmount>(); // The item is on its way
        public List<ItemData> IncomingItems = new List<ItemData>();
        public float MaxDurability;

        [JsonIgnore] public ConstructionSettings Settings => GameSettings.Instance.LoadConstructionSettings(SettingsID);
        
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
        
        public void InitData()
        {
            RemainingMaterialCosts = Settings.CraftRequirements.GetMaterialCosts();
            RemainingWork = Settings.CraftRequirements.WorkCost;
            Durability = MaxDurability;
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
        
        /// <summary>
        /// The percentage of durability remaining ex: 0.5 = 50%
        /// </summary>
        [JsonIgnore]
        public float DurabilityPercent
        {
            get
            {
                var percent = (float)Durability / (float)MaxDurability;
                return percent;
            }
        }

        [JsonIgnore]
        public float ConstructionPercent
        {
            get
            {
                if (State != EConstructionState.Built)
                {
                    return 1 - (RemainingWork / Settings.CraftRequirements.WorkCost);
                }
                else
                {
                    return 1f;
                }
            }
        }
    }