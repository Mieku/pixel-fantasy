using System.Collections.Generic;
using CodeMonkey.Utils;
using Player;
using Systems.CursorHandler.Scripts;
using Systems.Zones.Scripts;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Systems.Input_Management
{
    public class DefaultInputHandler : MonoBehaviour, IInputHandler
    {
        private float _lastClickTime = 0f;
        private const float DOUBLE_CLICK_TIME = 0.3f; // Time in seconds
        private PlayerInteractable _currentlyHoveredPI;
        
        private void Awake()
        {
            // Register this handler with the InputManager
            InputManager.Instance.RegisterInputHandler(InputMode.Default, this);
        }
        
        public void HandleInput()
        {
            if (Input.GetMouseButtonDown(0))
            {
                HandleLeftClick();
            }

            if (Input.GetMouseButtonDown(1))
            {
                if (EventSystem.current.IsPointerOverGameObject()) return;
                
                SelectionManager.Instance.ClearSelection();
                HUDController.Instance.HideDetails();
            }
            
            HandleHover();
        }

        private void HandleHover()
        {
            if(PlayerSettings.OutlineOnHover == false) return;
            
            if (EventSystem.current.IsPointerOverGameObject())
            {
                if (_currentlyHoveredPI != null)
                {
                    _currentlyHoveredPI.OnHoverEnd();
                    _currentlyHoveredPI = null;
                }
                return;
            }
            
            // Get mouse position in world coordinates
            Vector2 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            var playerInteractable = Helper.GetObjectAtPosition<PlayerInteractable>(mousePosition);

            // Handle hover end if we are no longer hovering over the previous object
            if (_currentlyHoveredPI != null && _currentlyHoveredPI != playerInteractable)
            {
                _currentlyHoveredPI.OnHoverEnd();
                _currentlyHoveredPI = null;
            }

            // Handle hover start if we're hovering a new object
            if (playerInteractable != null && playerInteractable != _currentlyHoveredPI)
            {
                playerInteractable.OnHoverStart();
                _currentlyHoveredPI = playerInteractable;
            }
        }
        
        public void OnEnter()
        {
            CursorManager.Instance.ChangeCursorState(ECursorState.Default);
        }

        public void OnExit()
        {
            if (_currentlyHoveredPI != null)
            {
                _currentlyHoveredPI.OnHoverEnd();
                _currentlyHoveredPI = null;
            }
        }
        
        private void HandleLeftClick()
        {
            if (EventSystem.current.IsPointerOverGameObject()) return;
            Vector3 mousePosition = UtilsClass.GetMouseWorldPosition();
            
            float timeSinceLastClick = Time.time - _lastClickTime;
    
            if (timeSinceLastClick <= DOUBLE_CLICK_TIME)
            {
                // Double click detected
                DoubleLeftClickDown(mousePosition);

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
                HandleClick(mousePosition);
            }
        }
        
        private void DoubleLeftClickDown(Vector3 mousePosition)
        {
            var playerInteractable = Helper.GetObjectAtPosition<PlayerInteractable>(mousePosition);
            
            if (playerInteractable != null)
            {
                var visiblePIs = PlayerInteractableDatabase.Instance.GetAllSimilarVisiblePIs(playerInteractable);
                
                if (Input.GetKey(KeyCode.LeftShift)) {}
                
                SelectMultiple(visiblePIs);
            }
        }
        
        public void HandleClick(Vector3 mousePosition)
        {
            var playerInteractable = Helper.GetObjectAtPosition<PlayerInteractable>(mousePosition);
            var zone = Helper.GetObjectAtPosition<ZoneCell>(mousePosition);
            if (playerInteractable != null)
            {
                if (Input.GetKey(KeyCode.LeftShift))
                {
                    SelectionManager.Instance.SelectAdditional(playerInteractable, true);
                }
                else
                {
                    SelectionManager.Instance.ClearSelection();
                    SelectionManager.Instance.Select(playerInteractable);
                }
            }
            else if (zone != null)
            {
                SelectionManager.Instance.ClearSelection();
                zone.RuntimeData.SelectZone();
            }
            else
            {
                SelectionManager.Instance.ClearSelection();
            }
        }
        
        public void SelectMultiple(List<PlayerInteractable> selectedPIs)
        {
            if (Input.GetKey(KeyCode.LeftShift))
            {
                foreach (var pi in selectedPIs)
                {
                    SelectionManager.Instance.SelectAdditional(pi, false);
                }
            }
            else
            {
                SelectionManager.Instance.ClearSelection();
                
                foreach (var pi in selectedPIs)
                {
                    SelectionManager.Instance.Highlight(pi);
                
                }
        
                HUDController.Instance.ShowMultipleDetails(selectedPIs);
            }
        }
    }
}
