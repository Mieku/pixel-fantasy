using System;
using Controllers;
using UnityEngine;

namespace HUD
{
    /// <summary>
    /// Attach this to canvases to be able to toggle their visibility on command
    /// </summary>
    [RequireComponent(typeof(CanvasGroup))]
    public class UIVisibilityComponent : MonoBehaviour
    {
        private CanvasGroup _canvasGroup;
        
        private void Awake()
        {
            _canvasGroup = GetComponent<CanvasGroup>();
            UIController.OnVisibilityChanged += OnVisibilityChanged;
            OnVisibilityChanged(UIController.IsUIVisible());
        }

        private void OnDestroy()
        {
            UIController.OnVisibilityChanged -= OnVisibilityChanged;
        }

        private void OnVisibilityChanged(bool isVisible)
        {
            if (isVisible)
            {
                _canvasGroup.alpha = 1;
                _canvasGroup.interactable = true;
                _canvasGroup.blocksRaycasts = true;
            }
            else
            {
                _canvasGroup.alpha = 0;
                _canvasGroup.interactable = false;
                _canvasGroup.blocksRaycasts = false;
            }
        }
    }
}
