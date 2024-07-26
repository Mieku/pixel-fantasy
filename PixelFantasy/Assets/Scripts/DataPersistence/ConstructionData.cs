using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using ScriptableObjects;
using Systems.Buildings.Scripts;
using UnityEngine;
using UnityEngine.Serialization;

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
        public string UniqueID;
        public string SettingsID;
        public float Durability;
        public EConstructionState State;
        public float RemainingWork;
        public List<CostData> RemainingMaterialCosts;
        public List<CostData> PendingResourceCosts = new List<CostData>(); // Claimed by a task but not used yet
        public List<CostData> IncomingResourceCosts = new List<CostData>(); // The item is on its way
        public List<string> IncomingItemsUIDs = new List<string>();
        public List<string> ReceivedItemUIDs = new List<string>();
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
            UniqueID = CreateUID();
            RemainingMaterialCosts = Settings.CraftRequirements.GetMaterialCosts();
            RemainingWork = Settings.CraftRequirements.WorkCost;
            Durability = MaxDurability;
        }
        
        protected string CreateUID()
        {
            return $"{Settings.ConstructionName}_{Guid.NewGuid()}";
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

        public void AddToReceivedItems(ItemData itemData)
        {
            ReceivedItemUIDs.Add(itemData.UniqueID);
            itemData.State = EItemState.BeingProcessed;
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