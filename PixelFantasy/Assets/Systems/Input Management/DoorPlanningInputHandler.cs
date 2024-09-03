using System;
using Managers;
using ScriptableObjects;
using Systems.Buildings.Scripts;
using Systems.Notifications.Scripts;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Systems.Input_Management
{
    public class DoorPlanningInputHandler : MonoBehaviour, IInputHandler
    {
        private Door _plannedDoor;
        private DoorSettings _doorSettings;
        private DyeSettings _dyeSettings;
        private Action _onCompletedCallback;
        
        private bool _isPlanning;
        
        private void Awake()
        {
            InputManager.Instance.RegisterInputHandler(InputMode.DoorPlanning, this);
        }
        
        public void PlanDoor(DoorSettings doorSettings, DyeSettings dyeSettings, Action onCompletedCallback)
        {
            _doorSettings = doorSettings;
            _dyeSettings = dyeSettings;
            _onCompletedCallback = onCompletedCallback;
            
            _plannedDoor = SpawnDoor(_doorSettings, _dyeSettings, _onCompletedCallback);
            
            _isPlanning = true;
        }
        
        private Door SpawnDoor(DoorSettings doorSettings, DyeSettings dyeSettings, Action onPlaced)
        {
            Door prefab = doorSettings.DoorPrefab;
            
            var door = Instantiate(prefab, ParentsManager.Instance.transform);
            door.Init(doorSettings, dyeSettings);
            door.OnDoorPlaced = onPlaced;
            return door;
        }
        
        public void CancelPlacement()
        {
            Destroy(_plannedDoor.gameObject);
            _plannedDoor = null;
            _doorSettings = null;
            _dyeSettings = null;
            _isPlanning = false;
            
            _onCompletedCallback?.Invoke();
        }
        
        public void HandleInput()
        {
            bool overUI = EventSystem.current.IsPointerOverGameObject();
            
            if (Input.GetMouseButtonDown(0) && _isPlanning && !overUI)
            {
                if (_plannedDoor.CheckPlacement())
                {
                    _plannedDoor.SetState(EConstructionState.Blueprint);
                    _plannedDoor.TriggerPlaced();
                    _plannedDoor = null;
                    
                    PlanDoor(_doorSettings, _dyeSettings, _onCompletedCallback);
                }
                else
                {
                    NotificationManager.Instance.Toast("Invalid Location");
                }
            }

            if (Input.GetMouseButtonDown(1) && _isPlanning)
            {
                CancelPlacement();
            }
        }

        public void OnEnter()
        {
            
        }

        public void OnExit()
        {
            if (_isPlanning)
            {
                CancelPlacement();
            }
        }
    }
}
