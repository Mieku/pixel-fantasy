using System.Collections.Generic;
using UnityEngine;

namespace Systems.Input_Management
{
    public enum InputMode
    {
        Selection,
        WallPlanning,
        // Add other modes as needed
    }
    
    public class InputManager : MonoBehaviour
    {
        public static InputManager Instance { get; private set; }
        
        private Dictionary<InputMode, IInputHandler> _inputHandlers = new Dictionary<InputMode, IInputHandler>();
        private IInputHandler _currentInputHandler;
        private InputMode _currentMode = InputMode.Selection;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(this.gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(this.gameObject);

            // Initialize with default mode
            SetInputMode(_currentMode);
        }

        private void Update()
        {
            _currentInputHandler?.HandleInput();
        }

        public void RegisterInputHandler(InputMode mode, IInputHandler handler)
        {
            if (!_inputHandlers.ContainsKey(mode))
            {
                _inputHandlers.Add(mode, handler);
            }
        }

        public IInputHandler SetInputMode(InputMode mode)
        {
            if (_currentMode == mode) return null;

            // Exit current handler
            _currentInputHandler?.OnExit();

            // Set new mode
            _currentMode = mode;

            if (_inputHandlers.TryGetValue(mode, out IInputHandler handler))
            {
                _currentInputHandler = handler;
                _currentInputHandler.OnEnter();
                return handler;
            }

            _currentInputHandler = null;
            return null;
        }

        // Optional: Expose method to switch modes from other scripts
        public IInputHandler SwitchToMode(InputMode mode)
        {
            return SetInputMode(mode);
        }
    }
}
