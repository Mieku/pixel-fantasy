using System;
using System.Collections;
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
        [SerializeField] private MountainsHandler _mountainsHandler;
        [SerializeField] private ResourcesHandler _resourcesHandler;
        [SerializeField] private RampsHandler _rampsHandler;

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
            StartCoroutine(GeneratePlaneCoroutine());
        }
        
        public IEnumerator GeneratePlaneCoroutine()
        {
            // Immediate operations
            _tileWorldCreator.twcAsset = _currentBiome.WorldCreatorAsset;
            _tileWorldCreator.ExecuteAllBlueprintLayers();

            // Allow frame to render and update UI/loading screen here
            yield return StartCoroutine(RefreshPlaneCoroutine());
            
            yield return StartCoroutine(SpawnResourcesCoroutine());
        }
        
        public IEnumerator RefreshPlaneCoroutine()
        {
            ClearAllTilemaps();
            // Perform operations, yielding as necessary
            yield return null;
            // Continue with other steps
            
            var blueprintLayers = _tileWorldCreator.twcAsset.mapBlueprintLayers;
            
            var grassBlueprint = blueprintLayers.Find(layer => layer.layerName == "Grass");
            if (grassBlueprint != null)
            {
                yield return StartCoroutine(BuildTilemap(grassBlueprint.map, _grassTilemap, _grassRuleTile));
            }
            
            var waterBlueprint = blueprintLayers.Find(layer => layer.layerName == "Water");
            if (waterBlueprint != null)
            {
                yield return StartCoroutine(BuildTilemap(waterBlueprint.map, _waterTilemap, _waterRuleTile));
            }
            
            var elevationBlueprint = blueprintLayers.Find(layer => layer.layerName == "Elevation");
            if (elevationBlueprint != null)
            {
                yield return StartCoroutine(BuildTilemap(elevationBlueprint.map, _elevationTilemap, _elevationRuleTile));
                
                var rampsBlueprint = blueprintLayers.Find(layer => layer.layerName == "Ramps");
                if (rampsBlueprint != null)
                {
                    yield return StartCoroutine(SpawnRamps(rampsBlueprint.map, elevationBlueprint.map));
                }
            }
            
            var dirtBlueprint = blueprintLayers.Find(layer => layer.layerName == "Dirt");
            if (dirtBlueprint != null)
            {
                yield return StartCoroutine(BuildTilemap(dirtBlueprint.map, _groundCoverTilemap, _dirtRuleTile));
            }
            
            var elevatedDirtBlueprint = blueprintLayers.Find(layer => layer.layerName == "Elevated Dirt");
            if (elevatedDirtBlueprint != null)
            {
                yield return StartCoroutine(BuildTilemap(elevatedDirtBlueprint.map, _groundCoverTilemap, _dirtRuleTile));
            }
            
            yield return StartCoroutine(DetermineStartPosition(blueprintLayers.Find(layer => layer.layerName == "Start Points")));
        }

        [Button("Clear All Tilemaps")]
        private void ClearAllTilemaps()
        {
            _grassTilemap.ClearAllTiles();
            _waterTilemap.ClearAllTiles();
            _elevationTilemap.ClearAllTiles();
            _groundCoverTilemap.ClearAllTiles();
            
            _mountainsHandler.DeleteMountains();
            _rampsHandler.DeleteRamps();
            _resourcesHandler.DeleteResources();
        }

        private IEnumerator DetermineStartPosition(TileWorldCreatorAsset.BlueprintLayerData layerData)
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

            yield return null;
        }

        private IEnumerator BuildTilemap(bool [,] blueprint, Tilemap tileMap, RuleTile ruleTile)
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

            yield return null;
        }

        private IEnumerator SpawnResourcesCoroutine()
        {
            var blueprintLayers = _tileWorldCreator.twcAsset.mapBlueprintLayers;
            var mountainsBlueprint = blueprintLayers.Find(layer => layer.layerName == "Mountains");
            if (mountainsBlueprint != null)
            {
                yield return StartCoroutine(SpawnMountains(mountainsBlueprint.map));
            }

            _resourcesHandler.DeleteResources();
            
            var forestBlueprint = blueprintLayers.Find(layer => layer.layerName == "Forest");
            if (forestBlueprint != null)
            {
                yield return StartCoroutine(SpawnForest(forestBlueprint.map));
            }

            var vegitationBlueprint = blueprintLayers.Find(layer => layer.layerName == "Vegitation");
            if (vegitationBlueprint != null)
            {
                yield return StartCoroutine(SpawnVegetation(vegitationBlueprint.map));
            }
            
            var additionalsBlueprint = blueprintLayers.Find(layer => layer.layerName == "Additionals");
            if (additionalsBlueprint != null)
            {
                yield return StartCoroutine(SpawnAdditionals(additionalsBlueprint.map));
            }
        }

        private IEnumerator SpawnRamps(bool[,] rampsBlueprint, bool[,] elevationBlueprint)
        {
            _rampsHandler.DeleteRamps();
            
            for (int x = 0; x < rampsBlueprint.GetLength(0); x++)
            {
                for (int y = 0; y < rampsBlueprint.GetLength(1); y++)
                {
                    if (rampsBlueprint[x, y])
                    {
                        var cellStart = new Vector3Int(x * 2, y * 2, 0);
                        
                        bool n = false;
                        if (elevationBlueprint.GetLength(1) >= y + 1)
                        {
                            n = elevationBlueprint[x, y + 1];
                        }
                        
                        bool e = false;
                        if (elevationBlueprint.GetLength(0) >= x + 1)
                        {
                            e = elevationBlueprint[x + 1, y];
                        }
                        
                        bool s = false;
                        if (y != 0)
                        {
                            s = elevationBlueprint[x, y - 1];
                        }
                        
                        bool w = false;
                        if (x != 0)
                        {
                            w = elevationBlueprint[x - 1, y];
                        }
                        
                        bool ne = false;
                        if (elevationBlueprint.GetLength(0) >= x + 1 && elevationBlueprint.GetLength(1) >= y + 1)
                        {
                            ne = elevationBlueprint[x + 1, y + 1];
                        }
                        
                        bool nw = false;
                        if (x != 0 && elevationBlueprint.GetLength(1) >= y + 1)
                        {
                            nw = elevationBlueprint[x - 1, y + 1];
                        }
                        
                        bool sw = false;
                        if (x != 0 && y != 0)
                        {
                            sw = elevationBlueprint[x - 1, y - 1];
                        }
                        
                        bool se = false;
                        if (elevationBlueprint.GetLength(0) >= x + 1 && y != 0)
                        {
                            se = elevationBlueprint[x + 1, y - 1];
                        }

                        // North
                        if (!n && !ne && !nw && e && w)
                        {
                            _rampsHandler.SpawnRamp(ERampDirection.North, cellStart.x + 1f, cellStart.y + 1.5f);
                        }
                        
                        // East
                        if (!e && !ne && !se && n && s)
                        {
                            _rampsHandler.SpawnRamp(ERampDirection.East, cellStart.x + 1.5f, cellStart.y + 1f);
                        }
                        
                        // South
                        if (!s && !sw && !se && w && e)
                        {
                            _rampsHandler.SpawnRamp(ERampDirection.South, cellStart.x + 1f, cellStart.y + 0.5f);
                        }
                        
                        // West
                        if (!w && !nw && !sw && s && n)
                        {
                            _rampsHandler.SpawnRamp(ERampDirection.West, cellStart.x + 0.5f, cellStart.y + 1f);
                        }
                    }
                }
            }

            yield return null;
        }

        private IEnumerator SpawnAdditionals(bool[,] additionalsBlueprint)
        {
            // Scale the blueprint up so one cell is 2x2 cells
            bool[,] scaledBlueprint =
                new bool[additionalsBlueprint.GetLength(0) * 2, additionalsBlueprint.GetLength(1) * 2];
            
            for (int x = 0; x < additionalsBlueprint.GetLength(0); x++)
            {
                for (int y = 0; y < additionalsBlueprint.GetLength(1); y++)
                {
                    var cellStart = new Vector2Int(x * 2, y * 2);

                    scaledBlueprint[cellStart.x, cellStart.y] = additionalsBlueprint[x, y];
                    scaledBlueprint[cellStart.x, cellStart.y + 1] = additionalsBlueprint[x, y];
                    scaledBlueprint[cellStart.x + 1, cellStart.y] = additionalsBlueprint[x, y];
                    scaledBlueprint[cellStart.x + 1, cellStart.y + 1] = additionalsBlueprint[x, y];
                }
            }

            for (int x = 0; x < scaledBlueprint.GetLength(0); x++)
            {
                for (int y = 0; y < scaledBlueprint.GetLength(1); y++)
                {
                    if (scaledBlueprint[x, y] && Helper.RollDice(_currentBiome.AdditionalsChanceToSpawn))
                    {
                        float offsetX = Random.Range(0f, 1.75f); // Adjust this value to ensure it fits within the cell
                        float offsetY = Random.Range(0f, 1.75f); // Adjust this value to ensure it fits within the cell
                        
                        float posX = Random.Range(0.1f, 0.9f);
                        float posY = Random.Range(0.1f, 0.9f);
                        var spawnPos = new Vector2(x + posX, y + posY);
                        
                        var vegetationType = _currentBiome.GetRandomAdditional();
                        _resourcesHandler.SpawnResource(vegetationType, spawnPos);
                    }
                }
            }
            
            yield return null;
        }

        private IEnumerator SpawnVegetation(bool[,] vegetationBlueprint)
        {
            int clusterRadius = _currentBiome.VegitationClusterRadius;
            int maxVegetationPerCluster = _currentBiome.MaxVegetationPerCluster;
            
            // Directly use the original blueprint for iterating through potential vegetation centers
            List<Vector2Int> potentialCenters = new List<Vector2Int>();
            for (int x = 0; x < vegetationBlueprint.GetLength(0); x++)
            {
                for (int y = 0; y < vegetationBlueprint.GetLength(1); y++)
                {
                    if (vegetationBlueprint[x, y]) // Position is marked for potential vegetation
                    {
                        potentialCenters.Add(new Vector2Int(x, y));
                    }
                }
            }

            // Shuffle the list of potential centers to randomize cluster starting points
            potentialCenters = potentialCenters.OrderBy(a => Guid.NewGuid()).ToList();

            // Create clusters
            foreach (var center in potentialCenters)
            {
                int vegetationCount = Random.Range(1, maxVegetationPerCluster + 1);
                List<Vector2Int> placedPositions = new List<Vector2Int>();

                for (int i = 0; i < vegetationCount; i++)
                {
                    Vector2Int spawnPos;
                    int attempts = 0;
                    do
                    {
                        int offsetX = Random.Range(-clusterRadius, clusterRadius + 1);
                        int offsetY = Random.Range(-clusterRadius, clusterRadius + 1);
                        spawnPos = new Vector2Int(center.x + offsetX, center.y + offsetY);

                        bool withinBounds = spawnPos.x >= 0 && spawnPos.y >= 0 && spawnPos.x < vegetationBlueprint.GetLength(0) && spawnPos.y < vegetationBlueprint.GetLength(1);
                        if (withinBounds && vegetationBlueprint[spawnPos.x, spawnPos.y] && !placedPositions.Contains(spawnPos))
                        {
                            break; // Found a valid position
                        }
                        attempts++;
                    } while (attempts < 20); // Limit attempts to avoid infinite loops

                    if (attempts < 20)
                    {
                        // Apply a precise random offset within each cell for more organic yet contained placement
                        float offsetX = Random.Range(0f, 1.75f); // Adjust this value to ensure it fits within the cell
                        float offsetY = Random.Range(0f, 1.75f); // Adjust this value to ensure it fits within the cell
                        Vector3 worldPosition = new Vector3((spawnPos.x * 2) + offsetX, (spawnPos.y * 2) + offsetY, 0); // Adjust the multiplication factor according to your world's scale
                        var vegetationType = _currentBiome.GetRandomVegitation();
                        _resourcesHandler.SpawnResource(vegetationType, worldPosition);
                        placedPositions.Add(spawnPos);
                    }
                }
            }

            yield return null;
        }


        private IEnumerator SpawnForest(bool[,] forestBlueprint)
        {
            // Define a minimum distance between resources
            float minDistanceBetweenResources = 1.0f; // Adjust as needed

            for (int x = 0; x < forestBlueprint.GetLength(0); x++)
            {
                for (int y = 0; y < forestBlueprint.GetLength(1); y++)
                {
                    if (forestBlueprint[x, y])
                    {
                        var cellStart = new Vector2Int(x * 2, y * 2);
                        List<Vector2> usedPositions = new List<Vector2>();
                        int randTreeAmount = Random.Range(1, _currentBiome.ForestTreeDensity);

                        // Spawn trees while ensuring they don't get too close to each other
                        for (int i = 0; i < randTreeAmount; i++)
                        {
                            Vector2 spawnPos;
                            int attemptCounter = 0;
                            do
                            {
                                float posX = Random.Range(0.1f, 1.9f);
                                float posY = Random.Range(0.1f, 1.9f);
                                spawnPos = new Vector2(cellStart.x + posX, cellStart.y + posY);
                                attemptCounter++;
                            } while (Helper.IsTooCloseToOthers(spawnPos, usedPositions, minDistanceBetweenResources) && attemptCounter < 10);

                            if (attemptCounter < 10) // Ensures a position was found that satisfies the minimum distance constraint
                            {
                                var forestTree = _currentBiome.GetRandomForestTree();
                                _resourcesHandler.SpawnResource(forestTree, spawnPos);
                                usedPositions.Add(spawnPos);
                            }
                        }

                        // Now, spawn additional resources with the same distance constraints
                        int randAdditionalAmount = Random.Range(0, _currentBiome.ForestAdditionalDensity);
                        for (int i = 0; i < randAdditionalAmount; i++)
                        {
                            Vector2 spawnPos;
                            int attemptCounter = 0;
                            do
                            {
                                float posX = Random.Range(0.1f, 1.9f);
                                float posY = Random.Range(0.1f, 1.9f);
                                spawnPos = new Vector2(cellStart.x + posX, cellStart.y + posY);
                                attemptCounter++;
                            } while (Helper.IsTooCloseToOthers(spawnPos, usedPositions, minDistanceBetweenResources) && attemptCounter < 10);

                            if (attemptCounter < 10) // Ensures a position was found that satisfies the minimum distance constraint
                            {
                                var forestResource = _currentBiome.GetRandomForestAdditional();
                                _resourcesHandler.SpawnResource(forestResource, spawnPos);
                                usedPositions.Add(spawnPos);
                            }
                        }
                    }
                }
            }

            yield return null;
        }

        private IEnumerator SpawnMountains(bool [,] mountainsBlueprint)
        {
            _mountainsHandler.DeleteMountains();
            
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

            yield return null;
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
