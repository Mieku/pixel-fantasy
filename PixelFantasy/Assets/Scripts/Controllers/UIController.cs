using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Controllers
{
    public class UIController : MonoBehaviour
    {
        public static UIController Instance;
        public static Action<bool> OnVisibilityChanged;

        private static bool _isVisible = true;
        
        [SerializeField] private InputActionReference _hideUIInputAction;

        protected void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
            
            _hideUIInputAction.action.performed += OnHideUIToggled;
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
            
            _hideUIInputAction.action.performed -= OnHideUIToggled;
        }

        private void OnHideUIToggled(InputAction.CallbackContext ctx)
        {
            ToggleUIVisible();
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