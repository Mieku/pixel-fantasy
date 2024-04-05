using System;
using Managers;
using Unity.Cinemachine;
using UnityEngine;

namespace Controllers
{
    public class CameraManager : Singleton<CameraManager>
    {
        [SerializeField] private float _moveSpeed;
        [SerializeField] private float _smoothTime = 0.1F;
        [SerializeField] private float _minFov;
        [SerializeField] private float _maxFov;
        [SerializeField] private float _scrollSensitivity;

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
        
        private void Update()
        {
            CameraPanningInput();
            CameraZoomInput();
            
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
            
            Vector3 desiredMove = Vector3.zero;

            // Input detection for panning
            if (Input.GetKey(KeyCode.A)) { desiredMove += Vector3.left; }
            if (Input.GetKey(KeyCode.D)) { desiredMove += Vector3.right; }
            if (Input.GetKey(KeyCode.W)) { desiredMove += Vector3.up; }
            if (Input.GetKey(KeyCode.S)) { desiredMove += Vector3.down; }
            
            // Calculate target position based on input
            Vector3 targetPos = transform.position + desiredMove * _moveSpeed;
            if (desiredMove != Vector3.zero)
            {
                _isMovingToTarget = false; // Cancel automatic movement if manually panning
                transform.position = Vector3.SmoothDamp(transform.position, targetPos, ref _velocity, _smoothTime);
            }
        }

        private void CameraZoomInput()
        {
            float scroll = Input.GetAxis("Mouse ScrollWheel");
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
