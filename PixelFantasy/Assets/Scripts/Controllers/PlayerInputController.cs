using CodeMonkey.Utils;
using Gods;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Controllers
{
    public class PlayerInputController : God<PlayerInputController>
    {
        private PlayerInputState _playerInputState = PlayerInputState.None;
        private Vector3 _currentMousePos;
        private ClickObject _curSelectedObject;
        private bool _isOverUI;

        public string StoredKey;
        
        private void Update()
        {
            DetectMouseInput();
            DetectKeyboardInput();
        }

        public void SelectObject(ClickObject clickObject, SelectionData selectionData = null)
        {
            if (_curSelectedObject != null)
            {
                _curSelectedObject.UnselectObject();
                HUDController.Instance.HideItemDetails();
            }

            _curSelectedObject = clickObject;

            if (_curSelectedObject != null)
            {
                _curSelectedObject.SelectObject();
                HUDController.Instance.ShowItemDetails(selectionData);
            }
        }

        #region Mouse Handlers

        private void DetectMouseInput()
        {
            _currentMousePos = UtilsClass.GetMouseWorldPosition();
            _isOverUI = EventSystem.current.IsPointerOverGameObject();

            // Left Click
            if (Input.GetMouseButtonDown(0))
            {
                LeftClickDown();
            }

            if (Input.GetMouseButton(0))
            {
                LeftClickHeld();
            }

            if (Input.GetMouseButtonUp(0))
            {
                LeftClickUp();
            }
            
            // Right Click
            if (Input.GetMouseButtonDown(1))
            {
                RightClickDown();
            }
            
            if (Input.GetMouseButton(1))
            {
                RightClickHeld();
            }

            if (Input.GetMouseButtonUp(1))
            {
                RightClickUp();
            }
        }

        private void LeftClickDown()
        {
            GameEvents.Trigger_OnLeftClickDown(_currentMousePos, _playerInputState, _isOverUI);
        }

        private void LeftClickHeld()
        {
            GameEvents.Trigger_OnLeftClickHeld(_currentMousePos, _playerInputState, _isOverUI);
        }

        private void LeftClickUp()
        {
            GameEvents.Trigger_OnLeftClickUp(_currentMousePos, _playerInputState, _isOverUI);
        }

        private void RightClickDown()
        {
            GameEvents.Trigger_OnRightClickDown(_currentMousePos, _playerInputState, _isOverUI);
        }

        private void RightClickHeld()
        {
            GameEvents.Trigger_OnRightClickHeld(_currentMousePos, _playerInputState, _isOverUI);
        }

        private void RightClickUp()
        {
            GameEvents.Trigger_OnRightClickUp(_currentMousePos, _playerInputState, _isOverUI);
        }

        #endregion

        #region Keyboard Handlers

        private void DetectKeyboardInput()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                EscapePressed();
            }
        }

        private void EscapePressed()
        {
            ChangeState(PlayerInputState.None);
        }
        
        #endregion

        /// <summary>
        /// Changes the input state for the player
        /// </summary>
        public void ChangeState(PlayerInputState newState)
        {
            ClearStoredData();
            _playerInputState = newState;
        }
        public void ChangeState(PlayerInputState newState, string key)
        {
            ClearStoredData();
            _playerInputState = newState;
            StoredKey = key;
        }

        public PlayerInputState GetCurrentState()
        {
            return _playerInputState;
        }

        /// <summary>
        /// Clears any stored input state data (ex: type of structure to build)
        /// </summary>
        private void ClearStoredData()
        {
            StoredKey = null;
            SelectObject(null);
        }
    }

    public enum PlayerInputState
    {
        None,
        BuildStorage,
        CHEAT_SpawnResource,
        BuildStructure,
        BuildFlooring,
    }
}
