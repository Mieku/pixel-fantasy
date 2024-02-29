using System.Collections.Generic;
using System.Linq;
using Characters;
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

        public List<T> FindFurnituresOfType<T>()
        {
            return _allFurniture.OfType<T>().ToList();
        }
        
        public T GetClosestFurnitureOfType<T>(Vector2 requestorPos)
        {
            var allFurnituressOfType = FindFurnituresOfType<T>();
            List<(T, float)> furnitureDistances = new List<(T, float)>();
            foreach (var furnitureT  in allFurnituressOfType)
            {
                var furniture = furnitureT as Furniture;
                if (furniture != null && furniture.FurnitureState == Furniture.EFurnitureState.Built)
                {
                    var furniturePos = furniture.UseagePosition(requestorPos);
                    if (furniturePos != null)
                    {
                        var pathResult = Helper.DoesPathExist((Vector2)furniturePos, requestorPos);
                        if (pathResult.pathExists)
                        {
                            float distance = Helper.GetPathLength(pathResult.navMeshPath);
                            furnitureDistances.Add((furnitureT, distance));
                        }
                    }
                }
            }

            if (furnitureDistances.Count == 0)
            {
                return default;
            }
            
            var sortedFurnitures = furnitureDistances.OrderBy(x => x.Item2).Select(x => x.Item1).ToList();
            var selectedFurniture = sortedFurnitures[0];
            return selectedFurniture;
        }

        public BedFurniture FindClosestUnclaimedBed(Kinling kinling)
        {
            Vector2 requestorPos = kinling.transform.position;
            var allBeds = FindFurnituresOfType<BedFurniture>().Where(b => b.IsUnassigned(kinling));
            
            List<(BedFurniture, float)> furnitureDistances = new List<(BedFurniture, float)>();
            foreach (var bed in allBeds)
            {
                if (bed.FurnitureState == Furniture.EFurnitureState.Built)
                {
                    var furniturePos = bed.UseagePosition(requestorPos);
                    if (furniturePos != null)
                    {
                        var pathResult = Helper.DoesPathExist((Vector2)furniturePos, requestorPos);
                        if (pathResult.pathExists)
                        {
                            float distance = Helper.GetPathLength(pathResult.navMeshPath);
                            furnitureDistances.Add((bed, distance));
                        }
                    }
                }
            }

            if (furnitureDistances.Count == 0)
            {
                return default;
            }
            
            var sortedFurnitures = furnitureDistances.OrderBy(x => x.Item2).Select(x => x.Item1).ToList();
            var selectedFurniture = sortedFurnitures[0];
            return selectedFurniture;
        }

        public CraftingTable GetCraftingTableForItem(CraftedItemData item)
        {
            var allTables = FindFurnituresOfType<CraftingTable>();
            foreach (var table in allTables)
            {
                if (table.CanCraftItem(item))
                {
                    return table;
                }
            }

            return null;
        }
    }
}
