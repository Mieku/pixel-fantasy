using System;
using System.Collections.Generic;
using System.Linq;
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
        public AppearanceCollection Collection;

        public AvatarData DebugAvatarData;

        private AvatarData _avatarData
        {
            get
            {
                if (_kinling.RuntimeData != null && _kinling.RuntimeData.Avatar != null)
                {
                    return _kinling.RuntimeData.Avatar;
                }

                return DebugAvatarData;
            }
        }

        [Button("Update Appearance")]
        public void UpdateAppearance()
        {
            Avatar.SideSpriteLibraryAsset = BuildSpriteLibraryAssetForDirection(AvatarLayer.EAppearanceDirection.Right);
            Avatar.UpSpriteLibraryAsset = BuildSpriteLibraryAssetForDirection(AvatarLayer.EAppearanceDirection.Up);
            Avatar.DownSpriteLibraryAsset = BuildSpriteLibraryAssetForDirection(AvatarLayer.EAppearanceDirection.Down);
            
            Avatar.RefreshAppearanceLibrary();
        }
        
        public SpriteLibraryAsset BuildSpriteLibraryAssetForDirection(AvatarLayer.EAppearanceDirection direction, string changed = null, bool forceMerge = false)
        {
            var collection = Collection.GetLayersByDirection(direction);
            
            var width = collection[0].Textures[0].width;
            var height = collection[0].Textures[0].height;
            
            var dict = collection.ToDictionary(i => i.ID, i => i);
            var layers = new Dictionary<string, Color32[]>();

            if(!string.IsNullOrEmpty(_avatarData.Body)) layers.Add("Body", dict["Body"].GetPixels(_avatarData.Body, null, changed));
            if(!string.IsNullOrEmpty(_avatarData.Clothing)) layers.Add("Clothing", dict["Clothing"].GetPixels(_avatarData.Clothing, null, changed));

            if(!string.IsNullOrEmpty(_avatarData.Eyes)) layers.Add("Eyes", dict["Eyes"].GetPixels(_avatarData.Eyes, null, changed));
            if(!string.IsNullOrEmpty(_avatarData.Blush)) layers.Add("Blush", dict["Blush"].GetPixels(_avatarData.Blush, null, changed));

            if(!string.IsNullOrEmpty(_avatarData.Beard)) layers.Add("Beard", dict["Beard"].GetPixels(_avatarData.Beard, null, changed));
            if (!string.IsNullOrEmpty(_avatarData.Hair)) layers.Add("Hair", dict["Hair"].GetPixels(_avatarData.Hair, null, changed));

            if(!string.IsNullOrEmpty(_avatarData.FaceAccessory)) layers.Add("FaceAccessory", dict["FaceAccessory"].GetPixels(_avatarData.FaceAccessory, null, changed));
            if(!string.IsNullOrEmpty(_avatarData.Hat)) layers.Add("Hat", dict["Hat"].GetPixels(_avatarData.Hat, null, changed));

            if(!string.IsNullOrEmpty(_avatarData.Hands)) layers.Add("Hands", dict["Hands"].GetPixels(_avatarData.Hands, null, changed));
            if(!string.IsNullOrEmpty(_avatarData.Offhand)) layers.Add("Offhand", dict["Offhand"].GetPixels(_avatarData.Offhand, null, changed));
            if(!string.IsNullOrEmpty(_avatarData.Weapon)) layers.Add("Weapon", dict["Weapon"].GetPixels(_avatarData.Weapon, null, changed));


            var order = collection.Select(i => i.ID).ToList();

            layers = layers.Where(i => i.Value != null).OrderBy(i => order.IndexOf(i.Key)).ToDictionary(i => i.Key, i => i.Value);
            
            if(!string.IsNullOrEmpty(_avatarData.Offhand)) 
            {
                var offHand = layers["Offhand"];
                var last = layers.Last(i => i.Key != "Weapon");
                var copy = last.Value.ToArray();

                for (var i = 2 * 32 * width; i < 3 * 32 * width; i++)
                {
                    if (offHand[i].a > 0) copy[i] = offHand[i];
                }

                layers[last.Key] = copy;
            }
            var Texture = new Texture2D(width, height) { filterMode = FilterMode.Point };
            Texture = MergeLayers(Texture, layers.Values.ToArray());
            
            Texture.SetPixels(0, Texture.height - 32, 32, 32, new Color[32 * 32]);
            
            var clipNames = new List<string>(_clipNames);
                
            clipNames.Reverse();

            var sprites = new Dictionary<string, Sprite>();
            int maxFramesPerClip = width / 32;

            for (var i = 0; i < clipNames.Count; i++)
            {
                for (var j = 0; j < maxFramesPerClip; j++)
                {
                    var key = clipNames[i] + "_" + j;

                    sprites.Add(key, Sprite.Create(Texture, new Rect(j * 32, i * 32, 32, 32), new Vector2(0.5f, 0.125f), 16, 0, SpriteMeshType.FullRect));
                }
            }
            
            var spriteLibraryAsset = ScriptableObject.CreateInstance<SpriteLibraryAsset>();

            switch (direction)
            {
                case AvatarLayer.EAppearanceDirection.Right:
                    spriteLibraryAsset.name = "Right";
                    break;
                case AvatarLayer.EAppearanceDirection.Left:
                    spriteLibraryAsset.name = "Left";
                    break;
                case AvatarLayer.EAppearanceDirection.Up:
                    spriteLibraryAsset.name = "Up";
                    break;
                case AvatarLayer.EAppearanceDirection.Down:
                    spriteLibraryAsset.name = "Down";
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(direction), direction, null);
            }

            foreach (var sprite in sprites)
            {
                var split = sprite.Key.Split('_');

                spriteLibraryAsset.AddCategoryLabel(sprite.Value, split[0], split[1]);
            }
            
            return spriteLibraryAsset;
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
