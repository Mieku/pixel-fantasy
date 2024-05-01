using System;
using System.Collections.Generic;
using System.Linq;
using Assets.PixelFantasy.PixelHeroes.Common.Scripts.Utils;
using Characters;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.U2D.Animation;

namespace Systems.Appearance.Scripts
{
    public class AppearanceBuilder : MonoBehaviour
    {
        [SerializeField] private Kinling _kinling;
        [SerializeField] private List<string> _clipNames = new List<string>();
        [SerializeField] private SpriteLibrary _spriteLibrary;

        public Avatar Avatar;
        public Texture2D Texture { get; private set; }
        public AppearanceCollection Collection;
        
        private AvatarData _avatarData => _kinling.RuntimeData.Avatar;
        private Dictionary<string, Sprite> _sprites;

        [Button("Rebuild")]
        public void Rebuild(string changed = null, bool forceMerge = false)
        {
            // TODO: Not a fan of this
            var width = Collection.Layers[0].GetTexturesByDirection(Avatar.Direction)[0].width;
            var height = Collection.Layers[0].GetTexturesByDirection(Avatar.Direction)[0].height;
            
            var dict = Collection.Layers.ToDictionary(i => i.Name, i => i);
            var layers = new Dictionary<string, Color32[]>();
            
            if (_avatarData.Body != "") layers.Add("Body", dict["Body"].GetPixels(_avatarData.Body, null, changed, Avatar.Direction));
            if (_avatarData.Hands != "") layers.Add("Hands", dict["Hands"].GetPixels(_avatarData.Hands, null, changed, Avatar.Direction));
            if (_avatarData.Eyes != "") layers.Add("Eyes", dict["Eyes"].GetPixels(_avatarData.Eyes, null, changed, Avatar.Direction));
            if (_avatarData.Blush != "") layers.Add("Blush", dict["Blush"].GetPixels(_avatarData.Blush, null, changed, Avatar.Direction));
            if (_avatarData.Hair != "") layers.Add("Hair", dict["Hair"].GetPixels(_avatarData.Hair, _avatarData.Hat == "" ? null : layers["Body"], changed, Avatar.Direction));
            
            if (_avatarData.Beard != "") layers.Add("Beard", dict["Beard"].GetPixels(_avatarData.Beard, null, changed, Avatar.Direction));
            if (_avatarData.Clothing != "") layers.Add("Clothing", dict["Clothing"].GetPixels(_avatarData.Clothing, null, changed, Avatar.Direction));
            if (_avatarData.Hat != "") layers.Add("Hat", dict["Hat"].GetPixels(_avatarData.Hat, null, changed, Avatar.Direction));
            if (_avatarData.FaceAccessory != "") layers.Add("FaceAccessory", dict["FaceAccessory"].GetPixels(_avatarData.FaceAccessory, null, changed, Avatar.Direction));
            if (_avatarData.Weapon != "") layers.Add("Weapon", dict["Weapon"].GetPixels(_avatarData.Weapon, null, changed, Avatar.Direction));
            if (_avatarData.Offhand != "") layers.Add("Offhand", dict["Offhand"].GetPixels(_avatarData.Offhand, null, changed, Avatar.Direction));
            
            var order = Collection.Layers.Select(i => i.Name).ToList();

            layers = layers.Where(i => i.Value != null).OrderBy(i => order.IndexOf(i.Key)).ToDictionary(i => i.Key, i => i.Value);

            if (Texture == null) Texture = new Texture2D(width, height) { filterMode = FilterMode.Point };
            
            if (_avatarData.Offhand != "")
            {
                var offHand = layers["Offhand"];
                var last = layers.Last(i => i.Key != "Weapon");
                var copy = last.Value.ToArray();

                for (var i = 2 * 64 * width; i < 3 * 64 * width; i++)
                {
                    if (offHand[i].a > 0) copy[i] = offHand[i];
                }

                layers[last.Key] = copy;
            }
            
            Texture = MergeLayers(Texture, layers.Values.ToArray());
            Texture.SetPixels(0, Texture.height - 32, 32, 32, new Color[32 * 32]);
            
            if (_sprites == null)
            {
                var clipNames = new List<string>(_clipNames);
                
                clipNames.Reverse();

                _sprites = new Dictionary<string, Sprite>();

                for (var i = 0; i < clipNames.Count; i++)
                {
                    for (var j = 0; j < 9; j++)
                    {
                        var key = clipNames[i] + "_" + j;

                        _sprites.Add(key, Sprite.Create(Texture, new Rect(j * 64, i * 64, 64, 64), new Vector2(0.5f, 0.125f), 16, 0, SpriteMeshType.FullRect));
                    }
                }
            }

            var spriteLibraryAsset = ScriptableObject.CreateInstance<UnityEngine.U2D.Animation.SpriteLibraryAsset>();

            foreach (var sprite in _sprites)
            {
                var split = sprite.Key.Split('_');

                spriteLibraryAsset.AddCategoryLabel(sprite.Value, split[0], split[1]);
            }

            _spriteLibrary.spriteLibraryAsset = spriteLibraryAsset;
        }
        
        public Texture2D MergeLayers(Texture2D texture, params Color32[][] layers)
        {
            if (layers.Length == 0) throw new Exception("No layers to merge.");
            
            var result = new Color[texture.width * texture.height];

            foreach (var layer in layers.Where(i => i != null))
            {
                if (layer.Length != result.Length) Debug.LogWarning("Invalid layer size.");

                for (var i = 0; i < result.Length; i++)
                {
                    if (layer[i].a > 0) result[i] = layer[i];
                }
            }

            texture.SetPixels(result);
            texture.Apply();

            return texture;
        }
    }
}
