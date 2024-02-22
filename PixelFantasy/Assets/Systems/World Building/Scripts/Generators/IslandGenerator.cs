using System;
using System.Collections.Generic;
using System.Linq;
using TWC;
using TWC.Actions;
using TWC.editor;
using TWC.Utilities;
using UnityEditor;
using UnityEngine;

namespace Systems.World_Building.Scripts.Generators
{
    [ActionCategory(Category = ActionCategoryAttribute.CategoryTypes.Generators)]
    [ActionName(Name = "Island Generator")]
    public class IslandGenerator : TWCBlueprintAction, ITWCAction
    {
        public int numberOfSteps = 2;
        public int deathLimit = 4;
        public int birthLimit = 4;
        public bool keepOnlyLargestIsland = true; // Add this setting
        public float minIslandAreaPercentage = 50f; // Minimum percentage of the map that must be land


        private bool[,] newMap;
        private int height;
        private int width;
        private TWCGUILayout guiLayout;

        // Added for shape and complexity control
        public float coastlineComplexity = 0.5f; // Adjust complexity: 0 (simple) to 1 (complex)

#if UNITY_EDITOR
        public override void DrawGUI(Rect _rect, int _layerIndex, TileWorldCreatorAsset _asset, TileWorldCreator _twc)
        {
            using (guiLayout = new TWCGUILayout(_rect))
            {
                guiLayout.Add();
                numberOfSteps = EditorGUI.IntField(guiLayout.rect, "Number of Steps", numberOfSteps);
                guiLayout.Add();
                deathLimit = EditorGUI.IntField(guiLayout.rect, "Death Limit", deathLimit);
                guiLayout.Add();
                birthLimit = EditorGUI.IntField(guiLayout.rect, "Birth Limit", birthLimit);
                guiLayout.Add();
                coastlineComplexity = EditorGUI.Slider(guiLayout.rect, "Coastline Complexity", coastlineComplexity, 0f, 1f);
                guiLayout.Add();
                keepOnlyLargestIsland = EditorGUI.Toggle(guiLayout.rect, "Keep Only Largest Island", keepOnlyLargestIsland); // Add checkbox for new setting
                guiLayout.Add();
                minIslandAreaPercentage = EditorGUI.Slider(guiLayout.rect, "Min Island Area (%)", minIslandAreaPercentage, 0f, 100f);
            }
        }
#endif

        public ITWCAction Clone()
        {
            var clone = new IslandGenerator();
    
            // Existing settings
            clone.numberOfSteps = this.numberOfSteps;
            clone.deathLimit = this.deathLimit;
            clone.birthLimit = this.birthLimit;
            clone.coastlineComplexity = this.coastlineComplexity;
    
            // New setting
            clone.keepOnlyLargestIsland = this.keepOnlyLargestIsland; // Copy the setting to the clone
    
            return clone;
        }
        
        public float GetGUIHeight()
        {
            return guiLayout != null ? guiLayout.height : 18;
        }

        public bool[,] Execute(bool[,] map, TileWorldCreator _twc)
        {
            height = map.GetLength(1);
            width = map.GetLength(0);

            UnityEngine.Random.InitState(_twc.currentSeed);

            newMap = Generate();
            return TileWorldCreatorUtilities.MergeMap(map, newMap);
        }

        public bool[,] Generate()
        {
            newMap = new bool[width, height];
            InitialiseMap(newMap); // Initialize the map with your chosen method

            // Perform cellular automata steps
            for (int i = 0; i < numberOfSteps; i++)
            {
                newMap = DoSimulationStep(newMap);
            }

            AdjustCoastlineComplexity(newMap); // Optional, adjust coastline complexity if needed

            if (keepOnlyLargestIsland) // Keep only the largest island, if applicable
            {
                KeepOnlyLargestIsland(newMap);
            }

            EnsureMinimumIslandArea(newMap); // Ensure the island meets the minimum area requirement
            FillSmallHoles(newMap); // New method to fill small holes
            SmoothIslandEdges(newMap); // Smooth the edges after expansion

            return newMap;
        }
        
        private void FillSmallHoles(bool[,] map)
        {
            bool[,] visited = new bool[width, height];
            List<Vector2Int> fillCandidates = new List<Vector2Int>();

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    if (!visited[x, y] && !map[x, y]) // If it's water and not visited
                    {
                        fillCandidates.Clear();
                        bool isEnclosed = CheckEnclosedWater(x, y, map, visited, fillCandidates);

                        if (isEnclosed && fillCandidates.Count < 10) // Arbitrary size limit for small holes
                        {
                            foreach (var point in fillCandidates)
                            {
                                map[point.x, point.y] = true; // Fill the hole
                            }
                        }
                    }
                }
            }
        }

        private bool CheckEnclosedWater(int x, int y, bool[,] map, bool[,] visited, List<Vector2Int> fillCandidates)
        {
            if (x < 0 || x >= width || y < 0 || y >= height) return false; // Initial bounds check (might be redundant here but safe to keep)

            Queue<Vector2Int> queue = new Queue<Vector2Int>();
            queue.Enqueue(new Vector2Int(x, y));
            bool enclosed = true;

            while (queue.Count > 0)
            {
                Vector2Int current = queue.Dequeue();
                if (current.x < 0 || current.x >= width || current.y < 0 || current.y >= height)
                {
                    enclosed = false; // Reached the boundary, so it's not enclosed.
                    continue;
                }
                if (visited[current.x, current.y] || map[current.x, current.y]) continue; // Already visited or it's land.

                visited[current.x, current.y] = true;
                fillCandidates.Add(current); // Add as a candidate for filling.

                // Enqueue all unvisited, valid neighbors
                EnqueueIfValid(queue, current.x + 1, current.y, map, visited);
                EnqueueIfValid(queue, current.x - 1, current.y, map, visited);
                EnqueueIfValid(queue, current.x, current.y + 1, map, visited);
                EnqueueIfValid(queue, current.x, current.y - 1, map, visited);
            }

            return enclosed;
        }

        private void EnqueueIfValid(Queue<Vector2Int> queue, int x, int y, bool[,] map, bool[,] visited)
        {
            if (x >= 0 && x < width && y >= 0 && y < height && !visited[x, y] && !map[x, y])
            {
                queue.Enqueue(new Vector2Int(x, y));
            }
        }

        private void SmoothIslandEdges(bool[,] map)
        {
            bool[,] tmpMap = new bool[width, height];

            // Copy the original map to a temporary one
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    tmpMap[x, y] = map[x, y];
                }
            }

            // Apply smoothing rules
            for (int x = 1; x < width - 1; x++)
            {
                for (int y = 1; y < height - 1; y++)
                {
                    int neighbors = CountNeighbours(x, y, tmpMap);

                    // A cell becomes land if it was land and has 4 or more neighbors, or if it was water and has more than 5 neighbors
                    map[x, y] = tmpMap[x, y] ? (neighbors >= 4) : (neighbors > 5);
                }
            }
        }

        
        private void InitialiseMap(bool[,] map)
        {
            // Clear the map first
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    map[x, y] = false; // Start with water everywhere
                }
            }
            
            InitialiseIrregular(map);
        }

        bool[,] DoSimulationStep(bool[,] oldMap)
        {
            bool[,] tmpMap = new bool[width, height];
            // Consider slightly lowering the death limit and adjusting the birth limit to encourage expansion
            int adjustedDeathLimit = deathLimit; // You might lower this if you find the island breaking up too much
            int adjustedBirthLimit = birthLimit; // Lowering this encourages more cells to become land

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    int neighbors = CountNeighbours(x, y, oldMap);
                    bool isAlive = oldMap[x, y];
                    // Adjust rules to favor land cohesion
                    tmpMap[x, y] = (isAlive && neighbors >= adjustedDeathLimit) || (!isAlive && neighbors > adjustedBirthLimit);
                }
            }
            return tmpMap;
        }

        private void EnsureMinimumIslandArea(bool[,] map)
        {
            int totalCells = width * height;
            int requiredLandCells = Mathf.RoundToInt(totalCells * (minIslandAreaPercentage / 100f));
            int currentLandCells = 0;

            // Calculate current land cells
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    if (map[x, y]) currentLandCells++;
                }
            }

            // Define a list to hold potential expansion points
            List<Vector2Int> expansionPoints = new List<Vector2Int>();

            // Populate initial expansion points based on existing land cells
            for (int x = 1; x < width - 1; x++)
            {
                for (int y = 1; y < height - 1; y++)
                {
                    if (map[x, y] && CountNeighbours(x, y, map) < 8) // Edge of the island
                    {
                        AddExpansionPoints(expansionPoints, x, y, map);
                    }
                }
            }

            int iterations = 0; // Safety mechanism
            while (currentLandCells < requiredLandCells && iterations < 10000) // Limit iterations to prevent infinite loop
            {
                if (expansionPoints.Count == 0) break; // Exit if no expansion points are available

                // Randomly select an expansion point to turn into land
                int index = UnityEngine.Random.Range(0, expansionPoints.Count);
                Vector2Int point = expansionPoints[index];
                if (!map[point.x, point.y]) // If currently water
                {
                    map[point.x, point.y] = true;
                    currentLandCells++;
                    AddExpansionPoints(expansionPoints, point.x, point.y, map); // Add new expansion points around the new land cell
                }
                expansionPoints.RemoveAt(index); // Remove the chosen point from expansion points

                iterations++;
            }
        }

        private void AddExpansionPoints(List<Vector2Int> expansionPoints, int x, int y, bool[,] map)
        {
            for (int i = -1; i <= 1; i++)
            {
                for (int j = -1; j <= 1; j++)
                {
                    if (i == 0 && j == 0) continue; // Skip the center point
                    int newX = x + i;
                    int newY = y + j;
                    if (newX > 0 && newX < width - 1 && newY > 0 && newY < height - 1 && !map[newX, newY])
                    {
                        expansionPoints.Add(new Vector2Int(newX, newY));
                    }
                }
            }
        }


        int CountNeighbours(int x, int y, bool[,] map)
        {
            int count = 0;
            for (int i = -1; i <= 1; i++)
            {
                for (int j = -1; j <= 1; j++)
                {
                    int neighbourX = x + i;
                    int neighbourY = y + j;
                    if (i == 0 && j == 0) continue;
                    if (neighbourX >= 0 && neighbourX < width && neighbourY >= 0 && neighbourY < height && map[neighbourX, neighbourY])
                    {
                        count++;
                    }
                }
            }
            return count;
        }

        private void InitialiseIrregular(bool[,] map)
        {
            float noiseScale = 0.1f; // Adjust for more or less granularity
            // Define margins to keep the island generation away from the edges
            int margin = Mathf.Min(width, height) / 5; // Example margin calculation

            for (int x = margin; x < width - margin; x++)
            {
                for (int y = margin; y < height - margin; y++)
                {
                    float xCoord = (float)(x - margin) / (width - 2 * margin) * noiseScale;
                    float yCoord = (float)(y - margin) / (height - 2 * margin) * noiseScale;
                    float sample = Mathf.PerlinNoise(xCoord + UnityEngine.Random.value * 100, yCoord + UnityEngine.Random.value * 100);
                    map[x, y] = sample > 0.5; // Adjust threshold for more or less land
                }
            }
        }

        
        private void AdjustCoastlineComplexity(bool[,] map)
        {
            // Iterate through each cell in the map, excluding the borders for simplicity
            for (int x = 1; x < width - 1; x++)
            {
                for (int y = 1; y < height - 1; y++)
                {
                    // Calculate the number of land neighbors
                    var neighbors = CountNeighbours(x, y, map);

                    // Apply adjustments based on the coastline complexity setting
                    if (map[x, y]) // If the current cell is land
                    {
                        // If the land cell has fewer than 2 land neighbors, consider turning it into water to increase complexity
                        if (neighbors < 2 && UnityEngine.Random.value < coastlineComplexity)
                        {
                            map[x, y] = false;
                        }
                    }
                    else // If the current cell is water
                    {
                        // If the water cell is surrounded by at least 3 land cells, consider turning it into land to smooth out the coastline
                        if (neighbors > 3 && UnityEngine.Random.value > coastlineComplexity)
                        {
                            map[x, y] = true;
                        }
                    }
                }
            }
        }

        private void KeepOnlyLargestIsland(bool[,] map)
        {
            List<List<Vector2Int>> islands = new List<List<Vector2Int>>();
            bool[,] visited = new bool[width, height];

            // Flood fill to find islands
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    if (!visited[x, y] && map[x, y])
                    {
                        List<Vector2Int> island = new List<Vector2Int>();
                        FloodFillIsland(map, x, y, visited, island);
                        islands.Add(island);
                    }
                }
            }

            // Find the largest island
            List<Vector2Int> largestIsland = islands.OrderByDescending(isl => isl.Count).FirstOrDefault();

            // Remove all but the largest island
            if (largestIsland != null)
            {
                for (int x = 0; x < width; x++)
                {
                    for (int y = 0; y < height; y++)
                    {
                        if (!largestIsland.Contains(new Vector2Int(x, y)))
                        {
                            map[x, y] = false; // Remove this part of an island
                        }
                    }
                }
            }
        }

        private void FloodFillIsland(bool[,] map, int x, int y, bool[,] visited, List<Vector2Int> island)
        {
            if (x < 0 || x >= width || y < 0 || y >= height) return; // Out of bounds
            if (visited[x, y] || !map[x, y]) return; // Already visited or water

            visited[x, y] = true;
            island.Add(new Vector2Int(x, y));

            // Recursive flood fill
            FloodFillIsland(map, x + 1, y, visited, island);
            FloodFillIsland(map, x - 1, y, visited, island);
            FloodFillIsland(map, x, y + 1, visited, island);
            FloodFillIsland(map, x, y - 1, visited, island);
        }
    }
}
