using System;
using System.Collections;
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

        private void Start()
        {
            
        }

        public void GenerateArea(OverworldPreview.OverworldSelectionData overworldSelectionData, System.Action<Texture2D> callback)
        {
            var seed = GeneratePositionSeed(overworldSelectionData.Position);
            _tileWorldCreator.SetCustomRandomSeed(seed);

            // Access the water blueprint layer and its generator
            var waterBlueprintLayer = GetBlueprintLayer("Water");
            if (waterBlueprintLayer != null)
            {
                var coastGenerator = GetGeneratorFromBlueprintLayer<CoastGenerator>(waterBlueprintLayer);
                if (coastGenerator != null)
                {
                    // Modify the generator's properties based on the overworld selection data
                    AdjustCoastGenerator(coastGenerator, overworldSelectionData);
                }
            }

            // Start the coroutine to generate the world map
            StartCoroutine(GenerateWorldMapCoroutine(callback));
        }
        
        private IEnumerator GenerateWorldMapCoroutine(System.Action<Texture2D> callback)
        {
            // Execute the blueprint layers
            _tileWorldCreator.ExecuteAllBlueprintLayers();

            // Wait until execution is complete (add a small delay to ensure it completes)
            yield return new WaitForEndOfFrame();
    
            // Build the texture from the generated map data
            Texture2D resultTexture = BuildTextureFromMap();

            // Invoke the callback with the result texture
            callback?.Invoke(resultTexture);
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

        private Texture2D BuildTextureFromMap()
        {
            int width = _tileWorldCreator.twcAsset.mapWidth;
            int height = _tileWorldCreator.twcAsset.mapHeight;
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
            foreach (var layer in _tileWorldCreator.twcAsset.mapBlueprintLayers)
            {
                if (layer.active && !layer.mapResultFailed)
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

        private void AdjustCoastGenerator(CoastGenerator coastGenerator, OverworldPreview.OverworldSelectionData overworldSelectionData)
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
}
