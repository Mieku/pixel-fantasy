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
    [ActionName(Name = "Mountain Range Generator")]
    public class MountainRangeGenerator : TWCBlueprintAction, ITWCAction
    {
        public bool topLeftFilled = true;
        public bool topRightFilled = true;
        public bool bottomLeftFilled = true;
        public bool bottomRightFilled = true;
        public float initialFillPercentage = 0.45f; // Initial fill percentage for cellular automata
        public int smoothIterations = 5; // Number of smoothing iterations

        private bool[,] newMap;
        private int height;
        private int width;
        private TWCGUILayout guiLayout;
        private System.Random random;

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
                initialFillPercentage = EditorGUI.Slider(guiLayout.rect, "Initial Fill Percentage", initialFillPercentage, 0f, 1f);
                guiLayout.Add();
                smoothIterations = EditorGUI.IntField(guiLayout.rect, "Smooth Iterations", smoothIterations);
            }
        }
#endif

        public ITWCAction Clone()
        {
            var clone = new MountainRangeGenerator
            {
                topLeftFilled = this.topLeftFilled,
                topRightFilled = this.topRightFilled,
                bottomLeftFilled = this.bottomLeftFilled,
                bottomRightFilled = this.bottomRightFilled,
                initialFillPercentage = this.initialFillPercentage,
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

            random = new System.Random(_twc.currentSeed);

            newMap = Generate();
            return TileWorldCreatorUtilities.MergeMap(map, newMap);
        }

        public bool[,] Generate()
        {
            newMap = new bool[width, height];
            InitialiseMap(newMap);
            ApplyCellularAutomata(newMap);
            SmoothMap(newMap);
            KeepIslandsInFilledArea(newMap);

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
                    if (IsInFilledArea(x, y) && random.NextDouble() < initialFillPercentage)
                    {
                        map[x, y] = true;
                    }
                }
            }
        }

        private void ApplyCellularAutomata(bool[,] map)
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

        private void KeepIslandsInFilledArea(bool[,] map)
        {
            bool[,] visited = new bool[width, height];
            List<Vector2Int> islandsToKeep = new List<Vector2Int>();

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    if (!visited[x, y] && map[x, y])
                    {
                        List<Vector2Int> region = GetRegionTiles(map, visited, x, y);

                        if (IsRegionInFilledArea(region))
                        {
                            islandsToKeep.AddRange(region);
                        }
                    }
                }
            }

            // Clear the map and keep only the islands in the filled area
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    map[x, y] = false;
                }
            }

            foreach (Vector2Int tile in islandsToKeep)
            {
                map[tile.x, tile.y] = true;
            }
        }

        private bool IsRegionInFilledArea(List<Vector2Int> region)
        {
            foreach (var tile in region)
            {
                if (IsInFilledArea(tile.x, tile.y))
                {
                    return true;
                }
            }
            return false;
        }

        private bool IsInFilledArea(int x, int y)
        {
            int midWidth = width / 2;
            int midHeight = height / 2;

            if (x < midWidth && y >= midHeight && topLeftFilled)
            {
                return true;
            }
            if (x >= midWidth && y >= midHeight && topRightFilled)
            {
                return true;
            }
            if (x < midWidth && y < midHeight && bottomLeftFilled)
            {
                return true;
            }
            if (x >= midWidth && y < midHeight && bottomRightFilled)
            {
                return true;
            }

            return false;
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
