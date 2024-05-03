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
        public Color32 OutlineColour = Color.black;

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

            palette.Add(OutlineColour);

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
        public string Name;
        public Object SpriteFolder;
        public List<Texture2D> Textures;

        private Color32[] _pixels;

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

                var colors = new ColorDistinctor(texture.GetPixels32()).UniqueColors;

                if (colors.Any(i => i.a > 0 && i.a < 255))
                {
                    Debug.LogError($"Transparent pixels found in {path}");
                }

                var wrong = colors.Where(i => i.a == 255 && !palette.Any(j => i.FastEquals(j))).ToList();

                if (wrong.Any())
                {
                    Debug.LogError($"Colors outside of the palette found in {path}: {string.Join(",", wrong)}.");
                }
            }
        }
        
        public Color32[] GetPixels(string data, Color32[] mask, string changed)
        {
            var match = Regex.Match(data, @"(?<Name>[\w\- \[\]]+?)/(?<Paint>#\w+)/(?<H>[-\d]+):(?<S>[-\d]+):(?<V>[-\d]+)");
            var name = match.Groups["Name"].Value;
            var index = Textures.FindIndex(i => i.name == name);
            var paint = Color.white;

            if (index == -1) return null;
            
            if (match.Groups["Paint"].Success)
            {
                ColorUtility.TryParseHtmlString(match.Groups["Paint"].Value, out paint);
            }

            float h = 0f, s = 0f, v = 0f;

            if (match.Groups["H"].Success && match.Groups["S"].Success && match.Groups["V"].Success)
            {
                h = float.Parse(match.Groups["H"].Value, CultureInfo.InvariantCulture);
                s = float.Parse(match.Groups["S"].Value, CultureInfo.InvariantCulture);
                v = float.Parse(match.Groups["V"].Value, CultureInfo.InvariantCulture);
            }

            var update = changed == null || changed == Name;

            switch (changed)
            {
                case "Hat" when Name == "Hair":
                    update = true;
                    break;
            }

            return GetPixels(index, paint, h, s, v, mask, update);
        }
        
        public Color32[] GetPixels(int index, Color paint, float h, float s, float v, Color32[] mask, bool update)
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

            if (paint != Color.white)
            {
                if ( Name == "Body" || Name == "Hair")
                {
                    _pixels = Repaint3C(_pixels, paint, AppearanceCollection.Palette);
                }
                else
                {
                    for (var i = 0; i < _pixels.Length; i++)
                    {
                        if (_pixels[i].a > 0) _pixels[i] *= paint;
                    }
                }
            }

            if (Mathf.Approximately(h, 0) && Mathf.Approximately(s, 0) && Mathf.Approximately(v, 0)) return _pixels;

            for (var i = 0; i < _pixels.Length; i++)
            {
                if (_pixels[i].a > 0 && _pixels[i] != Color.black)
                {
                    _pixels[i] = TextureHelper.AdjustColor(_pixels[i], h, s, v);
                }
            }

            _pixels = TextureHelper.ApplyPalette(_pixels, AppearanceCollection.Palette);

            return _pixels;
        }
        
        public static Color32[] Repaint3C(Color32[] pixels, Color32 paint, List<Color32> palette)
        {
            var dict = new Dictionary<Color32, int>();

            for (var x = 0; x < 16; x++) // TODO: Hardcoded values.
            {
                for (var y = 0; y < 16; y++)
                {
                    var c = pixels[x + y * 256];
                    var black = (Color)palette[1];
                    if (c.a > 0 && c != Color.white && c != Color.black && c != black)
                    {
                        if (dict.ContainsKey(c))
                        {
                            dict[c]++;
                        }
                        else
                        {
                            dict.Add(c, 1);
                        }
                    }
                }
            }

            var colors = dict.Count > 3 ? dict.OrderByDescending(i => i.Value).Take(3).Select(i => i.Key).ToList() : dict.Keys.ToList();

            float GetBrightness(Color32 color)
            {
                Color.RGBToHSV(color, out _, out _, out var result);

                return result;
            }

            colors = colors.OrderBy(GetBrightness).ToList();

            if (colors.Count != 2 && colors.Count != 3)
            {
                throw new NotSupportedException("Sprite should have 2 or 3 colors only (+black outline).");
            }

            var index = palette.IndexOf(paint) - 1;
            
            var replacement = palette.GetRange(index, 3).OrderBy(i => ((Color) i).grayscale).ToList();
            var match = new Dictionary<Color32, Color32>
            {
                { colors[0], replacement[0] },
                { colors[1], replacement[1] }
            };

            if (colors.Count == 3)
            {
                match.Add(colors[2], replacement[2]);
            }

            for (var i = 0; i < pixels.Length; i++)
            {
                if (pixels[i].a > 0 && pixels[i] != Color.black && match.ContainsKey(pixels[i]))
                {
                    pixels[i] = match[pixels[i]];
                }
            }

            return pixels;
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
