using UnityEngine;

namespace InfinityPBR.Modules
{
    /*
     * IMPORTANT!!
     * 
     * This is a very basic Player Input! Your needs may be different, so please create your own
     * PlayerInput class. You can inherit from this if you'd like, and then override only the methods
     * you need to change, or create your own from scratch.
     */
    
    public class PlayerInput : MonoBehaviour
    {
        [Header("Plumbing")]
        public FirstPersonMovementControl movementControl;
        
        [Header("Input Options")]
        public KeyCode jumpKey = KeyCode.Space;
        public KeyCode lockPan = KeyCode.LeftControl;
        public KeyCode lockTilt = KeyCode.LeftControl;
        public KeyCode lockFlyingLookAtMove = KeyCode.Z;
        public KeyCode flyUp = KeyCode.E;
        public KeyCode flyDown = KeyCode.Q;

        protected virtual void Awake()
        {
            if (!movementControl)
                Debug.LogError("<size=14><color=red>Warning: FirstPersonMovementControl component not found.</color></size>");
        }

        protected virtual void Update()
        {
            if (!movementControl)
                return;

            HandleMovement();
            HandleButtons();
        }

        protected virtual void HandleButtons()
        {
            movementControl.jumpButton = Input.GetKey(jumpKey);
            movementControl.jumpButtonUp = Input.GetKeyUp(jumpKey);
            
            movementControl.lockPan = Input.GetKey(lockPan);
            movementControl.lockTilt = Input.GetKey(lockTilt);
            movementControl.lockFlyingLookAtMove = Input.GetKey(lockFlyingLookAtMove);
        }

        protected virtual void HandleMovement()
        {
            movementControl.moveForward = Input.GetAxis("Vertical");
            movementControl.moveRight = Input.GetAxis("Horizontal");
            movementControl.turnRight = Input.GetAxis("Mouse X");
            movementControl.lookUp = Input.GetAxis("Mouse Y");
            movementControl.FastMovementPressed = FastMovement;
            movementControl.flyUp = Input.GetKey(flyUp) ? 1 : 0;
            movementControl.flyDown = Input.GetKey(flyDown) ? 1 : 0;
        }

        protected virtual bool FastMovement => Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
    }
}