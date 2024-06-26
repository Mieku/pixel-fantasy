using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Data.Resource;
using Handlers;
using Sirenix.OdinInspector;
using Systems.Game_Setup.Scripts;
using TWC;
using UnityEngine;
using UnityEngine.Tilemaps;
using Debug = UnityEngine.Debug;
using Random = UnityEngine.Random;

namespace Systems.World_Building.Scripts
{
    public class WorldBuilder : MonoBehaviour
    {
        [SerializeField] private TileWorldCreator _tileWorldCreator;
        [SerializeField] private BiomeSettings _currentBiome;
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

        public Vector3Int StartPos;
        private List<TileWorldCreatorAsset.BlueprintLayerData> _blueprintLayers;

        public Vector2Int WorldSize
        {
            get
            {
                Vector2Int result = new Vector2Int();
                result.x = 36;
                result.y = 36;
                return result;
            }
        }

        public IEnumerator GenerateAreaCoroutine(List<TileWorldCreatorAsset.BlueprintLayerData> blueprintLayers)
        {
            Debug.Log("Beginning World Generation...");
            _blueprintLayers = blueprintLayers;

            // Calculate total steps
            int totalSteps = CalculateTotalSteps();
            LoadingScreen.Instance.Show("Generating World", "Initializing...", totalSteps);

            // Allow frame to render and update UI/loading screen here
            Stopwatch stopwatch = new Stopwatch();

            stopwatch.Start();
            yield return StartCoroutine(GenerateTilesCoroutine());
            yield return StartCoroutine(SpawnResourcesCoroutine());
            stopwatch.Stop();
            Debug.Log($"World was generated, took {stopwatch.ElapsedMilliseconds} ms");

            LoadingScreen.Instance.Hide();
        }

        private int CalculateTotalSteps()
        {
            int steps = 0;

            // Count steps for GenerateTilesCoroutine
            if (_blueprintLayers.Find(layer => layer.layerName == "Grass") != null) steps++;
            if (_blueprintLayers.Find(layer => layer.layerName == "Water") != null) steps++;
            if (_blueprintLayers.Find(layer => layer.layerName == "Elevation") != null) steps++;
            if (_blueprintLayers.Find(layer => layer.layerName == "Dirt") != null) steps++;
            if (_blueprintLayers.Find(layer => layer.layerName == "Elevated Dirt") != null) steps++;
            steps++; // For determining start position

            // Count steps for SpawnResourcesCoroutine
            if (_blueprintLayers.Find(layer => layer.layerName == "Mountains") != null) steps++;
            if (_blueprintLayers.Find(layer => layer.layerName == "Forest") != null) steps++;
            if (_blueprintLayers.Find(layer => layer.layerName == "Vegetation") != null) steps++;
            if (_blueprintLayers.Find(layer => layer.layerName == "Additionals") != null) steps++;

            return steps;
        }

        public IEnumerator GenerateTilesCoroutine()
        {
            ClearAllTilemaps();

            // Perform operations, yielding as necessary
            yield return null;

            LoadingScreen.Instance.SetLoadingInfoText("Generating Grass Tiles...");
            var grassBlueprint = _blueprintLayers.Find(layer => layer.layerName == "Grass");
            if (grassBlueprint != null)
            {
                yield return StartCoroutine(BuildTilemap(grassBlueprint.map, _grassTilemap, _grassRuleTile));
                LoadingScreen.Instance.StepCompleted();
            }

            yield return null;

            LoadingScreen.Instance.SetLoadingInfoText("Generating Water Tiles...");
            var waterBlueprint = _blueprintLayers.Find(layer => layer.layerName == "Water");
            if (waterBlueprint != null)
            {
                yield return StartCoroutine(BuildTilemap(waterBlueprint.map, _waterTilemap, _waterRuleTile));
                LoadingScreen.Instance.StepCompleted();
            }

            yield return null;

            LoadingScreen.Instance.SetLoadingInfoText("Generating Elevation Tiles...");
            var elevationBlueprint = _blueprintLayers.Find(layer => layer.layerName == "Elevation");
            if (elevationBlueprint != null && elevationBlueprint.active)
            {
                yield return StartCoroutine(BuildTilemap(elevationBlueprint.map, _elevationTilemap, _elevationRuleTile));

                var rampsBlueprint = _blueprintLayers.Find(layer => layer.layerName == "Ramps");
                if (rampsBlueprint != null)
                {
                    yield return StartCoroutine(SpawnRamps(rampsBlueprint.map, elevationBlueprint.map));
                }
                LoadingScreen.Instance.StepCompleted();
            }

            yield return null;

            LoadingScreen.Instance.SetLoadingInfoText("Generating Dirt Tiles...");
            var dirtBlueprint = _blueprintLayers.Find(layer => layer.layerName == "Dirt");
            if (dirtBlueprint != null)
            {
                yield return StartCoroutine(BuildTilemap(dirtBlueprint.map, _groundCoverTilemap, _dirtRuleTile));
                LoadingScreen.Instance.StepCompleted();
            }

            yield return null;

            var elevatedDirtBlueprint = _blueprintLayers.Find(layer => layer.layerName == "Elevated Dirt");
            if (elevatedDirtBlueprint != null)
            {
                yield return StartCoroutine(BuildTilemap(elevatedDirtBlueprint.map, _groundCoverTilemap, _dirtRuleTile));
                LoadingScreen.Instance.StepCompleted();
            }

            yield return null;

            LoadingScreen.Instance.SetLoadingInfoText("Determining Start Position...");
            yield return StartCoroutine(DetermineStartPosition(_blueprintLayers.Find(layer => layer.layerName == "Start Points")));
            LoadingScreen.Instance.StepCompleted();

            yield return null;
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
            StartPos = startpoints[random];

            yield return null;
        }

        private IEnumerator BuildTilemap(bool[,] blueprint, Tilemap tileMap, RuleTile ruleTile)
        {
            var tiles = new List<Vector3Int>();
            var ruleTiles = new List<RuleTile>();

            for (int x = 0; x < blueprint.GetLength(0); x++)
            {
                for (int y = 0; y < blueprint.GetLength(1); y++)
                {
                    var cellStart = new Vector3Int(x * 2, y * 2, 0);
                    if (blueprint[x, y])
                    {
                        tiles.Add(cellStart);
                        tiles.Add(cellStart + new Vector3Int(0, 1, 0));
                        tiles.Add(cellStart + new Vector3Int(1, 0, 0));
                        tiles.Add(cellStart + new Vector3Int(1, 1, 0));
                        ruleTiles.Add(ruleTile);
                        ruleTiles.Add(ruleTile);
                        ruleTiles.Add(ruleTile);
                        ruleTiles.Add(ruleTile);
                    }
                }
            }

            tileMap.SetTiles(tiles.ToArray(), ruleTiles.ToArray());

            yield return null;
        }

        public IEnumerator SpawnResourcesCoroutine()
        {
            Stopwatch stopwatch = new Stopwatch();

            yield return null;

            var mountainsBlueprint = _blueprintLayers.Find(layer => layer.layerName == "Mountains");
            if (mountainsBlueprint != null)
            {
                stopwatch.Start();
                LoadingScreen.Instance.SetLoadingInfoText("Building Mountains...");
                yield return StartCoroutine(SpawnMountains(mountainsBlueprint.map));
                stopwatch.Stop();
                Debug.Log($"Building Mountains Complete in {stopwatch.ElapsedMilliseconds} ms");
                LoadingScreen.Instance.StepCompleted();
            }

            _resourcesHandler.DeleteResources();

            yield return null;

            var forestBlueprint = _blueprintLayers.Find(layer => layer.layerName == "Forest");
            if (forestBlueprint != null)
            {
                stopwatch.Restart();
                LoadingScreen.Instance.SetLoadingInfoText("Building Forest...");
                yield return StartCoroutine(SpawnForest(forestBlueprint.map));
                stopwatch.Stop();
                Debug.Log($"Building Forest Complete in {stopwatch.ElapsedMilliseconds} ms");
                LoadingScreen.Instance.StepCompleted();
            }

            yield return null;

            var vegetationBlueprint = _blueprintLayers.Find(layer => layer.layerName == "Vegetation");
            if (vegetationBlueprint != null)
            {
                stopwatch.Restart();
                LoadingScreen.Instance.SetLoadingInfoText("Building Vegetation...");
                yield return StartCoroutine(SpawnVegetation(vegetationBlueprint.map));
                stopwatch.Stop();
                Debug.Log($"Building Vegetation Complete in {stopwatch.ElapsedMilliseconds} ms");
                LoadingScreen.Instance.StepCompleted();
            }

            yield return null;

            var additionalsBlueprint = _blueprintLayers.Find(layer => layer.layerName == "Additionals");
            if (additionalsBlueprint != null)
            {
                stopwatch.Restart();
                LoadingScreen.Instance.SetLoadingInfoText("Building Additionals...");
                yield return StartCoroutine(SpawnAdditionals(additionalsBlueprint.map));
                stopwatch.Stop();
                Debug.Log($"Building Additionals Complete in {stopwatch.ElapsedMilliseconds} ms");
                LoadingScreen.Instance.StepCompleted();
            }

            yield return null;
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

                        bool n = y + 1 < elevationBlueprint.GetLength(1) && elevationBlueprint[x, y + 1];
                        bool e = x + 1 < elevationBlueprint.GetLength(0) && elevationBlueprint[x + 1, y];
                        bool s = y > 0 && elevationBlueprint[x, y - 1];
                        bool w = x > 0 && elevationBlueprint[x - 1, y];
                        bool ne = x + 1 < elevationBlueprint.GetLength(0) && y + 1 < elevationBlueprint.GetLength(1) && elevationBlueprint[x + 1, y + 1];
                        bool nw = x > 0 && y + 1 < elevationBlueprint.GetLength(1) && elevationBlueprint[x - 1, y + 1];
                        bool sw = x > 0 && y > 0 && elevationBlueprint[x - 1, y - 1];
                        bool se = x + 1 < elevationBlueprint.GetLength(0) && y > 0 && elevationBlueprint[x + 1, y - 1];

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
            int width = vegetationBlueprint.GetLength(0);
            int height = vegetationBlueprint.GetLength(1);
            int maxVegetationCount = _currentBiome.MaxVegetationPerCluster;

            HashSet<Vector2Int> placedPositions = new HashSet<Vector2Int>();

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    if (vegetationBlueprint[x, y] && Random.Range(0, 100) < _currentBiome.VegetationDensity)
                    {
                        Vector2Int pos = new Vector2Int(x, y);

                        if (!placedPositions.Contains(pos))
                        {
                            placedPositions.Add(pos);

                            float offsetX = Random.Range(0f, 1.75f); // Adjust this value to ensure it fits within the cell
                            float offsetY = Random.Range(0f, 1.75f); // Adjust this value to ensure it fits within the cell
                            Vector3 worldPosition = new Vector3((pos.x * 2) + offsetX, (pos.y * 2) + offsetY, 0); // Adjust the multiplication factor according to your world's scale

                            var vegetationType = _currentBiome.GetRandomVegetation();
                            _resourcesHandler.SpawnResource(vegetationType, worldPosition);
                        }
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
        




        private Dictionary<MountainTileType, List<Vector2Int>> InitializeClusterCentersParallel(bool[,] scaledBlueprint, BiomeSettings biomeSettings)
        {
            var clusterCenters = new ConcurrentDictionary<MountainTileType, List<Vector2Int>>();

            foreach (var mountainType in Enum.GetValues(typeof(MountainTileType)).Cast<MountainTileType>())
            {
                if (mountainType == MountainTileType.Empty) continue;

                int centerCount = CalculateCenterCountForMountainType(mountainType, biomeSettings, scaledBlueprint);
                var centers = new List<Vector2Int>();

                for (int i = 0; i < centerCount; i++)
                {
                    Vector2Int center;
                    do
                    {
                        center = new Vector2Int(Random.Range(0, scaledBlueprint.GetLength(0)), Random.Range(0, scaledBlueprint.GetLength(1)));
                    } while (!scaledBlueprint[center.x, center.y] || centers.Contains(center));

                    centers.Add(center);
                }

                clusterCenters[mountainType] = centers;
            }

            return new Dictionary<MountainTileType, List<Vector2Int>>(clusterCenters);
        }


       private IEnumerator SpawnMountains(bool[,] mountainsBlueprint)
{
    _mountainsHandler.DeleteMountains();

    bool[,] scaledBlueprint = new bool[mountainsBlueprint.GetLength(0) * 2, mountainsBlueprint.GetLength(1) * 2];

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

    var tileMap = new MountainTileType[scaledBlueprint.GetLength(0), scaledBlueprint.GetLength(1)];

    var stopwatch = new Stopwatch();
    stopwatch.Start();
    var clusterCenters = InitializeClusterCentersParallel(scaledBlueprint, _currentBiome);
    stopwatch.Stop();
    Debug.Log($"Initializing cluster centers took {stopwatch.ElapsedMilliseconds} ms");

    stopwatch.Restart();
    ExpandClustersParallel(scaledBlueprint, clusterCenters, tileMap, _currentBiome);
    stopwatch.Stop();
    Debug.Log($"Expanding clusters took {stopwatch.ElapsedMilliseconds} ms");

    stopwatch.Restart();
    yield return StartCoroutine(BatchSpawnMountainsAsync(tileMap));
    stopwatch.Stop();
    Debug.Log($"Batch spawning mountains took {stopwatch.ElapsedMilliseconds} ms");
}

private IEnumerator BatchSpawnMountainsAsync(MountainTileType[,] tileMap)
{
    var mountainTiles = new List<Vector3Int>();
    var mountainSettings = new List<MountainSettings>();

    for (int x = 0; x < tileMap.GetLength(0); x++)
    {
        for (int y = 0; y < tileMap.GetLength(1); y++)
        {
            var mountainType = tileMap[x, y];
            if (mountainType != MountainTileType.Empty)
            {
                var mountainData = _currentBiome.GetMountainSettings(mountainType);
                mountainTiles.Add(new Vector3Int(x, y, 0));
                mountainSettings.Add(mountainData);
            }
        }
    }

    yield return StartCoroutine(_mountainsHandler.BatchSpawnMountainsAsync(mountainTiles, mountainSettings));
}




        
private void ExpandClustersParallel(bool[,] scaledBlueprint, Dictionary<MountainTileType, List<Vector2Int>> clusterCenters, MountainTileType[,] tileMap, BiomeSettings biomeSettings)
{
    var maxTilesPerType = CalculateMaxTilesPerType(scaledBlueprint, biomeSettings);
    var currentTilesPerType = new ConcurrentDictionary<MountainTileType, int>();

    foreach (var type in maxTilesPerType.Keys)
    {
        currentTilesPerType[type] = 0;
    }

    Parallel.ForEach(clusterCenters.Keys, mountainType =>
    {
        if (mountainType == biomeSettings.DefaultMountainType) return;

        foreach (var center in clusterCenters[mountainType])
        {
            Queue<Vector2Int> frontier = new Queue<Vector2Int>();
            frontier.Enqueue(center);

            while (frontier.Count > 0 && currentTilesPerType[mountainType] < maxTilesPerType[mountainType])
            {
                Vector2Int current = frontier.Dequeue();
                if (IsWithinBounds(current, scaledBlueprint) && scaledBlueprint[current.x, current.y] && tileMap[current.x, current.y] == MountainTileType.Empty)
                {
                    lock (tileMap)
                    {
                        tileMap[current.x, current.y] = mountainType;
                        currentTilesPerType[mountainType]++;
                    }

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
    });

    FillRemainingWithDefault(scaledBlueprint, tileMap, biomeSettings.DefaultMountainType);
}

        //
        // private IEnumerator SpawnMountains(bool[,] mountainsBlueprint)
        // {
        //     _mountainsHandler.DeleteMountains();
        //
        //     // Scale the blueprint up so one cell is 2x2 cells
        //     bool[,] scaledBlueprint = new bool[mountainsBlueprint.GetLength(0) * 2, mountainsBlueprint.GetLength(1) * 2];
        //
        //     for (int x = 0; x < mountainsBlueprint.GetLength(0); x++)
        //     {
        //         for (int y = 0; y < mountainsBlueprint.GetLength(1); y++)
        //         {
        //             var cellStart = new Vector2Int(x * 2, y * 2);
        //
        //             scaledBlueprint[cellStart.x, cellStart.y] = mountainsBlueprint[x, y];
        //             scaledBlueprint[cellStart.x, cellStart.y + 1] = mountainsBlueprint[x, y];
        //             scaledBlueprint[cellStart.x + 1, cellStart.y] = mountainsBlueprint[x, y];
        //             scaledBlueprint[cellStart.x + 1, cellStart.y + 1] = mountainsBlueprint[x, y];
        //         }
        //     }
        //
        //     // Initialize cluster centers based on the biome data
        //     MountainTileType[,] tileMap = new MountainTileType[scaledBlueprint.GetLength(0), scaledBlueprint.GetLength(1)];
        //     Dictionary<MountainTileType, List<Vector2Int>> clusterCenters;
        //     InitializeClusterCenters(scaledBlueprint, _currentBiome, out clusterCenters);
        //
        //     // Expand clusters to create organic distributions of mountain types
        //     ExpandClustersParallel(scaledBlueprint, clusterCenters, tileMap, _currentBiome);
        //
        //     // Batch spawn mountains
        //     for (int x = 0; x < tileMap.GetLength(0); x++)
        //     {
        //         for (int y = 0; y < tileMap.GetLength(1); y++)
        //         {
        //             var mountainType = tileMap[x, y];
        //             if (mountainType != MountainTileType.Empty)
        //             {
        //                 var mountainData = _currentBiome.GetMountainSettings(mountainType);
        //                 _mountainsHandler.SpawnMountain(mountainData, x + 0.5f, y + 0.5f);
        //             }
        //         }
        //     }
        //
        //     yield return null;
        // }

        // private void InitializeClusterCenters(bool[,] scaledBlueprint, BiomeSettings biomeSettings, out Dictionary<MountainTileType, List<Vector2Int>> clusterCenters)
        // {
        //     clusterCenters = new Dictionary<MountainTileType, List<Vector2Int>>();
        //
        //     foreach (var mountainType in Enum.GetValues(typeof(MountainTileType)).Cast<MountainTileType>())
        //     {
        //         if (mountainType == MountainTileType.Empty) continue;
        //
        //         int centerCount = CalculateCenterCountForMountainType(mountainType, biomeSettings, scaledBlueprint);
        //
        //         clusterCenters[mountainType] = new List<Vector2Int>();
        //
        //         for (int i = 0; i < centerCount; i++)
        //         {
        //             Vector2Int center;
        //             do
        //             {
        //                 center = new Vector2Int(Random.Range(0, scaledBlueprint.GetLength(0)), Random.Range(0, scaledBlueprint.GetLength(1)));
        //             } while (!scaledBlueprint[center.x, center.y] || clusterCenters[mountainType].Contains(center));
        //
        //             clusterCenters[mountainType].Add(center);
        //         }
        //     }
        // }

        private int CalculateCenterCountForMountainType(MountainTileType mountainType, BiomeSettings biomeSettings, bool[,] scaledBlueprint)
        {
            int totalMountainCells = 0;
            for (int x = 0; x < scaledBlueprint.GetLength(0); x++)
            {
                for (int y = 0; y < scaledBlueprint.GetLength(1); y++)
                {
                    if (scaledBlueprint[x, y]) totalMountainCells++;
                }
            }

            float percentage = biomeSettings.GetMountainTypePercentage(mountainType);
            int centerCount = Mathf.RoundToInt(totalMountainCells * percentage);

            return centerCount;
        }

        // private void ExpandClustersParallel(bool[,] scaledBlueprint, Dictionary<MountainTileType, List<Vector2Int>> clusterCenters, MountainTileType[,] tileMap, BiomeSettings biomeSettings)
        // {
        //     var maxTilesPerType = CalculateMaxTilesPerType(scaledBlueprint, biomeSettings);
        //     var currentTilesPerType = new ConcurrentDictionary<MountainTileType, int>();
        //
        //     foreach (var type in maxTilesPerType.Keys)
        //     {
        //         currentTilesPerType[type] = 0;
        //     }
        //
        //     Parallel.ForEach(clusterCenters.Keys, mountainType =>
        //     {
        //         if (mountainType == biomeSettings.DefaultMountainType) return;
        //
        //         foreach (var center in clusterCenters[mountainType])
        //         {
        //             Queue<Vector2Int> frontier = new Queue<Vector2Int>();
        //             frontier.Enqueue(center);
        //
        //             while (frontier.Count > 0 && currentTilesPerType[mountainType] < maxTilesPerType[mountainType])
        //             {
        //                 Vector2Int current = frontier.Dequeue();
        //                 if (IsWithinBounds(current, scaledBlueprint) && scaledBlueprint[current.x, current.y] && tileMap[current.x, current.y] == MountainTileType.Empty)
        //                 {
        //                     lock (tileMap)
        //                     {
        //                         tileMap[current.x, current.y] = mountainType;
        //                         currentTilesPerType[mountainType]++;
        //                     }
        //
        //                     Vector2Int[] neighbors = {
        //                         new Vector2Int(current.x, current.y + 1),
        //                         new Vector2Int(current.x, current.y - 1),
        //                         new Vector2Int(current.x + 1, current.y),
        //                         new Vector2Int(current.x - 1, current.y)
        //                     };
        //
        //                     foreach (var neighbor in neighbors)
        //                     {
        //                         if (IsWithinBounds(neighbor, scaledBlueprint) && scaledBlueprint[neighbor.x, neighbor.y] && tileMap[neighbor.x, neighbor.y] == MountainTileType.Empty)
        //                         {
        //                             frontier.Enqueue(neighbor);
        //                         }
        //                     }
        //                 }
        //             }
        //         }
        //     });
        //
        //     FillRemainingWithDefault(scaledBlueprint, tileMap, biomeSettings.DefaultMountainType);
        // }

        private void FillRemainingWithDefault(bool[,] scaledBlueprint, MountainTileType[,] tileMap, MountainTileType defaultType)
        {
            for (int x = 0; x < tileMap.GetLength(0); x++)
            {
                for (int y = 0; y < tileMap.GetLength(1); y++)
                {
                    if (scaledBlueprint[x, y] && tileMap[x, y] == MountainTileType.Empty)
                    {
                        tileMap[x, y] = defaultType;
                    }
                }
            }
        }

        private Dictionary<MountainTileType, int> CalculateMaxTilesPerType(bool[,] scaledBlueprint, BiomeSettings biomeSettings)
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
            foreach (var mountain in biomeSettings.Mountains)
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
