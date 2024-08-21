using System;
using Controllers;
using DG.Tweening;
using ScriptableObjects;
using UnityEngine;
using UnityEngine.UI;

namespace HUD
{
    public class WorkDisplay : MonoBehaviour
    {
        [SerializeField] private GameObject _canvasHandle;
        [SerializeField] private Image _fillImg;
        [SerializeField] private CanvasGroup _canvasGroup;
        [SerializeField] private float _fadeDuration = 0.5f; // duration of the fade in/out
        
        private void Awake()
        {
            _canvasHandle.SetActive(false);
        }

        private void OnEnable()
        {
            OnCameraZoomChanged(CameraManager.Instance.GetCamaraZoom());
            
            GameEvents.OnCameraZoomChanged += OnCameraZoomChanged;
        }

        private void OnDisable()
        {
            GameEvents.OnCameraZoomChanged -= OnCameraZoomChanged;
        }

        private void OnCameraZoomChanged(float zoomAmount)
        {
            if (zoomAmount > GameSettings.Instance.MaxZoomDisplayWork)
            {
                _canvasGroup.DOFade(0, _fadeDuration);
            }
            else
            {
                _canvasGroup.DOFade(1, _fadeDuration);
            }
        }

        public void Show(bool shouldShow)
        {
            _canvasHandle.SetActive(shouldShow);
        }

        public void SetProgress(float progressPercent)
        {
            _canvasHandle.SetActive(true);
            
            progressPercent = Mathf.Clamp01(progressPercent);
            _fillImg.fillAmount = progressPercent;
        }
    }
}
