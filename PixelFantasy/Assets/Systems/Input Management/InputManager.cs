using System.Collections.Generic;
using Managers;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Systems.Input_Management
{
    public enum InputMode
    {
        Default,
        CommandSelection,
        WallPlanning,
        FloorPlanning,
        DoorPlanning,
        PlaceFurniture,
        ZonePlanning,
        // Add other modes as needed
    }
    
    public class InputManager : Singleton<InputManager>
    {
        private Dictionary<InputMode, IInputHandler> _inputHandlers = new Dictionary<InputMode, IInputHandler>();
        private IInputHandler _currentInputHandler;
        [ShowInInspector] private InputMode _currentMode = InputMode.Default;

        protected override void Awake()
        {
            base.Awake();
            
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
            //if (_currentMode == mode) return _currentInputHandler;

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

        public IInputHandler ReturnToDefault()
        {
            return SetInputMode(InputMode.Default);
        }
    }
}
