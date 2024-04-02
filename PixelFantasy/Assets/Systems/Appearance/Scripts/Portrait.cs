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
        [SerializeField] private Image _mouth;
        [SerializeField] private Image _nose;
        [SerializeField] private Image _eyes;
        [SerializeField] private Image _eyelashes;
        [SerializeField] private Image _eyebrows;
        [SerializeField] private Image _blush;
        [SerializeField] private Image _hair;
        
        private const float BLINK_SPEED = 0.1f;
        private const float MIN_EYES_OPEN = 1.0f;
        private const float MAX_EYES_OPEN = 4.0f;
        
        private bool _eyesClosed;
        private float _eyesTimer;
        private float _stareTime;
        private float _blinkTime;
        private bool _isBlinking;

        private KinlingData _kinlingData;

        public void Init(KinlingData kinlingData)
        {
            _kinlingData = kinlingData;

            _head.sprite = kinlingData.Appearance.SkinTone.PortraitHead;
            _blush.sprite = kinlingData.Appearance.SkinTone.PortraitBlush;
            _nose.sprite = kinlingData.Appearance.SkinTone.PortraitNose;
            _eyes.sprite = kinlingData.Appearance.Eyes.PortraitEyes;
            _eyelashes.sprite = kinlingData.Appearance.Eyelashes;
            _eyebrows.sprite = kinlingData.Appearance.Eyebrows;
            _hair.sprite = kinlingData.Appearance.Hair.Portrait;
            
            _blush.gameObject.SetActive(kinlingData.Gender == Gender.Female);
        }

        private void Update()
        {
            //HandleBlinking();
            UpdateMood();
        }

        private void UpdateMood()
        {
            
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
