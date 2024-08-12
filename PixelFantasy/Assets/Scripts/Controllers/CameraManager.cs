using System;
using Managers;
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
        [SerializeField] private float _moveSpeed;
        [SerializeField] private float _smoothTime = 0.1F;
        [SerializeField] private float _minFov;
        [SerializeField] private float _maxFov;
        [SerializeField] private float _scrollSensitivity;

        private Vector2 _moveInput;

        public bool IgnoreKeyboardInput { get; set; }

        private Vector3 _velocity = Vector3.zero;
        private CinemachineCamera _cam;
        
        private Vector3 _targetPosition;
        private bool _isMovingToTarget;
        private const int PPU = 16;

        protected override void Awake()
        {
            base.Awake();
            _cam = GetComponent<CinemachineCamera>();
        }
        
        public CameraData SaveCameraData()
        {
            CameraData data = new CameraData
            {
                OrthographicSize = _cam.Lens.OrthographicSize,
                PosX = transform.position.x,
                PosY = transform.position.y
            };

            return data;
        }

        public void LoadCameraData(CameraData data)
        {
            transform.position = new Vector3(data.PosX, data.PosY, transform.position.z);
            _cam.Lens.OrthographicSize = data.OrthographicSize;
        }

        private void FixedUpdate()
        {
            CameraPanningInput();
            
            if (_isMovingToTarget)
            {
                // Smoothly move the camera if it is supposed to move
                transform.position = Vector3.SmoothDamp(transform.position, _targetPosition, ref _velocity, _smoothTime);
                
                if (Vector3.Distance(transform.position, _targetPosition) < 0.01f)
                {
                    _isMovingToTarget = false; // Stop moving once close enough
                }
            }
        }

        private void LateUpdate()
        {
            // Ensures the camera adheres to the PPU of the game
            var position = transform.position;
            position = new Vector3(Mathf.Round(position.x * PPU) / PPU, Mathf.Round(position.y * PPU) / PPU, position.z);
            transform.position = position;
        }

        private void CameraPanningInput()
        {
            if (IgnoreKeyboardInput) return;
            
            // Calculate target position based on input
            Vector3 targetPos = (Vector2)transform.position + _moveInput * _moveSpeed;
            targetPos.z = -15;
            
            if (_moveInput != Vector2.zero)
            {
                _isMovingToTarget = false; // Cancel automatic movement if manually panning
                transform.position = Vector3.SmoothDamp(transform.position, targetPos, ref _velocity, _smoothTime);
            }
        }

        public void MoveCameraUp(InputAction.CallbackContext context)
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

        public void MoveCameraDown(InputAction.CallbackContext context)
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

        public void MoveCameraLeft(InputAction.CallbackContext context)
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

        public void MoveCameraRight(InputAction.CallbackContext context)
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

        public void OnZoomCamera(InputValue value)
        {
            if (IgnoreKeyboardInput) return;
            
            var scroll = value.Get<Vector2>().y;
            float dynamicZoomSpeed = _scrollSensitivity * _cam.Lens.OrthographicSize;
            float newSize = _cam.Lens.OrthographicSize - scroll * dynamicZoomSpeed;
            _cam.Lens.OrthographicSize = Mathf.Clamp(newSize, _minFov, _maxFov);
        }

        public void LookAtPosition(Vector2 pos)
        {
            _targetPosition = new Vector3(pos.x, pos.y, transform.position.z); // Set target, keep current Z
            _isMovingToTarget = true;
        }
    }
}
