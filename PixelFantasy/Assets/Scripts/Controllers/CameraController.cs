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

        private void Awake()
        {
            cam = GetComponent<Camera>();
        }

        void Update()
        {
            CameraPanningInput();
            CameraZoomInput();
        }

        private void CameraPanningInput()
        {
            if (IgnoreKeyboardInput) return;
            
            velocityVector = Vector3.zero;

            // Left
            if (Input.GetKey(KeyCode.A))
            {
                velocityVector += Vector3.left;
            }
            
            // Right
            if (Input.GetKey(KeyCode.D))
            {
                velocityVector += Vector3.right;
            }
            
            // Up
            if (Input.GetKey(KeyCode.W))
            {
                velocityVector += Vector3.up;
            }
            
            // Down
            if (Input.GetKey(KeyCode.S))
            {
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
    }
}
