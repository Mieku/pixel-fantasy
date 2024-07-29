using System.Collections.Generic;
using System.Linq;
using Characters;
using DataPersistence;
using Items;
using Managers;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Handlers
{
    public class FurnitureDatabase : Singleton<FurnitureDatabase>
    {
        [ShowInInspector] private Dictionary<string, FurnitureData> _registeredFurniture = new Dictionary<string, FurnitureData>();
        
        public void RegisterFurniture(FurnitureData furniture)
        {
            _registeredFurniture[furniture.UniqueID] = furniture;
        }

        public void DeregisterFurniture(FurnitureData furniture)
        {
            _registeredFurniture.Remove(furniture.UniqueID);
        }

        public FurnitureData Query(string uid)
        {
            return _registeredFurniture[uid];
        }

        public Dictionary<string, FurnitureData> SaveFurnitureData()
        {
            return _registeredFurniture;
        }
        
        public Furniture FindFurnitureObject(string uniqueID)
        {
            var allFurniture = transform.GetComponentsInChildren<Furniture>();
            foreach (var furniture in allFurniture)
            {
                if (furniture.RuntimeData.UniqueID == uniqueID)
                {
                    return furniture;
                }
            }

            return null;
        }

        public void LoadFurnitureData(Dictionary<string, FurnitureData> data)
        {
            foreach (var furnitureData in data)
            {
                SpawnLoadedFurniture(furnitureData.Value);
            }
        }

        public Furniture SpawnLoadedFurniture(FurnitureData data)
        {
            var prefab = data.FurnitureSettings.FurniturePrefab;
            var furniture = Instantiate(prefab, data.Position, Quaternion.identity, transform);
            furniture.name = data.ItemName;
            furniture.LoadData(data);
            RegisterFurniture(data);
            PlayerInteractableDatabase.Instance.RegisterPlayerInteractable(furniture);
            return furniture;
        }

        public void ClearAllFurniture()
        {
            var furnitures = transform.GetComponentsInChildren<Furniture>();
            foreach (var furniture in furnitures.Reverse())
            {
                DestroyImmediate(furniture.gameObject);
            }
   
            _registeredFurniture.Clear();
        }

        public List<T> FindFurnituresOfType<T>()
        {
            return _registeredFurniture.OfType<T>().ToList();
        }
        
        public T GetClosestFurnitureOfType<T>(Vector2 requestorPos)
        {
            var allFurnituressOfType = FindFurnituresOfType<T>();
            List<(T, float)> furnitureDistances = new List<(T, float)>();
            foreach (var furnitureT  in allFurnituressOfType)
            {
                var furniture = furnitureT as Furniture;
                if (furniture != null && furniture.RuntimeData.FurnitureState == EFurnitureState.Built)
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
                if (bed.RuntimeData.FurnitureState == EFurnitureState.Built)
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

        public CraftingTable GetCraftingTableForItem(CraftedItemSettings item)
        {
            var allTables = FindFurnituresOfType<CraftingTable>();
            foreach (var table in allTables)
            {
                if (table.RuntimeTableData.CanCraftItem(item))
                {
                    return table;
                }
            }

            return null;
        }
    }
}
