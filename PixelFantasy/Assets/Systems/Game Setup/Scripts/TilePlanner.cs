using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Systems.World_Building.Scripts.Generators;
using TWC;
using TWC.Actions;
using UnityEngine;

namespace Systems.Game_Setup.Scripts
{
    public class TilePlanner : MonoBehaviour
    {
        [SerializeField] private TileWorldCreator _tileWorldCreator;
        [SerializeField] private int _numCellsToTrim;

        private List<string> _blueprintLayersToShow = new List<string>() { "Water", "Grass", "Mountains", "Forest", "Elevation" };

        private void Start()
        {
            
        }

        public void GenerateArea(OverworldPreview.OverworldSelectionData overworldSelectionData, System.Action<Texture2D, List<TileWorldCreatorAsset.BlueprintLayerData>, LocationStats> callback)
        {
            var seed = GeneratePositionSeed(overworldSelectionData.Position);
            _tileWorldCreator.SetCustomRandomSeed(seed);

            AdjustCoastline(overworldSelectionData);
            AdjustMountains(overworldSelectionData);
            
            
            // Start the coroutine to generate the world map
            StartCoroutine(GenerateWorldMapCoroutine(callback));
        }

        private void AdjustCoastline(OverworldPreview.OverworldSelectionData overworldSelectionData)
        {
            // Access the water blueprint layer and its generator
            var waterBlueprintLayer = GetBlueprintLayer("Water");
            if (waterBlueprintLayer != null)
            {
                var coastGenerator = GetGeneratorFromBlueprintLayer<CoastGenerator>(waterBlueprintLayer);
                if (coastGenerator != null)
                {
                    if (overworldSelectionData.TopLeftFeature == OverworldPreview.EFeatureTileType.Ocean)
                    {
                        coastGenerator.topLeftFilled = true;
                    }
                    else
                    {
                        coastGenerator.topLeftFilled = false;
                    }
            
                    if (overworldSelectionData.TopRightFeature == OverworldPreview.EFeatureTileType.Ocean)
                    {
                        coastGenerator.topRightFilled = true;
                    }
                    else
                    {
                        coastGenerator.topRightFilled = false;
                    }
            
                    if (overworldSelectionData.BottomLeftFeature == OverworldPreview.EFeatureTileType.Ocean)
                    {
                        coastGenerator.bottomLeftFilled = true;
                    }
                    else
                    {
                        coastGenerator.bottomLeftFilled = false;
                    }
            
                    if (overworldSelectionData.BottomRightFeature == OverworldPreview.EFeatureTileType.Ocean)
                    {
                        coastGenerator.bottomRightFilled = true;
                    }
                    else
                    {
                        coastGenerator.bottomRightFilled = false;
                    }
                }
            }
        }
        
        private void AdjustMountains(OverworldPreview.OverworldSelectionData overworldSelectionData)
        {
            // Access the water blueprint layer and its generator
            var mountainsBlueprintLayer = GetBlueprintLayer("Mountains");
            if (mountainsBlueprintLayer != null)
            {
                var mountainsGenerator = GetGeneratorFromBlueprintLayer<MountainRangeGenerator>(mountainsBlueprintLayer);
                if (mountainsGenerator != null)
                {
                    if (overworldSelectionData.TopLeftFeature == OverworldPreview.EFeatureTileType.Mountain)
                    {
                        mountainsGenerator.topLeftFilled = true;
                    }
                    else
                    {
                        mountainsGenerator.topLeftFilled = false;
                    }
            
                    if (overworldSelectionData.TopRightFeature == OverworldPreview.EFeatureTileType.Mountain)
                    {
                        mountainsGenerator.topRightFilled = true;
                    }
                    else
                    {
                        mountainsGenerator.topRightFilled = false;
                    }
            
                    if (overworldSelectionData.BottomLeftFeature == OverworldPreview.EFeatureTileType.Mountain)
                    {
                        mountainsGenerator.bottomLeftFilled = true;
                    }
                    else
                    {
                        mountainsGenerator.bottomLeftFilled = false;
                    }
            
                    if (overworldSelectionData.BottomRightFeature == OverworldPreview.EFeatureTileType.Mountain)
                    {
                        mountainsGenerator.bottomRightFilled = true;
                    }
                    else
                    {
                        mountainsGenerator.bottomRightFilled = false;
                    }
                }
            }
        }
        
        private IEnumerator GenerateWorldMapCoroutine(System.Action<Texture2D, List<TileWorldCreatorAsset.BlueprintLayerData>, LocationStats> callback)
        {
            // Execute the blueprint layers
            _tileWorldCreator.ExecuteAllBlueprintLayers();

            // Wait until execution is complete (add a small delay to ensure it completes)
            yield return new WaitForEndOfFrame();

            var trimmedBlueprints = TrimBlueprints(_tileWorldCreator.twcAsset.mapBlueprintLayers);
    
            // Build the texture from the generated map data
            Texture2D resultTexture = BuildTextureFromMap(trimmedBlueprints, _tileWorldCreator.twcAsset.mapWidth - (_numCellsToTrim * 2), _tileWorldCreator.twcAsset.mapHeight - (_numCellsToTrim * 2));

            var stats = GetLocationStats(_tileWorldCreator.twcAsset.mapBlueprintLayers);
            
            // Invoke the callback with the result texture
            callback?.Invoke(resultTexture, _tileWorldCreator.twcAsset.mapBlueprintLayers, stats);
        }

        private LocationStats GetLocationStats(List<TileWorldCreatorAsset.BlueprintLayerData> blueprints)
        {
            var mountains = GetBlueprintLayer(blueprints, "Mountains");
            float mountainsFilled = 0;
            if (mountains != null)
            {
                mountainsFilled = GetPercentageFilled(mountains);
            }
            
            var water = GetBlueprintLayer(blueprints, "Water");
            float waterFilled = 0;
            if (water != null)
            {
                waterFilled = GetPercentageFilled(water);
            }
            
            var forest = GetBlueprintLayer(blueprints, "Forest");
            float forestFilled = 0;
            if (forest != null)
            {
                forestFilled = GetPercentageFilled(forest);
            }
            
            var vegitation = GetBlueprintLayer(blueprints, "Vegitation");
            float vegitationFilled = 0;
            if (vegitation != null)
            {
                vegitationFilled = GetPercentageFilled(vegitation);
            }

            LocationStats stats = new LocationStats
            {
                Mountains = mountainsFilled,
                Vegitation = vegitationFilled,
                Water = waterFilled,
                Forest = forestFilled
            };

            return stats; 
        }

        private float GetPercentageFilled(TileWorldCreatorAsset.BlueprintLayerData blueprint)
        {
            bool[,] map = blueprint.map;
            int totalTiles = map.GetLength(0) * map.GetLength(1);
            int filledTiles = 0;

            foreach (bool tile in map)
            {
                if (tile)
                {
                    filledTiles++;
                }
            }

            return (float)filledTiles / totalTiles * 100f;
        }

        private List<TileWorldCreatorAsset.BlueprintLayerData> TrimBlueprints(List<TileWorldCreatorAsset.BlueprintLayerData> originalBlueprints)
        {
            int numCellsToTrim = _numCellsToTrim;
            int newWidth = _tileWorldCreator.twcAsset.mapWidth - numCellsToTrim * 2;
            int newHeight = _tileWorldCreator.twcAsset.mapHeight - numCellsToTrim * 2;
    
            var trimmedBlueprints = new List<TileWorldCreatorAsset.BlueprintLayerData>();
    
            foreach (var originalBlueprint in originalBlueprints)
            {
                if (originalBlueprint.active)
                {
                    var trimmedLayer = new TileWorldCreatorAsset.BlueprintLayerData(originalBlueprint.layerName, true)
                    {
                        layerName = originalBlueprint.layerName,
                        active = originalBlueprint.active,
                        previewColor = originalBlueprint.previewColor,
                        stack = originalBlueprint.stack,
                        map = new bool[newWidth, newHeight]
                    };
            
                    for (int x = 0; x < newWidth; x++)
                    {
                        for (int y = 0; y < newHeight; y++)
                        {
                            trimmedLayer.map[x, y] = originalBlueprint.map[x + numCellsToTrim, y + numCellsToTrim];
                        }
                    }
            
                    trimmedBlueprints.Add(trimmedLayer);
                }
                else
                {
                    // If the blueprint layer is not active, just add it without changes
                    trimmedBlueprints.Add(originalBlueprint);
                }
            }
    
            return trimmedBlueprints;
        }


        private int GeneratePositionSeed(Vector2 position)
        {
            // Convert position components to strings
            string xPos = ((int)(position.x * 1000)).ToString();
            string yPos = ((int)(position.y * 1000)).ToString();

            // Concatenate the strings
            string seedString = xPos + yPos;

            // Hash the seed string to generate an integer seed
            int seed = seedString.GetHashCode();

            return seed;
        }

        private Texture2D BuildTextureFromMap(List<TileWorldCreatorAsset.BlueprintLayerData> blueprints, int width, int height)
        {
            Texture2D texture = new Texture2D(width, height, TextureFormat.RGBA32, false);
            texture.filterMode = FilterMode.Point;
            texture.wrapMode = TextureWrapMode.Clamp;

            // Initialize the texture with a default color (e.g., transparent or background color)
            Color defaultColor = new Color(0, 0, 0, 0); // Fully transparent
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    texture.SetPixel(x, y, defaultColor);
                }
            }

            // Process each layer and update the texture accordingly
            foreach (var layer in blueprints)
            {
                if (layer.active && _blueprintLayersToShow.Contains(layer.layerName))
                {
                    bool[,] map = layer.map;
                    Color layerColor = layer.previewColor;
                    layerColor.a = 1; // Ensure the layer color is fully opaque

                    for (int x = 0; x < width; x++)
                    {
                        for (int y = 0; y < height; y++)
                        {
                            if (map[x, y])
                            {
                                texture.SetPixel(x, y, layerColor);
                            }
                        }
                    }
                }
            }

            texture.Apply();
            return texture;
        }

        private TileWorldCreatorAsset.BlueprintLayerData GetBlueprintLayer(string layerName)
        {
            foreach (var layer in _tileWorldCreator.twcAsset.mapBlueprintLayers)
            {
                if (layer.layerName == layerName)
                {
                    return layer;
                }
            }
            return null;
        }
        
        private TileWorldCreatorAsset.BlueprintLayerData GetBlueprintLayer(List<TileWorldCreatorAsset.BlueprintLayerData> blueprints, string layerName)
        {
            foreach (var layer in blueprints)
            {
                if (layer.layerName == layerName)
                {
                    return layer;
                }
            }
            return null;
        }

        private T GetGeneratorFromBlueprintLayer<T>(TileWorldCreatorAsset.BlueprintLayerData layer) where T : TWCBlueprintAction
        {
            foreach (var action in layer.stack)
            {
                if (action.action is T generator)
                {
                    return generator;
                }
            }
            return null;
        }
    }

    public class LocationStats
    {
        public float Mountains;
        public float Water;
        public float Forest;
        public float Vegitation;

        public float TreesVeg => Vegitation + Forest;
        public float AvailableSpace => 100f - (Water + Mountains);
    }
}
