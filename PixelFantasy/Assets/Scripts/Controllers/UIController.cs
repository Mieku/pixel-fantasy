using System;
using UnityEngine;

namespace Controllers
{
    public class UIController : MonoBehaviour
    {
        public static UIController Instance;
        public static Action<bool> OnVisibilityChanged;

        private static bool _isVisible = true;

        protected void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }

        public static bool IsUIVisible()
        {
            return _isVisible;
        }

        public static void SetUIVisible(bool isVisible)
        {
            _isVisible = isVisible;
            OnVisibilityChanged?.Invoke(isVisible);
        }

        public static void ToggleUIVisible()
        {
            SetUIVisible(!_isVisible);
        }
    }
}