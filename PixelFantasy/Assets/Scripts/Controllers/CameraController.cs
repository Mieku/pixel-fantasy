using System;
using UnityEngine;

namespace Controllers
{
    public class CameraController : MonoBehaviour
    {
        [SerializeField] private float moveSpeed;
        [SerializeField] private float minFov, maxFov, scrollSensitivity;
        
        public bool IgnoreKeyboardInput { get; set; }

        private Vector3 velocityVector;
        private Camera cam;
        
        private Vector3 targetPosition;
        private bool isMovingToTarget;

        private void Awake()
        {
            cam = GetComponent<Camera>();
        }

        void Update()
        {
            CameraPanningInput();
            CameraZoomInput();
            
            // Smoothly move the camera if it is supposed to move
            if (isMovingToTarget)
            {
                // Lerp position
                transform.position = Vector3.Lerp(transform.position, targetPosition, moveSpeed * Time.deltaTime);
            
                // Check if the camera is close enough to the target position
                if (Vector3.Distance(transform.position, targetPosition) < 0.01f)
                {
                    isMovingToTarget = false; // Stop moving once close enough
                }
            }
        }

        private void CameraPanningInput()
        {
            if (IgnoreKeyboardInput) return;
            
            velocityVector = Vector3.zero;

            // Left
            if (Input.GetKey(KeyCode.A))
            {
                isMovingToTarget = false; 
                velocityVector += Vector3.left;
            }
            
            // Right
            if (Input.GetKey(KeyCode.D))
            {
                isMovingToTarget = false; 
                velocityVector += Vector3.right;
            }
            
            // Up
            if (Input.GetKey(KeyCode.W))
            {
                isMovingToTarget = false; 
                velocityVector += Vector3.up;
            }
            
            // Down
            if (Input.GetKey(KeyCode.S))
            {
                isMovingToTarget = false; 
                velocityVector += Vector3.down;
            }
            
            transform.position += velocityVector * (moveSpeed * Time.deltaTime);
        }

        private void CameraZoomInput()
        {
            var fov = cam.orthographicSize;
            
            fov -= Input.mouseScrollDelta.y * scrollSensitivity;
            fov = Mathf.Clamp(fov, minFov, maxFov);
            cam.orthographicSize = fov;
        }

        public void LookAtPosition(Vector2 pos)
        {
            targetPosition = new Vector3(pos.x, pos.y, transform.position.z); // Set target, keep current Z
            isMovingToTarget = true;
        }
    }
}
