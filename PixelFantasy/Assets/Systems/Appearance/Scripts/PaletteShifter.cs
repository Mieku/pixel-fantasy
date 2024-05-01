using System;
using System.Collections.Generic;
using System.Linq;
using Assets.PixelFantasy.PixelHeroes.Common.Scripts.Utils;
using Sirenix.OdinInspector;
using Sirenix.Utilities.Editor;
using UnityEngine;
using UnityEngine.UI;

namespace Systems.Appearance.Scripts
{
    public class PaletteShifter : MonoBehaviour
    {
        [SerializeField] private Image _image;
        [SerializeField] private SpriteRenderer _spriteRenderer;
        [SerializeField] private Color32 _colour;


        private Sprite _original;
        
        [field: SerializeField]
        public List<Color32> Palette = new List<Color32>
        {
            new Color32(255, 0, 64, 255), new Color32(19, 19, 19, 255), new Color32(27, 27, 27, 255), new Color32(39, 39, 39, 255), new Color32(61, 61, 61, 255), new Color32(93, 93, 93, 255), new Color32(133, 133, 133, 255), new Color32(180, 180, 180, 255), new Color32(255, 255, 255, 255), new Color32(199, 207, 221, 255), new Color32(146, 161, 185, 255), new Color32(101, 115, 146, 255), new Color32(66, 76, 110, 255), new Color32(42, 47, 78, 255), new Color32(26, 25, 50, 255), new Color32(14, 7, 27, 255), new Color32(28, 18, 28, 255), new Color32(57, 31, 33, 255), new Color32(93, 44, 40, 255), new Color32(138, 72, 54, 255), new Color32(191, 111, 74, 255), new Color32(230, 156, 105, 255), new Color32(246, 202, 159, 255), new Color32(249, 230, 207, 255), new Color32(237, 171, 80, 255), new Color32(224, 116, 56, 255), new Color32(198, 69, 36, 255), new Color32(142, 37, 29, 255), new Color32(255, 80, 0, 255), new Color32(237, 118, 20, 255), new Color32(255, 162, 20, 255), new Color32(255, 200, 37, 255), new Color32(255, 235, 87, 255), new Color32(211, 252, 126, 255), new Color32(153, 230, 95, 255), new Color32(90, 197, 79, 255), new Color32(51, 152, 75, 255), new Color32(30, 111, 80, 255), new Color32(19, 76, 76, 255), new Color32(12, 46, 68, 255), new Color32(0, 57, 109, 255), new Color32(0, 105, 170, 255), new Color32(0, 152, 220, 255), new Color32(0, 205, 249, 255), new Color32(12, 241, 255, 255), new Color32(148, 253, 255, 255), new Color32(253, 210, 237, 255), new Color32(243, 137, 245, 255), new Color32(219, 63, 253, 255), new Color32(122, 9, 250, 255), new Color32(48, 3, 217, 255), new Color32(12, 2, 147, 255), new Color32(3, 25, 63, 255), new Color32(59, 20, 67, 255), new Color32(98, 36, 97, 255), new Color32(147, 56, 143, 255), new Color32(202, 82, 201, 255), new Color32(200, 80, 134, 255), new Color32(246, 129, 135, 255), new Color32(245, 85, 93, 255), new Color32(234, 50, 60, 255), new Color32(196, 36, 48, 255), new Color32(137, 30, 43, 255), new Color32(87, 28, 39, 255)
        };

        [Button("Test")]
        public void Test()
        {
            Texture2D texture2D;
            if (_image != null)
            {
                _original = _image.sprite;
                texture2D = _image.sprite.texture;
            }
            else
            {
                _original = _spriteRenderer.sprite;
                texture2D = _spriteRenderer.sprite.texture;
            }
            
            var copyTexture = Instantiate(texture2D);
            
            if (_image != null)
            {
                _image.sprite = Sprite.Create(copyTexture, new Rect(0.0f, 0.0f, texture2D.width, texture2D.height), new Vector2(0.5f, 0.5f), _original.pixelsPerUnit);
            }
            else
            {
                _spriteRenderer.sprite = Sprite.Create(copyTexture, _original.rect, new Vector2(0.5f, -0.25f), _original.pixelsPerUnit);
            }
            
            var test = GetPixels(copyTexture, _colour);
            copyTexture.SetPixels32(test);
            copyTexture.Apply();
            
        }

        [Button("Reset")]
        public void Reset()
        {
            if (_image != null)
            {
                _image.sprite = _original;
            }
            else
            {
                _spriteRenderer.sprite = _original;
            }
        }
        
        public Color32[] GetPixels(Texture2D texture, Color paint)
        {
            var pixels = texture.GetPixels32();

            // if (mask != null)
            // {
            //     for (var i = 0; i < pixels.Length; i++)
            //     {
            //         if (mask[i].a <= 0)
            //         {
            //             pixels[i] = new Color32();
            //         }
            //         else if (mask[i] == Color.black)
            //         {
            //             pixels[i] = mask[i];
            //         }
            //     }
            // }

            if (paint != Color.white)
            {
                pixels = Repaint3C(pixels, paint, Palette);
                
                // if (Name == "Head" || Name == "Body" || Name == "Arms" || Name == "Hair")
                // {
                //     _pixels = Repaint3C(_pixels, paint, SpriteCollection.Palette);
                // }
                // else
                // {
                //     for (var i = 0; i < _pixels.Length; i++)
                //     {
                //         if (_pixels[i].a > 0) _pixels[i] *= paint;
                //     }
                // }
            }

           // if (Mathf.Approximately(h, 0) && Mathf.Approximately(s, 0) && Mathf.Approximately(v, 0)) return pixels;

            // for (var i = 0; i < pixels.Length; i++)
            // {
            //     if (pixels[i].a > 0) //&& pixels[i] != Color.black)
            //     {
            //         pixels[i] = AdjustColor(pixels[i], h, s, v);
            //     }
            // }

            pixels = ApplyPalette(pixels, Palette);

            return pixels;
        }
        
        public static Color AdjustColor(Color color, float hue, float saturation, float value)
        {
            hue /= 180f;
            saturation /= 100f;
            value /= 100f;

            var a = color.a;

            Color.RGBToHSV(color, out var h, out var s, out var v);

            h += hue / 2f;

            if (h > 1) h -= 1;
            else if (h < 0) h += 1;

            color = Color.HSVToRGB(h, s, v);

            var grey = 0.3f * color.r + 0.59f * color.g + 0.11f * color.b;

            color.r = grey + (color.r - grey) * (saturation + 1);
            color.g = grey + (color.g - grey) * (saturation + 1);
            color.b = grey + (color.b - grey) * (saturation + 1);

            if (color.r < 0) color.r = 0;
            if (color.g < 0) color.g = 0;
            if (color.b < 0) color.b = 0;

            color.r += value * color.r;
            color.g += value * color.g;
            color.b += value * color.b;
            color.a = a;

            return color;
        }

       public static Color32[] ApplyPalette(Color32[] pixels, List<Color32> palette)
        {
            var unique = new ColorDistinctor(pixels).UniqueColors.OrderByDescending(i => pixels.Count(j => FastEquals(i, j))).ToList();
            var map = new Dictionary<Color32, Color32>();
            var mapInvert = new Dictionary<Color32, Color32>();

            foreach (var color in unique)
            {
                if (color.a == 0) continue;

                var nearest = FindNearest(color, palette);
                var used = mapInvert.ContainsKey(nearest);

                if (used)
                {
                    var match = mapInvert[nearest];
                    var dist = ColorDiff.GetEuclidean(color, match);

                    if (dist > 2500)
                    {
                        Debug.Log($"Bad mapping (glued pixels) found: {color} / {match} dist={dist}.");

                        var alternative = FindNearest(color, palette, palette.IndexOf(nearest));

                        used = mapInvert.ContainsKey(alternative);

                        if (used)
                        {
                            Debug.LogWarning("Bad mapping not fixed.");
                        }
                        else
                        {
                            nearest = alternative;
                            Debug.Log("Bad mapping fixed.");
                        }
                    }
                }
                else
                {
                    mapInvert.Add(nearest, color);
                }

                map.Add(color, nearest);
            }

            for (var i = 0; i < pixels.Length; i++)
            {
                if (pixels[i].a < 255)
                {
                    pixels[i] = new Color32();
                }
                else
                {
                    var color = map[pixels[i]];

                    color.a = pixels[i].a;
                    pixels[i] = color;
                }
            }

            return pixels;
        }
       
        private static Color32 FindNearest(Color32 color, List<Color32> palette, int ignore = -1)
        {
            var nearest = palette[ignore == 0 ? 1 : 0];
            var difference = ColorDiff.GetEuclidean(color, nearest);

            for (var j = 1; j < palette.Count; j++)
            {
                if (j == ignore) continue;

                var d = ColorDiff.GetEuclidean(color, palette[j]);

                if (d >= difference) continue;

                difference = d;
                nearest = palette[j];
            }

            return nearest;
        }

        public static bool FastEquals(Color32 a, Color32 b)
        {
            return a.r == b.r && a.g == b.g && a.b == b.b && a.a == b.a;
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
    }
}
