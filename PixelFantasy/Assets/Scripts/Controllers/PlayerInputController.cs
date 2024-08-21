using System.Collections.Generic;
using Characters;
using CodeMonkey.Utils;
using Managers;
using Systems.CursorHandler.Scripts;
using Systems.Zones.Scripts;
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
        private Kinling _curSelectedKinling;

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
            //ClearSelection();
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

        public Kinling SelectedKinling => _curSelectedKinling;

        public void SelectUnit(Kinling kinling)
        {
            CloseDetailsPanels();

            if (_curSelectedObject != null)
            {
                _curSelectedObject.UnselectObject();
            }
            
            _curSelectedKinling = kinling;
            HUDController.Instance.ShowUnitDetails(kinling);

            _curSelectedObject = kinling.GetClickObject();
            _curSelectedObject.SelectObject();
            
            CommandController.Instance.HideCommands();
        }

        public void SelectObject(ClickObject clickObject)
        {
            CloseDetailsPanels();

            _curSelectedKinling = null;
            if (_curSelectedObject != null)
            {
                _curSelectedObject.UnselectObject();
            }

            HUDController.Instance.ShowItemDetails(clickObject.Owner);

            _curSelectedObject = clickObject;
            _curSelectedObject.SelectObject();
            
            CommandController.Instance.HideCommands();
        }

        public void OnClickObjectDestroy(ClickObject clickObject)
        {
            if (_curSelectedObject == clickObject)
            {
                ClearSelection();
            }
        }

        public void ClearSelection()
        {
            HUDController.Instance.HideDetails();
            CommandController.Instance.HideCommands();
            
            if (_curSelectedObject != null)
            {
                _curSelectedObject.UnselectObject();
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
            _playerInputState = newState;
        }
        
        public PlayerInputState GetCurrentState()
        {
            return _playerInputState;
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
        //BuildBuilding,
        BuildDoor,
    }
}
