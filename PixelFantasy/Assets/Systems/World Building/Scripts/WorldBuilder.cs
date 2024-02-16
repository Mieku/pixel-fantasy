using System;
using System.Collections.Generic;
using System.Linq;
using Handlers;
using ScriptableObjects;
using Sirenix.OdinInspector;
using TWC;
using UnityEngine;
using UnityEngine.Tilemaps;
using Random = UnityEngine.Random;

namespace Systems.World_Building.Scripts
{
    public class WorldBuilder : MonoBehaviour
    {
        [SerializeField] private TileWorldCreator _tileWorldCreator;
        [SerializeField] private BiomeData _currentBiome;
        [SerializeField] private WorldSpawner _spawner;
        [SerializeField] private MountainsHandler _mountainsHandler;
        [SerializeField] private Transform _resourcesParent;

        [SerializeField] private Tilemap _grassTilemap;
        [SerializeField] private RuleTile _grassRuleTile;
        [SerializeField] private Tilemap _waterTilemap;
        [SerializeField] private RuleTile _waterRuleTile;
        [SerializeField] private Tilemap _elevationTilemap;
        [SerializeField] private RuleTile _elevationRuleTile;
        
        [SerializeField] private Tilemap _groundCoverTilemap;
        [SerializeField] private RuleTile _dirtRuleTile;
        [SerializeField] private RuleTile _forestFloorRuleTile;

        [SerializeField] private GameObject _starterStockPile;
        
        [Button("Generate Plane")]
        private void GeneratePlane()
        {
            _tileWorldCreator.twcAsset = _currentBiome.WorldCreatorAsset;
            _tileWorldCreator.ExecuteAllBlueprintLayers();

            RefreshPlane();

            SpawnResources();
        }

        [Button("Refresh Plane")]
        private void RefreshPlane()
        {
            ClearAllTilemaps();
            
            var blueprintLayers = _tileWorldCreator.twcAsset.mapBlueprintLayers;
            
            var grassBlueprint = blueprintLayers.Find(layer => layer.layerName == "Grass");
            if (grassBlueprint != null)
            {
                BuildTilemap(grassBlueprint.map, _grassTilemap, _grassRuleTile);
            }
            
            var waterBlueprint = blueprintLayers.Find(layer => layer.layerName == "Water");
            if (waterBlueprint != null)
            {
                BuildTilemap(waterBlueprint.map, _waterTilemap, _waterRuleTile);
            }
            
            var elevationBlueprint = blueprintLayers.Find(layer => layer.layerName == "Elevation");
            if (elevationBlueprint != null)
            {
                BuildTilemap(elevationBlueprint.map, _elevationTilemap, _elevationRuleTile);
            }
            
            var dirtBlueprint = blueprintLayers.Find(layer => layer.layerName == "Dirt");
            if (dirtBlueprint != null)
            {
                BuildTilemap(dirtBlueprint.map, _groundCoverTilemap, _dirtRuleTile);
            }
            
            var elevatedDirtBlueprint = blueprintLayers.Find(layer => layer.layerName == "Elevated Dirt");
            if (elevatedDirtBlueprint != null)
            {
                BuildTilemap(elevatedDirtBlueprint.map, _groundCoverTilemap, _dirtRuleTile);
            }
            
            
            DetermineStartPosition(blueprintLayers.Find(layer => layer.layerName == "Start Points"));
        }

        [Button("Clear All Tilemaps")]
        private void ClearAllTilemaps()
        {
            _grassTilemap.ClearAllTiles();
            _waterTilemap.ClearAllTiles();
            _elevationTilemap.ClearAllTiles();
            _groundCoverTilemap.ClearAllTiles();
            
            _mountainsHandler.DeleteChildren();
        }

        private void DetermineStartPosition(TileWorldCreatorAsset.BlueprintLayerData layerData)
        {
            List<Vector3Int> startpoints = new List<Vector3Int>();
            var map = layerData.map;
            for (int x = 0; x < map.GetLength(0); x++)
            {
                for (int y = 0; y < map.GetLength(1); y++)
                {
                    var cellStart = new Vector3Int(x * 2, y * 2, 0);
                    if (map[x, y])
                    {
                        startpoints.Add(cellStart);
                    }
                }
            }

            if (startpoints.Count == 0)
            {
                Debug.LogError("No start point found");
            }

            var random = Random.Range(0, startpoints.Count);
            _starterStockPile.transform.position = startpoints[random];
        }

        private void BuildTilemap(bool [,] blueprint, Tilemap tileMap, RuleTile ruleTile)
        {
            for (int x = 0; x < blueprint.GetLength(0); x++)
            {
                for (int y = 0; y < blueprint.GetLength(1); y++)
                {
                    var cellStart = new Vector3Int(x * 2, y * 2, 0);
                    if (blueprint[x, y])
                    {
                        tileMap.SetTile(cellStart, ruleTile);
                        tileMap.SetTile(cellStart + new Vector3Int(0 , 1, 0), ruleTile);
                        tileMap.SetTile(cellStart + new Vector3Int(1 , 0, 0), ruleTile);
                        tileMap.SetTile(cellStart + new Vector3Int(1 , 1, 0), ruleTile);
                    }
                }
            }
        }

        [Button("Spawn Resources")]
        private void SpawnResources()
        {
            _spawner.Init(_currentBiome, _resourcesParent);
            
            var blueprintLayers = _tileWorldCreator.twcAsset.mapBlueprintLayers;
            var mountainsBlueprint = blueprintLayers.Find(layer => layer.layerName == "Mountains");
            if (mountainsBlueprint != null)
            {
                SpawnMountains(mountainsBlueprint.map);
            }
        }

        private void SpawnMountains(bool [,] mountainsBlueprint)
        {
            _mountainsHandler.DeleteChildren();
            
            // Scale the blueprint up so one cell is 2x2 cells
            bool[,] scaledBlueprint =
                new bool[mountainsBlueprint.GetLength(0) * 2, mountainsBlueprint.GetLength(1) * 2];
            
            for (int x = 0; x < mountainsBlueprint.GetLength(0); x++)
            {
                for (int y = 0; y < mountainsBlueprint.GetLength(1); y++)
                {
                    var cellStart = new Vector2Int(x * 2, y * 2);

                    scaledBlueprint[cellStart.x, cellStart.y] = mountainsBlueprint[x, y];
                    scaledBlueprint[cellStart.x, cellStart.y + 1] = mountainsBlueprint[x, y];
                    scaledBlueprint[cellStart.x + 1, cellStart.y] = mountainsBlueprint[x, y];
                    scaledBlueprint[cellStart.x + 1, cellStart.y + 1] = mountainsBlueprint[x, y];
                }
            }

            // Initialize cluster centers based on the biome data
            MountainTileType[,] tileMap = new MountainTileType[scaledBlueprint.GetLength(0), scaledBlueprint.GetLength(1)];
            Dictionary<MountainTileType, List<Vector2Int>> clusterCenters;
            InitializeClusterCenters(scaledBlueprint, _currentBiome, out clusterCenters);

            // Expand clusters to create organic distributions of mountain types
            ExpandClusters(scaledBlueprint, clusterCenters, ref tileMap, _currentBiome);
            for (int x = 0; x < tileMap.GetLength(0); x++)
            {
                for (int y = 0; y < tileMap.GetLength(1); y++)
                {
                    var mountainType = tileMap[x, y];
                    if (mountainType != MountainTileType.Empty)
                    {
                        var mountainData = _currentBiome.GetMountainData(mountainType);
                        _mountainsHandler.SpawnMountain(mountainData, x + 0.5f, y + 0.5f);
                    }
                }
            }
            _mountainsHandler.Debug_MountainStats();
        }
        
        private void InitializeClusterCenters(bool[,] scaledBlueprint, BiomeData biomeData, out Dictionary<MountainTileType, List<Vector2Int>> clusterCenters)
        {
            clusterCenters = new Dictionary<MountainTileType, List<Vector2Int>>();

            // Assuming TileType and a way to relate it with biomeData's MountainData
            foreach (var mountainType in Enum.GetValues(typeof(MountainTileType)).Cast<MountainTileType>())
            {
                if (mountainType == MountainTileType.Empty) continue; // Skip the 'Empty' type

                int centerCount = CalculateCenterCountForMountainType(mountainType, biomeData, scaledBlueprint); // Implement this based on biome data
        
                clusterCenters[mountainType] = new List<Vector2Int>();

                for (int i = 0; i < centerCount; i++)
                {
                    Vector2Int center;
                    do
                    {
                        center = new Vector2Int(Random.Range(0, scaledBlueprint.GetLength(0)), Random.Range(0, scaledBlueprint.GetLength(1)));
                    } while (!scaledBlueprint[center.x, center.y] || clusterCenters[mountainType].Contains(center)); // Ensure uniqueness and correct placement
            
                    clusterCenters[mountainType].Add(center);
                }
            }
        }

        private int CalculateCenterCountForMountainType(MountainTileType mountainType, BiomeData biomeData, bool[,] scaledBlueprint)
        {
            int totalMountainCells = 0;
            for (int x = 0; x < scaledBlueprint.GetLength(0); x++)
            {
                for (int y = 0; y < scaledBlueprint.GetLength(1); y++)
                {
                    if (scaledBlueprint[x, y]) totalMountainCells++;
                }
            }

            // Assuming each cell represents a 2x2 area, adjust the calculation if necessary
            float percentage = biomeData.GetMountainTypePercentage(mountainType);
            int centerCount = Mathf.RoundToInt(totalMountainCells * percentage);

            return centerCount;
        }
        
        private void ExpandClusters(bool[,] scaledBlueprint, Dictionary<MountainTileType, List<Vector2Int>> clusterCenters, ref MountainTileType[,] tileMap, BiomeData biomeData)
        {
            // Calculate the maximum number of tiles allowed for each mountain type based on biome data
            var maxTilesPerType = CalculateMaxTilesPerType(scaledBlueprint, biomeData);
            var currentTilesPerType = new Dictionary<MountainTileType, int>();
            foreach (var type in maxTilesPerType.Keys)
            {
                currentTilesPerType[type] = 0; // Initialize counting for each type
            }

            // Expand clusters for each non-default mountain type, respecting the maximum number of tiles
            foreach (var mountainType in clusterCenters.Keys)
            {
                // Skip the default type during the initial expansion phase
                if (mountainType == biomeData.DefaultMountainType) continue;

                foreach (var center in clusterCenters[mountainType])
                {
                    Queue<Vector2Int> frontier = new Queue<Vector2Int>();
                    frontier.Enqueue(center);

                    while (frontier.Count > 0 && currentTilesPerType[mountainType] < maxTilesPerType[mountainType])
                    {
                        Vector2Int current = frontier.Dequeue();
                        if (IsWithinBounds(current, scaledBlueprint) && scaledBlueprint[current.x, current.y] && tileMap[current.x, current.y] == MountainTileType.Empty)
                        {
                            tileMap[current.x, current.y] = mountainType;
                            currentTilesPerType[mountainType]++;

                            Vector2Int[] neighbors = {
                                new Vector2Int(current.x, current.y + 1),
                                new Vector2Int(current.x, current.y - 1),
                                new Vector2Int(current.x + 1, current.y),
                                new Vector2Int(current.x - 1, current.y)
                            };

                            foreach (var neighbor in neighbors)
                            {
                                if (IsWithinBounds(neighbor, scaledBlueprint) && scaledBlueprint[neighbor.x, neighbor.y] && tileMap[neighbor.x, neighbor.y] == MountainTileType.Empty)
                                {
                                    frontier.Enqueue(neighbor);
                                }
                            }
                        }
                    }
                }
            }

            // Redistribute tiles to correct minor discrepancies before filling with the default type
            RedistributeTiles(ref tileMap, currentTilesPerType, maxTilesPerType, scaledBlueprint);

            // Fill in the remaining unassigned spaces with the default mountain type
            FillRemainingWithDefault(scaledBlueprint, ref tileMap, biomeData.DefaultMountainType);
        }

        private void FillRemainingWithDefault(bool[,] scaledBlueprint, ref MountainTileType[,] tileMap, MountainTileType defaultType)
        {
            for (int x = 0; x < tileMap.GetLength(0); x++)
            {
                for (int y = 0; y < tileMap.GetLength(1); y++)
                {
                    // Check if the current tile is within the mountainous area and is unassigned
                    if (scaledBlueprint[x, y] && tileMap[x, y] == MountainTileType.Empty)
                    {
                        tileMap[x, y] = defaultType; // Assign the default mountain type
                    }
                }
            }
        }
        
        private void RedistributeTiles(ref MountainTileType[,] tileMap, Dictionary<MountainTileType, int> currentTilesPerType, Dictionary<MountainTileType, int> maxTilesPerType, bool[,] scaledBlueprint)
        {
            // Identify over and underrepresented types
            var overrepresented = new List<MountainTileType>();
            var underrepresented = new List<MountainTileType>();

            foreach (var type in currentTilesPerType.Keys)
            {
                if (type == MountainTileType.Empty) continue; // Skip empty type
                if (currentTilesPerType[type] > maxTilesPerType[type])
                {
                    overrepresented.Add(type);
                }
                else if (currentTilesPerType[type] < maxTilesPerType[type])
                {
                    underrepresented.Add(type);
                }
            }

            // Attempt to redistribute tiles from over to underrepresented types
            foreach (var overType in overrepresented)
            {
                int excess = currentTilesPerType[overType] - maxTilesPerType[overType];
                foreach (var underType in underrepresented)
                {
                    int needed = maxTilesPerType[underType] - currentTilesPerType[underType];
                    if (needed <= 0) continue; // This type no longer needs additional tiles

                    for (int x = 0; x < tileMap.GetLength(0) && excess > 0 && needed > 0; x++)
                    {
                        for (int y = 0; y < tileMap.GetLength(1) && excess > 0 && needed > 0; y++)
                        {
                            // Check if this tile can be converted: it must be overrepresented and not part of the scaled blueprint's non-mountainous area
                            if (tileMap[x, y] == overType && scaledBlueprint[x, y])
                            {
                                tileMap[x, y] = underType; // Convert tile type
                                excess--;
                                needed--;
                                currentTilesPerType[overType]--;
                                currentTilesPerType[underType]++;
                            }
                        }
                    }

                    // Update the list if needed is satisfied for this underrepresented type
                    if (needed <= 0) underrepresented.Remove(underType);
                }
            }
        }

        
        private Dictionary<MountainTileType, int> CalculateMaxTilesPerType(bool[,] scaledBlueprint, BiomeData biomeData)
        {
            int totalMountainCells = 0;
            for (int x = 0; x < scaledBlueprint.GetLength(0); x++)
            {
                for (int y = 0; y < scaledBlueprint.GetLength(1); y++)
                {
                    if (scaledBlueprint[x, y]) totalMountainCells++;
                }
            }

            var maxTilesPerType = new Dictionary<MountainTileType, int>();
            foreach (var mountain in biomeData.Mountains)
            {
                int maxTiles = Mathf.RoundToInt(totalMountainCells * mountain.spawnPercentage);
                maxTilesPerType[mountain.GetMountainTileType()] = maxTiles;
            }

            return maxTilesPerType;
        }
        
        private bool IsWithinBounds(Vector2Int position, bool[,] array)
        {
            return position.x >= 0 && position.y >= 0 && position.x < array.GetLength(0) && position.y < array.GetLength(1);
        }
    }
}
