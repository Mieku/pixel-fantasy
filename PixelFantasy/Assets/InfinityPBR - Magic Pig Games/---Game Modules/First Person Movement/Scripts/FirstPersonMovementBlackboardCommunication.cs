using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static InfinityPBR.Modules.MainBlackboard;

namespace InfinityPBR.Modules
{
    /*
     * IMPORTANT!!
     *
     * This script does basic communication to the blackboard, and is focused on the default
     * FirstPersonMovementControl script. If you create your own FirstPersonMovementControl,
     * then you may need to update this.
     *
     * You may also want to update this (or make your own version of it that inherits from it),
     * if you want additional or custom functionality.
     */
    
    public class FirstPersonMovementBlackboardCommunication : MonoBehaviour
    {
        [Header("Post Options [These are also the note Subject strings]")] 
        public bool height = false; // Global height of the player
        public bool distanceToGround = true; // Distance to the ground
        public bool movementSpeed = true; // Forward movement speed
        public bool verticalDelta = true; // Vertical delta (up/down movement)
        public bool flyingStateChange = true; // When flying turns true or false
        public bool groundedStateChange = true; // When grounded turns true or false
        public bool jumpingStateChange = true; // When grounded turns true or false
        
        [Header("Notification Options")]
        public bool notifyHeight = false;
        public bool notifyDistanceToGround = false;
        public bool notifyMovementSpeed = false;
        public bool notifyVerticalDelta = false;
        public bool notifyFlyingStateChange = true;
        public bool notifyGroundedStateChange = true;
        public bool notifyJumpingStateChange = true;

        [Header("Options")] 
        public string noteTopic = "First Person Movement";
        
        private FirstPersonMovementControl _firstPersonMovementControl;
        private bool _inFlightMode = false;
        private bool _isGrounded = false;
        private bool _isJumping = false;
        private float _lastHeight = 0;
        private float _lastDelta = 0;
        private float _currentHeight = 0;
        private bool _firstPost = true;
        
        protected virtual void Awake()
        {
            _firstPersonMovementControl = GetComponent<FirstPersonMovementControl>();
        }

        protected virtual void Update()
        {
            if (blackboard == null) return;

            if (height || verticalDelta)
                _currentHeight = _firstPersonMovementControl.Height;
                
            PostHeight();
            PostDistanceToGround();
            PostMovementSpeed();
            PostVerticalDelta();
            PostFlyingStateChange();
            PostGroundedStateChange();

            _firstPost = false;
        }

        protected virtual void PostHeight()
        {
            if (!height) return;
            
            blackboard.UpdateNote(noteTopic
                , "Height"
                , _currentHeight
                , true
                , notifyHeight);
        }

        protected virtual void PostDistanceToGround()
        {
            if (!distanceToGround) return;
            // Skip the first frame so we don't have a huge delta
            if (_firstPost)
                return;
            
            blackboard.UpdateNote(noteTopic
                , "Distance To Ground"
                , _firstPersonMovementControl.DistanceToGround()
                , true
                , notifyDistanceToGround);
        }

        protected virtual void PostMovementSpeed()
        {
            if (!movementSpeed) return;
            
            blackboard.UpdateNote(noteTopic
                , "Movement Speed"
                , _firstPersonMovementControl.moveController.velocity.magnitude
                , true
                , notifyMovementSpeed);
        }

        protected virtual void PostVerticalDelta()
        {
            if (!verticalDelta) return;
            
            // If it's the first post, save the height and return so we don't have a huge delta
            if (_firstPost)
            {
                _lastHeight = _firstPersonMovementControl.Height;
                return;
            }

            var delta = _currentHeight - _lastHeight;
            if (delta == _lastDelta)
                return;
            
            blackboard.UpdateNote(noteTopic
                , "Vertical Delta"
                , _currentHeight - _lastHeight
                , true
                , notifyVerticalDelta);
            
            _lastHeight = _firstPersonMovementControl.Height;
            _lastDelta = delta;
        }

        protected virtual void PostFlyingStateChange()
        {
            if (!flyingStateChange) return;

            var inFlightMode = _firstPersonMovementControl.InFlightMode;
            var changed = _inFlightMode != inFlightMode || _firstPost;
            _inFlightMode = inFlightMode;

            if (!changed)
                return;
            
            blackboard.UpdateNote(noteTopic
                , "Flying State Change"
                , inFlightMode
                , true
                , notifyFlyingStateChange);
        }

        protected virtual void PostGroundedStateChange()
        {
            if (!groundedStateChange) return;
            
            var isGrounded = _firstPersonMovementControl.IsGrounded();
            var changed = _isGrounded != isGrounded || _firstPost;
            _isGrounded = isGrounded;

            if (!changed)
                return;
            
            blackboard.UpdateNote(noteTopic
                , "Grounded State Change"
                , isGrounded
                , true
                , notifyGroundedStateChange);
        }
        
        protected virtual void PostJumpingStateChange()
        {
            if (!jumpingStateChange) return;
            
            var isJumping = _firstPersonMovementControl.IsJumping;
            var changed = _isJumping != isJumping || _firstPost;
            _isJumping = isJumping;

            if (!changed)
                return;
            
            blackboard.UpdateNote(noteTopic
                , "Jumping State Change"
                , isJumping
                , true
                , notifyJumpingStateChange);
        }
    }

}
