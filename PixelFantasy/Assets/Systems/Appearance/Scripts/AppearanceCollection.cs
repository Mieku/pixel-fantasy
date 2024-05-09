using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Assets.PixelFantasy.PixelHeroes.Common.Scripts.Utils;
using Sirenix.OdinInspector;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Systems.Appearance.Scripts
{
    [CreateAssetMenu(fileName = "Appearance Collection", menuName = "Settings/Kinlings/Appearance Collection")]
    public class AppearanceCollection : ScriptableObject
    {
        public Texture2D PaletteTexture;

        public List<AvatarLayer> SideLayers;
        public List<AvatarLayer> UpLayers;
        public List<AvatarLayer> DownLayers;
        
        public List<AvatarLayer> GetLayersByDirection(AvatarLayer.EAppearanceDirection direction)
        {
            switch (direction)
            {
                case AvatarLayer.EAppearanceDirection.Left:
                case AvatarLayer.EAppearanceDirection.Right:
                    return SideLayers;
                case AvatarLayer.EAppearanceDirection.Up:
                    return UpLayers;
                case AvatarLayer.EAppearanceDirection.Down:
                    return DownLayers;
                default:
                    throw new ArgumentOutOfRangeException(nameof(direction), direction, null);
            }
        }

        public static readonly List<Color32> Palette = new List<Color32>
        {
            new Color32(255, 0, 64, 255), 
            new Color32(19, 19, 19, 255), 
            new Color32(27, 27, 27, 255), 
            new Color32(39, 39, 39, 255), 
            new Color32(61, 61, 61, 255), 
            new Color32(93, 93, 93, 255), 
            new Color32(133, 133, 133, 255), 
            new Color32(180, 180, 180, 255), 
            new Color32(255, 255, 255, 255), 
            new Color32(199, 207, 221, 255), 
            new Color32(146, 161, 185, 255), 
            new Color32(101, 115, 146, 255), 
            new Color32(66, 76, 110, 255), 
            new Color32(42, 47, 78, 255), 
            new Color32(26, 25, 50, 255), 
            new Color32(14, 7, 27, 255), 
            new Color32(28, 18, 28, 255), 
            new Color32(57, 31, 33, 255), 
            new Color32(93, 44, 40, 255), 
            new Color32(138, 72, 54, 255), 
            new Color32(191, 111, 74, 255), 
            new Color32(230, 156, 105, 255), 
            new Color32(246, 202, 159, 255), 
            new Color32(249, 230, 207, 255), 
            new Color32(237, 171, 80, 255), 
            new Color32(224, 116, 56, 255), 
            new Color32(198, 69, 36, 255), 
            new Color32(142, 37, 29, 255), 
            new Color32(255, 80, 0, 255), 
            new Color32(237, 118, 20, 255), 
            new Color32(255, 162, 20, 255), 
            new Color32(255, 200, 37, 255), 
            new Color32(255, 235, 87, 255), 
            new Color32(211, 252, 126, 255), 
            new Color32(153, 230, 95, 255), 
            new Color32(90, 197, 79, 255), 
            new Color32(51, 152, 75, 255), 
            new Color32(30, 111, 80, 255), 
            new Color32(19, 76, 76, 255), 
            new Color32(12, 46, 68, 255), 
            new Color32(0, 57, 109, 255), 
            new Color32(0, 105, 170, 255), 
            new Color32(0, 152, 220, 255), 
            new Color32(0, 205, 249, 255), 
            new Color32(12, 241, 255, 255), 
            new Color32(148, 253, 255, 255), 
            new Color32(253, 210, 237, 255), 
            new Color32(243, 137, 245, 255), 
            new Color32(219, 63, 253, 255), 
            new Color32(122, 9, 250, 255), 
            new Color32(48, 3, 217, 255), 
            new Color32(12, 2, 147, 255), 
            new Color32(3, 25, 63, 255), 
            new Color32(59, 20, 67, 255), 
            new Color32(98, 36, 97, 255), 
            new Color32(147, 56, 143, 255), 
            new Color32(202, 82, 201, 255), 
            new Color32(200, 80, 134, 255), 
            new Color32(246, 129, 135, 255), 
            new Color32(245, 85, 93, 255), 
            new Color32(234, 50, 60, 255), 
            new Color32(196, 36, 48, 255), 
            new Color32(137, 30, 43, 255), 
            new Color32(87, 28, 39, 255)
        };

        [Button("Refresh Layers")]
        public void Refresh()
        {
            var palette = PaletteTexture.GetPixels32().ToList();
            
            foreach (var sideLayer in SideLayers)
            {
                sideLayer.Refresh(palette, AvatarLayer.EAppearanceDirection.Right);
            }
            
            foreach (var upLayer in UpLayers)
            {
                upLayer.Refresh(palette, AvatarLayer.EAppearanceDirection.Up);
            }
            
            foreach (var downLayer in DownLayers)
            {
                downLayer.Refresh(palette, AvatarLayer.EAppearanceDirection.Down);
            }
            
            Debug.Log("Refresh done!");
        }
    }

    [Serializable]
    public class AvatarLayer
    {
        public string ID;
        public Object SpriteFolder;
        public List<Texture2D> Textures;

        private Color32[] _pixels;
        //private Color32 _outlineColour = new Color32(19, 19, 19, 255);

        public void Refresh(List<Color32> palette, EAppearanceDirection direction)
        {
            var root = UnityEditor.AssetDatabase.GetAssetPath(SpriteFolder);

            switch (direction)
            {
                case EAppearanceDirection.Left:
                case EAppearanceDirection.Right:
                    var sideFiles = Directory.GetFiles(root + "/Side", "*.png", SearchOption.AllDirectories).ToList();
                    UpdateTextures(sideFiles, palette);
                    break;
                case EAppearanceDirection.Up:
                    var upFiles = Directory.GetFiles(root + "/Up", "*.png", SearchOption.AllDirectories).ToList();
                    UpdateTextures(upFiles, palette);
                    break;
                case EAppearanceDirection.Down:
                    var downFiles = Directory.GetFiles(root + "/Down", "*.png", SearchOption.AllDirectories).ToList();
                    UpdateTextures(downFiles, palette);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(direction), direction, null);
            }
        }

        private void UpdateTextures(List<string> files, List<Color32> palette)
        {
            Textures.Clear();

            foreach (var path in files)
            {
                var texture = UnityEditor.AssetDatabase.LoadAssetAtPath<Texture2D>(path);
                Textures.Add(texture);

                var colors = texture.GetPixels32().Distinct().Where(c => c.a > 0).ToList();
                var wrongColors = colors.Where(c => !palette.Contains(c)).ToList();

                if (wrongColors.Any())
                {
                    Debug.LogError($"Colors outside of the palette found in {path}: {string.Join(",", wrongColors)}.");
                }
            }
        }
        
        public Color32[] GetPixels(string data, Color32[] mask, string changed)
        {
            var pattern = @"{ID:(?<ID>[^}]+)}" +                    // Matches ID
                          @"(?:{Colour:(?<Colour>#[\da-fA-F]{6})})?" + // Matches Colour, optional
                          @"(?:{Exempt:(?<Exempt>(?:#[\da-fA-F]{6})(?:,#[\da-fA-F]{6})*)})?" + // Matches Exempt, optional
                          @"(?:{HSV:(?<H>[-\d]+):(?<S>[-\d]+):(?<V>[-\d]+)})?"; // Matches HSV, optional

            var match = Regex.Match(data, pattern);

            var id = match.Groups["ID"].Value;
            var colourString = match.Groups["Colour"].Success ? match.Groups["Colour"].Value : "#FFFFFF"; // Default to white if no colour is specified
            var exemptStringArray = match.Groups["Exempt"].Success ? match.Groups["Exempt"].Value.Split(',') : new string[0]; // Splits the exempt colors into an array
            var h = match.Groups["H"].Success ? float.Parse(match.Groups["H"].Value, CultureInfo.InvariantCulture) : 0f;
            var s = match.Groups["S"].Success ? float.Parse(match.Groups["S"].Value, CultureInfo.InvariantCulture) : 0f;
            var v = match.Groups["V"].Success ? float.Parse(match.Groups["V"].Value, CultureInfo.InvariantCulture) : 0f;
            var index = Textures.FindIndex(i => i.name == id);
            if (index == -1) return null;

            if (string.IsNullOrEmpty(id))
            {
                Debug.LogError("ID was empty");
                return null;
            }
            
            // Get the colour
            ColorUtility.TryParseHtmlString(colourString, out var colour);
            
            // Get the exempt colours
            List<Color32> exemptColours = new List<Color32>(){ new Color32(19, 19, 19, 255) };
            foreach (var exemptString in exemptStringArray)
            {
                if (ColorUtility.TryParseHtmlString(exemptString, out var exemptColour))
                {
                    exemptColours.Add(exemptColour);
                }
            }
            
            var update = changed == null || changed == ID;

            switch (changed)
            {
                case "Hat" when ID == "Hair":
                    update = true;
                    break;
            }

            return GetPixels(index, colour, exemptColours, h, s, v, mask, update);
        }
        
        public Color32[] GetPixels(int index, Color colour, List<Color32> exemptColours, float h, float s, float v, Color32[] mask, bool update)
        {
            if (!update && _pixels?.Length > 0 && mask == null) return _pixels;
            
            _pixels = Textures[index].GetPixels32();

            if (mask != null)
            {
                for (var i = 0; i < _pixels.Length; i++)
                {
                    if (mask[i].a <= 0)
                    {
                        _pixels[i] = new Color32();
                    }
                    else if (mask[i] == Color.black)
                    {
                        _pixels[i] = mask[i];
                    }
                }
            }

            if (colour != Color.white)
            {
                if( ID is "Body" or "Hands")
                {
                  _pixels = Repaint4C(_pixels, colour, AppearanceCollection.Palette, exemptColours, 2);
                }
                else if (ID is "Hair" or "Beard" or "Clothing" )
                {
                    _pixels = Repaint4C(_pixels, colour, AppearanceCollection.Palette, exemptColours, 4);
                }
                else if (ID is "Eyes")
                {
                    _pixels = Repaint4C(_pixels, colour, AppearanceCollection.Palette, exemptColours, 1);
                }
                else
                {
                    for (var i = 0; i < _pixels.Length; i++)
                    {
                        if (_pixels[i].a > 0) _pixels[i] *= colour;
                    }
                }
            }

            if (Mathf.Approximately(h, 0) && Mathf.Approximately(s, 0) && Mathf.Approximately(v, 0)) return _pixels;

            for (var i = 0; i < _pixels.Length; i++)
            {
                if (_pixels[i].a > 0 && !exemptColours.Contains(_pixels[i]))
                {
                    _pixels[i] = TextureHelper.AdjustColor(_pixels[i], h, s, v);
                }
            }

            _pixels = TextureHelper.ApplyPalette(_pixels, AppearanceCollection.Palette);

            return _pixels;
        }
 
        public Color32[] Repaint4C(Color32[] pixels, Color32 baseColor, List<Color32> palette, List<Color32> exemptColours, int colorCount = 4)
        {
            int baseIndex = FindClosestColorIndex(baseColor, palette);
            var colorGradient = GenerateIndexedColorGradient(baseIndex, palette, colorCount);

            float maxBrightness = pixels.Max(p => GetBrightness(p));
            float minBrightness = pixels.Min(p => GetBrightness(p));
            float brightnessRange = maxBrightness - minBrightness;

            Color32[] resultPixels = new Color32[pixels.Length];

            for (int i = 0; i < pixels.Length; i++)
            {
                if (pixels[i].a > 0 && !exemptColours.Contains(pixels[i]))
                {
                    float normalizedBrightness = (GetBrightness(pixels[i]) - minBrightness) / brightnessRange;
                    int gradientIndex = (int)(normalizedBrightness * (colorGradient.Count - 1));
                    resultPixels[i] = colorGradient[gradientIndex];
                }
                else
                {
                    resultPixels[i] = pixels[i]; // Preserve transparency and outline color
                }
            }

            return resultPixels;
        }


        private List<Color32> GenerateIndexedColorGradient(int baseIndex, List<Color32> palette, int colorCount = 4)
        {
            List<Color32> gradient = new List<Color32>();
            switch (colorCount)
            {
                case 1:
                    gradient.Add(palette[baseIndex]); 
                    break;
                case 2:
                    // Only use base and one darker shade if total colors needed is 2
                    gradient.Add(palette[Mathf.Clamp(baseIndex - 1, 0, palette.Count - 1)]); // Darker
                    gradient.Add(palette[baseIndex]); // Base
                    break;
                case 4:
                default:
                    // Use one lighter, base, and two darker shades if total colors needed is 4 or unspecified
                    gradient.Add(palette[Mathf.Clamp(baseIndex - 2, 0, palette.Count - 1)]); // Darkest
                    gradient.Add(palette[Mathf.Clamp(baseIndex - 1, 0, palette.Count - 1)]); // Darker
                    gradient.Add(palette[baseIndex]); // Base
                    gradient.Add(palette[Mathf.Clamp(baseIndex + 1, 0, palette.Count - 1)]); // Lighter
                    return SortColorsByBrightness(gradient);
            }
            return gradient;
        }
        
        public static List<Color32> SortColorsByBrightness(List<Color32> colors)
        {
            var sortedColors = colors.OrderBy(color => GetLuminance(color)).ToList();
            return sortedColors;
        }

        // Helper method to calculate the luminance of a Color32
        private static float GetLuminance(Color32 color)
        {
            // Convert Color32 to Color to work with floating point precision
            Color c = color;
            // Standard formula for luminance
            return 0.299f * c.r + 0.587f * c.g + 0.114f * c.b;
        }
        
        private float GetBrightness(Color32 color)
        {
            return 0.299f * color.r + 0.587f * color.g + 0.114f * color.b;  // Standard luminosity formula
        }

        private int FindClosestColorIndex(Color32 color, List<Color32> palette)
        {
            float minDistance = float.MaxValue;
            int closestIndex = 0;
            for (int i = 0; i < palette.Count; i++)
            {
                float distance = ColorDistance(color, palette[i]);
                if (distance < minDistance)
                {
                    minDistance = distance;
                    closestIndex = i;
                }
            }
            return closestIndex;
        }

        private float ColorDistance(Color32 c1, Color32 c2)
        {
            return Mathf.Sqrt(Mathf.Pow(c1.r - c2.r, 2) + Mathf.Pow(c1.g - c2.g, 2) + Mathf.Pow(c1.b - c2.b, 2));
        }
        
        public enum EAppearanceDirection
        {
            Right,
            Left,
            Up,
            Down
        }
    }
}
