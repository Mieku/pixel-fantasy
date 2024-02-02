using System.Collections.Generic;
using Characters;
using CodeMonkey.Utils;
using Managers;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Controllers
{
    public class PlayerInputController : Singleton<PlayerInputController>
    {
        private PlayerInputState _playerInputState = PlayerInputState.None;
        private Vector3 _currentMousePos;
        private ClickObject _curSelectedObject;
        private bool _isOverUI;
        private bool _buildingInternalViewEnabled;

        public string StoredKey;

        protected override void Awake()
        {
            base.Awake();
            GameEvents.OnRightClickUp += GameEvents_OnRightClickUp;
        }

        private void OnDestroy()
        {
            GameEvents.OnRightClickUp -= GameEvents_OnRightClickUp;
        }
        
        protected void GameEvents_OnRightClickUp(Vector3 mousePos, PlayerInputState inputState, bool isOverUI) 
        {
            ClearSelection();
        }
        
        private void Update()
        {
            DetectMouseInput();
            DetectKeyboardInput();
        }

        private void CloseDetailsPanels()
        {
            HUDController.Instance.HideDetails();
        }

        public void SelectUnit(ClickObject clickObject, Kinling kinling)
        {
            CloseDetailsPanels();
            
            if (_curSelectedObject != null)
            {
                _curSelectedObject.UnselectObject();
            }
            
            _curSelectedObject = clickObject;
            
            if (_curSelectedObject != null)
            {
                _curSelectedObject.SelectObject();
                HUDController.Instance.ShowUnitDetails(kinling);
            }
        }

        public void SelectObject(ClickObject clickObject)
        {
            CloseDetailsPanels();
            
            if (_curSelectedObject != null)
            {
                _curSelectedObject.UnselectObject();
            }

            _curSelectedObject = clickObject;

            if (_curSelectedObject != null)
            {
                _curSelectedObject.SelectObject();
                HUDController.Instance.ShowItemDetails(_curSelectedObject.Owner);
            }
        }

        public void ClearSelection()
        {
            if (_curSelectedObject != null)
            {
                _curSelectedObject.UnselectObject();
                HUDController.Instance.HideDetails();
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

            if (Input.GetKeyDown(KeyCode.R))
            {
                _buildingInternalViewEnabled = !_buildingInternalViewEnabled;
                GameEvents.Trigger_OnHideRoofsToggled(_buildingInternalViewEnabled);
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
            
            EnableStructureGuides(newState);
        }
        public void ChangeState(PlayerInputState newState, string key)
        {
            ClearStoredData();
            _playerInputState = newState;
            StoredKey = key;

            EnableStructureGuides(newState);
        }

        private void EnableStructureGuides(PlayerInputState newState)
        {
            if (newState is PlayerInputState.Zone 
                or PlayerInputState.BuildFlooring)
            {
                GameEvents.Trigger_OnStructureGuideToggled(true);
            }
        }

        public PlayerInputState GetCurrentState()
        {
            return _playerInputState;
        }

        /// <summary>
        /// Clears any stored input state data (ex: type of structure to build)
        /// </summary>
        public void ClearStoredData()
        {
            StoredKey = null;
            GameEvents.Trigger_OnStructureGuideToggled(false);
        }
    }

    public enum PlayerInputState
    {
        None,
        BuildStorage,
        CHEAT_SpawnResource,
        BuildFlooring,
        BuildFurniture,
        BuildFarm,
        Zone,
        BuildBuilding,
    }
}
