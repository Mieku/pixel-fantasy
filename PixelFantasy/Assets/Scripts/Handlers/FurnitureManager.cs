using System.Collections.Generic;
using System.Linq;
using Characters;
using Data.Item;
using Items;
using Managers;
using ScriptableObjects;
using UnityEngine;
using FurnitureData = Data.Item.FurnitureData;

namespace Handlers
{
    public class FurnitureManager : Singleton<FurnitureManager>
    {
        private List<FurnitureData> _allFurniture = new List<FurnitureData>();
        public List<FurnitureData> AllFurniture => _allFurniture;

        public bool DoesFurnitureExist(FurnitureData furnitureData)
        {
            foreach (var furniture in _allFurniture)
            {
                if (furniture == furnitureData)
                {
                    return true;
                }
            }

            return false;
        }
        
        public void RegisterFurniture(FurnitureData furniture)
        {
            if (_allFurniture.Contains(furniture))
            {
                Debug.LogError($"Attempted to register already registered furniture: {furniture.guid}");
                return;
            }
            
            _allFurniture.Add(furniture);
        }

        public void DeregisterFurniture(FurnitureData furniture)
        {
            if (!_allFurniture.Contains(furniture))
            {
                //Debug.LogError($"Attempted to deregister not registered furniture: {furniture.guid}");
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
                var furniture = furnitureT as FurnitureData;
                if (furniture != null && furniture.State == EFurnitureState.Built)
                {
                    var furniturePos = furniture.LinkedFurniture.UseagePosition(requestorPos);
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
                if (bed.RuntimeData.State == EFurnitureState.Built)
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
                if (table.TableData.CanCraftItem(item))
                {
                    return table;
                }
            }

            return null;
        }
    }
}
