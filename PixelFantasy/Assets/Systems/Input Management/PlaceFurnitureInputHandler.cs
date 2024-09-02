using System;
using Items;
using Managers;
using ScriptableObjects;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

namespace Systems.Input_Management
{
    [Serializable]
    public enum PlacementDirection
    {
        South,
        North,
        West, 
        East
    }
    
    public class PlaceFurnitureInputHandler : MonoBehaviour, IInputHandler
    {
        [SerializeField] private InputActionReference _rotateClockwiseInput;
        [SerializeField] private InputActionReference _rotateCounterClockwiseInput;
        
        private Furniture _plannedFurniture;
        private PlacementDirection _placementDirection;
        private FurnitureSettings _furnitureSettings;
        private DyeSettings _dyeSettings;
        private Action _onCompletedCallback;
        
        private bool _isPlanning;
        
        private void Awake()
        {
            InputManager.Instance.RegisterInputHandler(InputMode.PlaceFurniture, this);
        }

        public void PlanFurniture(FurnitureSettings furnitureSettings, PlacementDirection placementDirection, DyeSettings dyeSettings, Action onCompletedCallback)
        {
            _placementDirection = placementDirection;
            _furnitureSettings = furnitureSettings;
            _dyeSettings = dyeSettings;
            _onCompletedCallback = onCompletedCallback;
            
            _plannedFurniture = SpawnFurniture(_furnitureSettings, _placementDirection, _dyeSettings);
            
            _isPlanning = true;
        }

        private Furniture SpawnFurniture(FurnitureSettings furnitureSettings, PlacementDirection placementDirection, DyeSettings dyeSettings)
        {
            Furniture prefab = furnitureSettings.FurniturePrefab;
            
            var furniture = Instantiate(prefab, ParentsManager.Instance.FurnitureParent);
            furniture.StartPlanning(furnitureSettings, placementDirection, dyeSettings);
            return furniture;
        }

        public void CancelPlacement()
        {
            Destroy(_plannedFurniture.gameObject);
            _plannedFurniture = null;
            _furnitureSettings = null;
            _dyeSettings = null;
            _isPlanning = false;
            
            _onCompletedCallback?.Invoke();
        }

        public void HandleInput()
        {
            bool overUI = EventSystem.current.IsPointerOverGameObject();
            
            if (Input.GetMouseButtonDown(0) && _isPlanning && !overUI)
            {
                if (_plannedFurniture.CheckPlacement())
                {
                    _plannedFurniture.CompletePlanning();
                    _plannedFurniture.InitializeFurniture(_furnitureSettings, _placementDirection, _dyeSettings);
                    _plannedFurniture = null;
                    
                    PlanFurniture(_furnitureSettings, _placementDirection, _dyeSettings, _onCompletedCallback);
                }
            }

            if (Input.GetMouseButtonDown(1) && _isPlanning)
            {
                CancelPlacement();
            }
        }

        private void OnRotateClockwise(InputAction.CallbackContext context)
        {
            if (_plannedFurniture != null && _isPlanning)
            {
                _placementDirection = _plannedFurniture.RotatePlan(true);
            }
        }
        
        private void OnRotateCounterClockwise(InputAction.CallbackContext context)
        {
            if (_plannedFurniture != null && _isPlanning)
            {
                _placementDirection = _plannedFurniture.RotatePlan(false);
            }
        }

        public void OnEnter()
        {
            _rotateClockwiseInput.action.performed += OnRotateClockwise;
            _rotateCounterClockwiseInput.action.performed += OnRotateCounterClockwise;
        }

        public void OnExit()
        {
            if (_isPlanning)
            {
                CancelPlacement();
            }
            
            _rotateClockwiseInput.action.performed -= OnRotateClockwise;
            _rotateCounterClockwiseInput.action.performed -= OnRotateCounterClockwise;
        }
    }
}
