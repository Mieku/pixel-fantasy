using System;
using System.Collections.Generic;
using TWC;
using TWC.Actions;
using TWC.editor;
using TWC.Utilities;
using UnityEditor;
using UnityEngine;

namespace Systems.World_Building.Scripts.Generators
{
    [ActionCategory(Category = ActionCategoryAttribute.CategoryTypes.Generators)]
    [ActionName(Name = "Coast Generator")]
    public class CoastGenerator : TWCBlueprintAction, ITWCAction
    {
        public bool topLeftFilled = true;
        public bool topRightFilled = true;
        public bool bottomLeftFilled = true;
        public bool bottomRightFilled = true;
        public float noiseScale = 10f; // Exposed variable for noise scale
        public int edgeOffset = 5; // Exposed variable for edge offset
        public int smoothIterations = 5; // Number of smoothing iterations

        private bool[,] newMap;
        private int height;
        private int width;
        private TWCGUILayout guiLayout;
        private float noiseOffsetX;
        private float noiseOffsetY;

#if UNITY_EDITOR
        public override void DrawGUI(Rect _rect, int _layerIndex, TileWorldCreatorAsset _asset, TileWorldCreator _twc)
        {
            using (guiLayout = new TWCGUILayout(_rect))
            {
                guiLayout.Add();
                topLeftFilled = EditorGUI.Toggle(guiLayout.rect, "Top Left Filled", topLeftFilled);
                guiLayout.Add();
                topRightFilled = EditorGUI.Toggle(guiLayout.rect, "Top Right Filled", topRightFilled);
                guiLayout.Add();
                bottomLeftFilled = EditorGUI.Toggle(guiLayout.rect, "Bottom Left Filled", bottomLeftFilled);
                guiLayout.Add();
                bottomRightFilled = EditorGUI.Toggle(guiLayout.rect, "Bottom Right Filled", bottomRightFilled);
                guiLayout.Add();
                noiseScale = EditorGUI.FloatField(guiLayout.rect, "Noise Scale", noiseScale);
                guiLayout.Add();
                edgeOffset = EditorGUI.IntField(guiLayout.rect, "Edge Offset", edgeOffset);
                guiLayout.Add();
                smoothIterations = EditorGUI.IntField(guiLayout.rect, "Smooth Iterations", smoothIterations);
            }
        }
#endif

        public ITWCAction Clone()
        {
            var clone = new CoastGenerator
            {
                topLeftFilled = this.topLeftFilled,
                topRightFilled = this.topRightFilled,
                bottomLeftFilled = this.bottomLeftFilled,
                bottomRightFilled = this.bottomRightFilled,
                noiseScale = this.noiseScale,
                edgeOffset = this.edgeOffset,
                smoothIterations = this.smoothIterations
            };
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

            // Generate random offsets for Perlin noise
            noiseOffsetX = UnityEngine.Random.Range(0f, 100f);
            noiseOffsetY = UnityEngine.Random.Range(0f, 100f);

            newMap = Generate();
            return TileWorldCreatorUtilities.MergeMap(map, newMap);
        }

        public bool[,] Generate()
        {
            newMap = new bool[width, height];
            InitialiseMap(newMap);
            SmoothMap(newMap);

            // Determine whether to keep one or two largest islands
            if ((topLeftFilled && bottomRightFilled && !topRightFilled && !bottomLeftFilled) ||
                (topRightFilled && bottomLeftFilled && !topLeftFilled && !bottomRightFilled))
            {
                KeepTwoLargestIslands(newMap);
            }
            else
            {
                KeepLargestIsland(newMap);
            }

            return newMap;
        }

        private void InitialiseMap(bool[,] map)
        {
            int midWidth = width / 2;
            int midHeight = height / 2;

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    if (x < midWidth && y >= midHeight) // Top Left
                    {
                        map[x, y] = topLeftFilled;
                    }
                    else if (x >= midWidth && y >= midHeight) // Top Right
                    {
                        map[x, y] = topRightFilled;
                    }
                    else if (x < midWidth && y < midHeight) // Bottom Left
                    {
                        map[x, y] = bottomLeftFilled;
                    }
                    else if (x >= midWidth && y < midHeight) // Bottom Right
                    {
                        map[x, y] = bottomRightFilled;
                    }
                }
            }

            ApplyNoiseToEdges(map);
        }

        private void ApplyNoiseToEdges(bool[,] map)
        {
            bool[,] tempMap = (bool[,])map.Clone();

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    if (IsLandWaterEdge(map, x, y))
                    {
                        for (int dx = -edgeOffset; dx <= edgeOffset; dx++)
                        {
                            for (int dy = -edgeOffset; dy <= edgeOffset; dy++)
                            {
                                int nx = x + dx;
                                int ny = y + dy;

                                if (nx >= 0 && nx < width && ny >= 0 && ny < height)
                                {
                                    float noiseValue = Mathf.PerlinNoise((nx / noiseScale) + noiseOffsetX, (ny / noiseScale) + noiseOffsetY);
                                    tempMap[nx, ny] = noiseValue > 0.5f;
                                }
                            }
                        }
                    }
                }
            }

            Array.Copy(tempMap, map, map.Length);
        }

        private bool IsLandWaterEdge(bool[,] map, int x, int y)
        {
            if (x <= 0 || y <= 0 || x >= width - 1 || y >= height - 1)
                return false;

            bool isLand = map[x, y];
            bool hasWaterNeighbor = !map[x - 1, y] || !map[x + 1, y] || !map[x, y - 1] || !map[x, y + 1];

            return isLand && hasWaterNeighbor;
        }

        private void SmoothMap(bool[,] map)
        {
            for (int i = 0; i < smoothIterations; i++)
            {
                bool[,] tempMap = (bool[,])map.Clone();

                for (int x = 1; x < width - 1; x++)
                {
                    for (int y = 1; y < height - 1; y++)
                    {
                        int neighbors = CountFilledNeighbors(tempMap, x, y);
                        if (neighbors > 4)
                        {
                            map[x, y] = true;
                        }
                        else if (neighbors < 4)
                        {
                            map[x, y] = false;
                        }
                    }
                }
            }
        }

        private void KeepLargestIsland(bool[,] map)
        {
            bool[,] visited = new bool[width, height];
            List<Vector2Int> largestIsland = new List<Vector2Int>();

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    if (!visited[x, y] && map[x, y])
                    {
                        List<Vector2Int> region = GetRegionTiles(map, visited, x, y);

                        if (region.Count > largestIsland.Count)
                        {
                            largestIsland = region;
                        }
                    }
                }
            }

            // Clear the map and keep only the largest island
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    map[x, y] = false;
                }
            }

            foreach (Vector2Int tile in largestIsland)
            {
                map[tile.x, tile.y] = true;
            }
        }

        private void KeepTwoLargestIslands(bool[,] map)
        {
            bool[,] visited = new bool[width, height];
            List<Vector2Int> largestIsland = new List<Vector2Int>();
            List<Vector2Int> secondLargestIsland = new List<Vector2Int>();

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    if (!visited[x, y] && map[x, y])
                    {
                        List<Vector2Int> region = GetRegionTiles(map, visited, x, y);

                        if (region.Count > largestIsland.Count)
                        {
                            secondLargestIsland = largestIsland;
                            largestIsland = region;
                        }
                        else if (region.Count > secondLargestIsland.Count)
                        {
                            secondLargestIsland = region;
                        }
                    }
                }
            }

            // Clear the map and keep only the two largest islands
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    map[x, y] = false;
                }
            }

            foreach (Vector2Int tile in largestIsland)
            {
                map[tile.x, tile.y] = true;
            }

            foreach (Vector2Int tile in secondLargestIsland)
            {
                map[tile.x, tile.y] = true;
            }
        }

        private List<Vector2Int> GetRegionTiles(bool[,] map, bool[,] visited, int startX, int startY)
        {
            List<Vector2Int> tiles = new List<Vector2Int>();
            Queue<Vector2Int> queue = new Queue<Vector2Int>();
            queue.Enqueue(new Vector2Int(startX, startY));
            visited[startX, startY] = true;

            while (queue.Count > 0)
            {
                Vector2Int tile = queue.Dequeue();
                tiles.Add(tile);

                for (int dx = -1; dx <= 1; dx++)
                {
                    for (int dy = -1; dy <= 1; dy++)
                    {
                        if (Math.Abs(dx) == Math.Abs(dy))
                            continue;

                        int nx = tile.x + dx;
                        int ny = tile.y + dy;

                        if (nx >= 0 && nx < width && ny >= 0 && ny < height)
                        {
                            if (!visited[nx, ny] && map[nx, ny])
                            {
                                visited[nx, ny] = true;
                                queue.Enqueue(new Vector2Int(nx, ny));
                            }
                        }
                    }
                }
            }

            return tiles;
        }

        private int CountFilledNeighbors(bool[,] map, int x, int y)
        {
            int count = 0;

            for (int dx = -1; dx <= 1; dx++)
            {
                for (int dy = -1; dy <= 1; dy++)
                {
                    if (dx == 0 && dy == 0)
                        continue;

                    int nx = x + dx;
                    int ny = y + dy;

                    if (nx >= 0 && nx < width && ny >= 0 && ny < height)
                    {
                        if (map[nx, ny])
                            count++;
                    }
                }
            }

            return count;
        }
    }
}
