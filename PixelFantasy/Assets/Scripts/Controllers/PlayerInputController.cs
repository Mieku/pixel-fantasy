using UnityEngine;
using CodeMonkey.Utils;
using Managers;
using Systems.Zones.Scripts;
using UnityEngine.EventSystems;

namespace Controllers
{
    public class PlayerInputController : Singleton<PlayerInputController>
    {
        private PlayerInputState _playerInputState = PlayerInputState.None;
        private Vector3 _currentMousePos;
        private bool _isOverUI;
        
        private float _lastClickTime = 0f;
        private const float DOUBLE_CLICK_TIME = 0.3f; // Time in seconds

        protected override void Awake()
        {
            base.Awake();
        }

        private void Update()
        {
            // DetectMouseInput();
            // DetectKeyboardInput();
        }

        private void DetectMouseInput()
        {
            _currentMousePos = UtilsClass.GetMouseWorldPosition();
            _isOverUI = EventSystem.current.IsPointerOverGameObject();

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

            if (Input.GetMouseButtonDown(1))
            {
                RightClickDown();
            }

            if (Input.GetMouseButtonUp(1))
            {
                RightClickUp();
            }
        }

        private void LeftClickDown()
        {
            if (_isOverUI) return;
            
            float timeSinceLastClick = Time.time - _lastClickTime;
    
            if (timeSinceLastClick <= DOUBLE_CLICK_TIME)
            {
                // Double click detected
                DoubleLeftClickDown();

                // Reset the last click time to avoid unnecessary checks
                _lastClickTime = 0;
            }
            else
            {
                if (timeSinceLastClick > DOUBLE_CLICK_TIME * 2) // Adjust as needed
                {
                    // Reset the timer if the time since the last click is significantly more than the double click threshold
                    _lastClickTime = Time.time;
                }

                // Single click logic, only if not detected as double click
                SingleLeftClickDown();
            }
        }

        private void SingleLeftClickDown()
        {
            // // Trigger selection handling via SelectionManager
            // var playerInteractable = Helper.GetObjectAtPosition<PlayerInteractable>(_currentMousePos);
            // var zone = Helper.GetObjectAtPosition<ZoneCell>(_currentMousePos);
            // if (playerInteractable != null && !playerInteractable.IsClickDisabled)
            // {
            //     SelectionManager.Instance.HandleClick(playerInteractable);
            // }
            // else if (zone != null)
            // {
            //     SelectionManager.Instance.ClearSelection();
            //     zone.RuntimeData.SelectZone();
            // }
            // else
            // {
            //     SelectionManager.Instance.ClearSelection();
            // }
            //
            // GameEvents.Trigger_OnLeftClickDown(_currentMousePos, _playerInputState, _isOverUI);
        }

        private void DoubleLeftClickDown()
        {
            // var playerInteractable = Helper.GetObjectAtPosition<PlayerInteractable>(_currentMousePos);
            //
            // if (playerInteractable != null && !playerInteractable.IsClickDisabled)
            // {
            //     var visiblePIs = PlayerInteractableDatabase.Instance.GetAllSimilarVisiblePIs(playerInteractable);
            //     SelectionManager.Instance.SelectMultiple(visiblePIs);
            // }
        }
        
        private void LeftClickHeld()
        {
            //GameEvents.Trigger_OnLeftClickHeld(_currentMousePos, _playerInputState, _isOverUI);
        }
        
        private void LeftClickUp()
        {
            //GameEvents.Trigger_OnLeftClickUp(_currentMousePos, _playerInputState, _isOverUI);
        }

        private void RightClickDown()
        {
            //GameEvents.Trigger_OnRightClickDown(_currentMousePos, _playerInputState, _isOverUI);
        }
        
        private void RightClickUp()
        {
           // GameEvents.Trigger_OnRightClickUp(_currentMousePos, _playerInputState, _isOverUI);
        }

        private void DetectKeyboardInput()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                EscapePressed();
            }
        }

        private void EscapePressed()
        {
            // ChangeState(PlayerInputState.None);
            // SelectionManager.Instance.ClearSelection();
        }

        public void ChangeState(PlayerInputState newState)
        {
            // _playerInputState = newState;
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
        BuildDoor,
    }
}
