using System;
using Characters;
using Managers;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

namespace Systems.Appearance.Scripts
{
    public class Portrait : MonoBehaviour
    {
        [SerializeField] private Image _head;
        [SerializeField] private Image _ears;
        [SerializeField] private Image _beard;
        [SerializeField] private Image _mouth;
        [SerializeField] private Image _nose;
        [SerializeField] private Image _eyes;
        [SerializeField] private Image _eyelashes;
        [SerializeField] private Image _eyebrows;
        [SerializeField] private Image _blush;
        [SerializeField] private Image _hair;

        [SerializeField] private Sprite _sadMouth;
        [SerializeField] private Sprite _unhappyMouth;
        [SerializeField] private Sprite _happyMouth;
        [SerializeField] private Sprite _veryHappyMouth;
        
        private const float BLINK_SPEED = 0.1f;
        private const float MIN_EYES_OPEN = 1.0f;
        private const float MAX_EYES_OPEN = 4.0f;
        
        private bool _eyesClosed;
        private float _eyesTimer;
        private float _stareTime;
        private float _blinkTime;
        private bool _isBlinking;

        private KinlingData _kinlingData;
        private AvatarData _avatarData => _kinlingData.Avatar;

        public void Init(KinlingData kinlingData)
        {
            _kinlingData = kinlingData;

            _eyelashes.sprite = _avatarData.PortraitEyelashes;
            _eyebrows.sprite = _avatarData.PortraitEyebrows;
            _hair.sprite = _avatarData.HairStyle.Portrait;

            if (_avatarData.BeardStyle != null)
            {
                _beard.sprite = _avatarData.BeardStyle.Portrait;
                _beard.gameObject.SetActive(true);
            }
            else
            {
                _beard.gameObject.SetActive(false);
            }
            
            _blush.gameObject.SetActive(kinlingData.Gender == EGender.Female);
            
            // Colour the portrait
            ColourImage(_hair, _avatarData.HairColour);
            ColourImage(_beard, _avatarData.HairColour);
            ColourImage(_eyes, _avatarData.EyeColour);
            
            ColourImage(_head, _avatarData.SkinTone);
            ColourImage(_ears, _avatarData.SkinTone);
            ColourImage(_nose, _avatarData.SkinTone);
            ColourImage(_blush, _avatarData.SkinTone);
        }

        private void ColourImage(Image image, Color32 colour)
        {
            Texture2D texture2D;
            var original = image.sprite;
            texture2D = image.sprite.texture;
            
            var copyTexture = Instantiate(texture2D);
            image.sprite = Sprite.Create(copyTexture, new Rect(0.0f, 0.0f, texture2D.width, texture2D.height), new Vector2(0.5f, 0.5f), original.pixelsPerUnit);

            var pixels = copyTexture.GetPixels32();
            var colourPixels =
                Helper.Repaint4C(pixels, colour, AppearanceCollection.Palette, new Color32(19, 19, 19, 255));
            
            
            copyTexture.SetPixels32(colourPixels);
            copyTexture.Apply();
        }

        private void Update()
        {
            HandleBlinking();
            UpdateMood();
        }

        private void UpdateMood()
        {
            if (_kinlingData == null) return;
            
            float mood = _kinlingData.Kinling.MoodData.OverallMood / 100f;;
            if (mood < 0.25f)
            {
                _mouth.sprite = _sadMouth;
            }
            else if (mood < .5f)
            {
                _mouth.sprite = _unhappyMouth;
            }
            else if(mood < 0.75f)
            {
                _mouth.sprite = _happyMouth;
            }
            else
            {
                _mouth.sprite = _veryHappyMouth;
            }
        }
        
        private void HandleBlinking()
        {
            _eyesTimer += TimeManager.Instance.DeltaTime;

            if (_isBlinking)
            {
                if (_eyesTimer >= BLINK_SPEED)
                {
                    _eyesTimer = 0f;
                    OpenEyes();
                }
            }
            else if (_eyesTimer >= _stareTime)
            {
                _eyesTimer = 0f;
                _stareTime = Random.Range(MIN_EYES_OPEN, MAX_EYES_OPEN);
                CloseEyes();
            }
        }

        private void OpenEyes()
        {
            _isBlinking = false;
            _eyes.gameObject.SetActive(true);
        }

        private void CloseEyes()
        {
            _isBlinking = true;
            _eyes.gameObject.SetActive(false);
        }
    }
}
