using System.Collections.Generic;
using Items;
using Managers;
using ScriptableObjects;
using UnityEngine;

namespace Handlers
{
    public class FurnitureManager : Singleton<FurnitureManager>
    {
        private List<Furniture> _allFurniture = new List<Furniture>();

        public bool DoesFurnitureExist(FurnitureItemData furnitureData)
        {
            foreach (var furniture in _allFurniture)
            {
                if (furniture.FurnitureItemData == furnitureData)
                {
                    return true;
                }
            }

            return false;
        }
        
        public void RegisterFurniture(Furniture furniture)
        {
            if (_allFurniture.Contains(furniture))
            {
                Debug.LogError($"Attempted to register already registered furniture: {furniture.FurnitureItemData.ItemName}");
                return;
            }
            
            _allFurniture.Add(furniture);
        }

        public void DeregisterFurniture(Furniture furniture)
        {
            if (!_allFurniture.Contains(furniture))
            {
                Debug.LogError($"Attempted to deregister not registered furniture: {furniture.FurnitureItemData.ItemName}");
                return;
            }

            _allFurniture.Remove(furniture);
        }
    }
}
