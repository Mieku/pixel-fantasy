using System;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

using TWC.editor;
using TWC.Utilities;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace TWC.Actions
{
    [ActionCategory(Category = ActionCategoryAttribute.CategoryTypes.Generators)]
    [ActionName(Name = "Perlin Noise Blob Generator")]
    public class PerlinNoiseBlobGenerator : TWCBlueprintAction, ITWCAction
    {
        public int numberOfIslands = 5;
        public float islandSize = 0.5f; // 0.1 to 1.0 range where 1.0 is the maximum size
        public float noiseScale = 5.0f;
        public int seedSalt = 0;

        private bool[,] newMap;
        private int height;
        private int width;
        private TWCGUILayout guiLayout;

        public ITWCAction Clone()
        {
            var _r = new PerlinNoiseBlobGenerator();
            _r.numberOfIslands = this.numberOfIslands;
            _r.islandSize = this.islandSize;
            _r.noiseScale = this.noiseScale;
            _r.seedSalt = this.seedSalt;

            return _r;
        }

#if UNITY_EDITOR
        public override void DrawGUI(Rect _rect, int _layerIndex, TileWorldCreatorAsset _asset, TileWorldCreator _twc)
        {
            using (guiLayout = new TWCGUILayout(_rect))
            {
                guiLayout.Add();
                numberOfIslands = EditorGUI.IntField(guiLayout.rect, "Number of Islands", numberOfIslands);
                guiLayout.Add();
                islandSize = EditorGUI.Slider(guiLayout.rect, "Island Size", islandSize, 0.1f, 1.0f);
                guiLayout.Add();
                noiseScale = EditorGUI.FloatField(guiLayout.rect, "Noise Scale", noiseScale);
                guiLayout.Add();
                seedSalt = EditorGUI.IntField(guiLayout.rect, "Seed Salt", seedSalt);
            }
        }
#endif

        public float GetGUIHeight()
        {
            if (guiLayout != null)
            {
                return guiLayout.height;
            }
            else
            {
                return 72;
            }
        }

        public bool[,] Execute(bool[,] map, TileWorldCreator _twc)
        {
            height = map.GetLength(1);
            width = map.GetLength(0);

            int combinedSeed = CombineSeedAndSalt(_twc.currentSeed, seedSalt);
            UnityEngine.Random.InitState(combinedSeed);

            var _boolMap = Generate(combinedSeed);
            _boolMap = EnsureNumberOfIslands(_boolMap, numberOfIslands);
            return TileWorldCreatorUtilities.MergeMap(map, _boolMap);
        }

        public bool[,] Generate(int seed)
        {
            newMap = new bool[width, height];
            InitialiseMap(newMap, seed);

            return newMap;
        }

        void InitialiseMap(bool[,] map, int seed)
        {
            float threshold = Mathf.Lerp(0.7f, 0.3f, islandSize); // Reverse the threshold calculation

            float xOffset = seed % 10000; // Ensure xOffset and yOffset are in a reasonable range
            float yOffset = (seed / 10000) % 10000;

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    float xCoord = (x + xOffset) / (float)width * noiseScale;
                    float yCoord = (y + yOffset) / (float)height * noiseScale;
                    float sample = Mathf.PerlinNoise(xCoord, yCoord);

                    map[x, y] = sample > threshold;
                }
            }
        }

        bool[,] EnsureNumberOfIslands(bool[,] map, int targetNumberOfIslands)
        {
            var islandSizes = new Dictionary<int, int>();
            var islandCells = new Dictionary<int, List<Vector2Int>>();
            var visited = new bool[width, height];
            int islandId = 1;

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    if (map[x, y] && !visited[x, y])
                    {
                        List<Vector2Int> cells = new List<Vector2Int>();
                        int size = FloodFill(map, visited, x, y, islandId, cells);
                        islandSizes.Add(islandId, size);
                        islandCells.Add(islandId, cells);
                        islandId++;
                    }
                }
            }

            if (islandSizes.Count > targetNumberOfIslands)
            {
                var sortedIslands = new List<int>(islandSizes.Keys);
                sortedIslands.Sort((a, b) => islandSizes[a].CompareTo(islandSizes[b]));

                for (int i = 0; i < sortedIslands.Count - targetNumberOfIslands; i++)
                {
                    RemoveIsland(map, islandCells[sortedIslands[i]]);
                }
            }

            return map;
        }

        int FloodFill(bool[,] map, bool[,] visited, int x, int y, int islandId, List<Vector2Int> cells)
        {
            var stack = new Stack<Vector2Int>();
            stack.Push(new Vector2Int(x, y));
            int size = 0;

            while (stack.Count > 0)
            {
                var p = stack.Pop();
                if (p.x >= 0 && p.x < width && p.y >= 0 && p.y < height && map[p.x, p.y] && !visited[p.x, p.y])
                {
                    visited[p.x, p.y] = true;
                    cells.Add(p);
                    size++;

                    stack.Push(new Vector2Int(p.x + 1, p.y));
                    stack.Push(new Vector2Int(p.x - 1, p.y));
                    stack.Push(new Vector2Int(p.x, p.y + 1));
                    stack.Push(new Vector2Int(p.x, p.y - 1));
                }
            }

            return size;
        }

        void RemoveIsland(bool[,] map, List<Vector2Int> cells)
        {
            foreach (var cell in cells)
            {
                map[cell.x, cell.y] = false;
            }
        }

        int CombineSeedAndSalt(int seed, int salt)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] inputBytes = Encoding.ASCII.GetBytes(seed.ToString() + salt.ToString());
                byte[] hashBytes = sha256.ComputeHash(inputBytes);
                int combinedSeed = BitConverter.ToInt32(hashBytes, 0);
                return combinedSeed;
            }
        }
    }
}
