using CodeMonkey.Utils;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Systems.Input_Management
{
    public class SelectionInputHandler : MonoBehaviour, IInputHandler
    {
        private void Awake()
        {
            // Register this handler with the InputManager
            InputManager.Instance.RegisterInputHandler(InputMode.Selection, this);
        }
        
        public void HandleInput()
        {
            if (Input.GetMouseButtonDown(0))
            {
                HandleLeftClick();
            }

            // If you are handling selection box updates in this input mode
            SelectionManager.Instance.UpdateSelectionBox();
        }

        public void OnEnter()
        {
            Debug.Log("Entered Selection Mode");
        }

        public void OnExit()
        {
            Debug.Log("Exited Selection Mode");
        }
        
        private void HandleLeftClick()
        {
            Vector3 mousePosition = UtilsClass.GetMouseWorldPosition();
            if (EventSystem.current.IsPointerOverGameObject()) return;

            // Delegate the selection handling to SelectionManager
            SelectionManager.Instance.HandleClick(mousePosition);
        }
    }
}