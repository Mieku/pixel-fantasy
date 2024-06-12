using System.Collections.Generic;
using Characters;
using UnityEngine;
using UnityEngine.UI;

namespace Systems.Appearance.Scripts
{
    public class Portrait : MonoBehaviour
    {
        [SerializeField] private Image _appearance;

        private KinlingData _kinlingData;
        private AvatarData _avatarData => _kinlingData.Avatar;

        public void Init(KinlingData kinlingData)
        {
            _kinlingData = kinlingData;

            _appearance.sprite = _kinlingData.Avatar.GetBaseAvatarSprite();
        }

        // Originally for portrait image, decided to keep for the future
        private void ColourImage(Image image, Color32 colour)
        {
            Texture2D texture2D;
            var original = image.sprite;
            texture2D = image.sprite.texture;
            
            var copyTexture = Instantiate(texture2D);
            image.sprite = Sprite.Create(copyTexture, new Rect(0.0f, 0.0f, texture2D.width, texture2D.height), new Vector2(0.5f, 0.5f), original.pixelsPerUnit);

            var pixels = copyTexture.GetPixels32();
            var colourPixels =
                Helper.Repaint4C(pixels, colour, AppearanceCollection.Palette, new List<Color32>() {new Color32(19, 19, 19, 255), Color.white});
            
            
            copyTexture.SetPixels32(colourPixels);
            copyTexture.Apply();
        }
    }
}
