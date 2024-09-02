using System.Collections.Generic;
using CodeMonkey.Utils;
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
        }
        
        public void OnEnter()
        {
            CursorManager.Instance.ChangeCursorState(ECursorState.Default);
        }

        public void OnExit()
        {
            
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
            
            if (playerInteractable != null && !playerInteractable.IsClickDisabled)
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
            if (playerInteractable != null && !playerInteractable.IsClickDisabled)
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
