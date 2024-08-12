using System;
using Managers;
using ScriptableObjects;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Controllers
{
    [Serializable]
    public class CameraData
    {
        public float PosX;
        public float PosY;
        public float OrthographicSize;
    }
    
    public class CameraManager : Singleton<CameraManager>
    {
        [SerializeField] private CinemachineCamera _cam;
        [SerializeField] private float _moveSpeed;
        [SerializeField] private float _smoothTime = 0.1F;
        [SerializeField] private float _minFov;
        [SerializeField] private float _maxFov;
        [SerializeField] private float _scrollSensitivity;

        [SerializeField] private InputActionReference _moveCamUpInput;
        [SerializeField] private InputActionReference _moveCamDownInput;
        [SerializeField] private InputActionReference _moveCamLeftInput;
        [SerializeField] private InputActionReference _moveCamRightInput;
        [SerializeField] private InputActionReference _zoomCameraInput;

        private Vector2 _moveInput;
       // private PlayerInput _playerInput;

        public bool IgnoreKeyboardInput { get; set; }

        private Vector3 _velocity = Vector3.zero;
        //private CinemachineCamera _cam;
        
        private Vector3 _targetPosition;
        private bool _isMovingToTarget;
        private const int PPU = 16;

        private void OnEnable()
        {
            // Subscribe to the input actions
            _moveCamUpInput.action.performed += OnMoveUp;
            _moveCamUpInput.action.canceled += OnMoveUp;
            _moveCamDownInput.action.performed += OnMoveDown;
            _moveCamDownInput.action.canceled += OnMoveDown;
            _moveCamLeftInput.action.performed += OnMoveLeft;
            _moveCamLeftInput.action.canceled += OnMoveLeft;
            _moveCamRightInput.action.performed += OnMoveRight;
            _moveCamRightInput.action.canceled += OnMoveRight;
            _zoomCameraInput.action.performed += OnZoomCamera;

            // Enable actions
            _moveCamUpInput.action.Enable();
            _moveCamDownInput.action.Enable();
            _moveCamLeftInput.action.Enable();
            _moveCamRightInput.action.Enable();
            _zoomCameraInput.action.Enable();
        }

        private void OnDisable()
        {
            // Unsubscribe from the input actions
            _moveCamUpInput.action.performed -= OnMoveUp;
            _moveCamUpInput.action.canceled -= OnMoveUp;
            _moveCamDownInput.action.performed -= OnMoveDown;
            _moveCamDownInput.action.canceled -= OnMoveDown;
            _moveCamLeftInput.action.performed -= OnMoveLeft;
            _moveCamLeftInput.action.canceled -= OnMoveLeft;
            _moveCamRightInput.action.performed -= OnMoveRight;
            _moveCamRightInput.action.canceled -= OnMoveRight;
            _zoomCameraInput.action.performed -= OnZoomCamera;

            // Disable actions
            _moveCamUpInput.action.Disable();
            _moveCamDownInput.action.Disable();
            _moveCamLeftInput.action.Disable();
            _moveCamRightInput.action.Disable();
            _zoomCameraInput.action.Disable();
        }

        public CameraData SaveCameraData()
        {
            CameraData data = new CameraData
            {
                OrthographicSize = _cam.Lens.OrthographicSize,
                PosX = _cam.transform.position.x,
                PosY = _cam.transform.position.y
            };

            return data;
        }

        public void LoadCameraData(CameraData data)
        {
            _cam.transform.position = new Vector3(data.PosX, data.PosY, _cam.transform.position.z);
            _cam.Lens.OrthographicSize = data.OrthographicSize;
        }

        private void FixedUpdate()
        {
            CameraPanningInput();
            
            if (_isMovingToTarget)
            {
                // Smoothly move the camera if it is supposed to move
                _cam.transform.position = Vector3.SmoothDamp(_cam.transform.position, _targetPosition, ref _velocity, _smoothTime);
                
                if (Vector3.Distance(_cam.transform.position, _targetPosition) < 0.01f)
                {
                    _isMovingToTarget = false; // Stop moving once close enough
                }
            }
        }

        private void LateUpdate()
        {
            // Ensures the camera adheres to the PPU of the game
            var position = _cam.transform.position;
            position = new Vector3(Mathf.Round(position.x * PPU) / PPU, Mathf.Round(position.y * PPU) / PPU, position.z);
            _cam.transform.position = position;
        }

        private void CameraPanningInput()
        {
            if (IgnoreKeyboardInput) return;
            
            // Calculate target position based on input
            Vector3 targetPos = (Vector2)_cam.transform.position + _moveInput * _moveSpeed;
            targetPos.z = -15;
            
            if (_moveInput != Vector2.zero)
            {
                _isMovingToTarget = false; // Cancel automatic movement if manually panning
                _cam.transform.position = Vector3.SmoothDamp(_cam.transform.position, targetPos, ref _velocity, _smoothTime);
            }
        }

        private void OnMoveUp(InputAction.CallbackContext context)
        {
            if (context.performed)
            {
                _moveInput += Vector2.up;
            }
            else if (context.canceled)
            {
                _moveInput -= Vector2.up;
            }
        }

        private void OnMoveDown(InputAction.CallbackContext context)
        {
            if (context.performed)
            {
                _moveInput += Vector2.down;
            }
            else if (context.canceled)
            {
                _moveInput -= Vector2.down;
            }
        }

        private void OnMoveLeft(InputAction.CallbackContext context)
        {
            if (context.performed)
            {
                _moveInput += Vector2.left;
            }
            else if (context.canceled)
            {
                _moveInput -= Vector2.left;
            }
        }

        private void OnMoveRight(InputAction.CallbackContext context)
        {
            if (context.performed)
            {
                _moveInput += Vector2.right;
            }
            else if (context.canceled)
            {
                _moveInput -= Vector2.right;
            }
        }

        public void OnZoomCamera(InputAction.CallbackContext context)
        {
            if (IgnoreKeyboardInput) return;
            
            var scroll = context.ReadValue<Vector2>().y;
            float dynamicZoomSpeed = _scrollSensitivity * _cam.Lens.OrthographicSize;
            float newSize = _cam.Lens.OrthographicSize - scroll * dynamicZoomSpeed;
            _cam.Lens.OrthographicSize = Mathf.Clamp(newSize, _minFov, _maxFov);
        }

        public void LookAtPosition(Vector2 pos)
        {
            _targetPosition = new Vector3(pos.x, pos.y, _cam.transform.position.z); // Set target, keep current Z
            _isMovingToTarget = true;
        }
    }
}
