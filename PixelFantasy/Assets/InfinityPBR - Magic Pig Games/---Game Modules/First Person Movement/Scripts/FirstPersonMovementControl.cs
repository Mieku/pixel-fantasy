using UnityEngine;
using UnityEngine.Serialization;

namespace InfinityPBR.Modules
{
    
    /*
     * IMPORTANT!!
     *
     * FirstPersonMovementControl may be what you need, but perhaps you would like to customize things,
     * such as adding a "double jump" or other behaviour and features. If so, you can create your own
     * movement controller that inherits from this one, and then override only the methods you need to
     * change, or create your own from scratch.
     */

    
    [System.Serializable]
    public struct FirstPersonMovementControlSaveData
    {
        public bool canMove;
        public bool canTilt;
        public bool canPan;
        public bool canJump;
        public bool canFly;
        public Dictionaries dictionaries;
        public float _currentSpeed;
        public bool _isJumping;
        public Vector3 _jumpDirection;
        public bool _jumpKeyReleased;
        public float _verticalSpeed;
        public bool _flightMode;
        public bool floatWhenFlying;
        public float floatAmplitude;
        public float floatFrequency;
    }
    
    public class FirstPersonMovementControl : MonoBehaviour, ISaveable
    {
        [Header("Required Objects")]
        public FirstPersonMovement firstPersonMovement;
        public CharacterController moveController;
        public Transform panTransform;
        public Transform tiltTransform;
        public Camera camera;
        
        [Header("Pass These Values In From Your Input Script")]
        // These are the values that need to be passed into this script from your custom control portion!
        public float moveForward;
        public float moveRight;
        public float lookUp;
        public float turnRight;
        public float flyUp;
        public float flyDown;
        public float gravityMultiplier = 2.0f;
        public bool jumpButton = false;
        public bool jumpButtonUp = false;
        public bool lockPan = false;
        public bool lockTilt = false;
        public bool lockFlyingLookAtMove = false;
        public float moveSpeedModifier = 0f;

        [Header("Control Options")]
        // Handles whether we can move around and look around
        public bool canMove = true;
        public bool canTilt = true;
        public bool canPan = true;
        public bool canJump = true;
        public bool canFly = true;
        
        [Header("Flight Options")]
        public bool floatWhenFlying = true;
        public float floatAmplitude = 0.25f;
        public float floatFrequency = 1f;

        // Dictionaries Module is required for these
        [Header("Custom Dictionary")]
        public Dictionaries dictionaries;

        public float Height => moveController.transform.position.y;
        public bool IsJumping => _isJumping;
        public bool InFlightMode => _flightMode;
        
        // Private variables
        private float _currentSpeed;
        private bool _isJumping;
        private Vector3 _jumpDirection;
        private bool _jumpKeyReleased = true;
        private float _verticalSpeed = 0.0f;  // This represents the vertical speed (jumping up or falling down)
        private bool _flightMode = false;

        protected internal bool FastMovementPressed { get; set; }

        private float FloatEffect
        {
            get
            {
                var normalizedWave = Mathf.Sin(floatFrequency * Time.time);
                var value2 = 0.5f * Mathf.Sin(1.5f * floatFrequency * Time.time + 0.5f);
                return (floatAmplitude * ((normalizedWave + value2) / 2)) * floatFrequency;
            }
        }

        protected virtual void OnEnable()
        {
            if (!HasRequiredObjects())
            {
                Debug.LogError("<size=14><color=red>Warning: Required objects are not assigned.</color></size>");
                return;
            }

            canPan = firstPersonMovement.canPan;
            canTilt = firstPersonMovement.canTilt;
            canMove = firstPersonMovement.canMove;
            canJump = firstPersonMovement.canJump;
            canFly = firstPersonMovement.canFly;
        }

        protected virtual void Start()
        {
            if (!HasRequiredObjects())
                return;
            
            SetStartingValues();
            
        }

        protected virtual void SetStartingValues()
        {
            
        }

        protected virtual bool HasRequiredObjects() => firstPersonMovement && moveController && tiltTransform && panTransform;

        protected virtual void Update()
        {
            if (!HasRequiredObjects())
                return;
            
            /* Note: The characterController, for some reason, was moving the player at start,
             * so we turn it off by default, and have Update() turn it on.
             */
            if (!moveController.enabled)
                moveController.enabled = true;
            
            HandleSpeed();
            Jump();
        }
        protected virtual void HandleSpeed()
        {
            // Compute speed based on default run (^ is the "XOR operator" -- returns true only if one value is true, otherwise false)
            var targetSpeed = FastMovementPressed ^ firstPersonMovement.defaultRun 
                ? firstPersonMovement.runSpeed 
                : firstPersonMovement.walkSpeed;

            targetSpeed *= 1 + moveSpeedModifier;

            // Modify speed if the player is flying
            if (_flightMode)
                targetSpeed *= firstPersonMovement.flightSpeedModifier;

            // Transition smoothly only if transitionSpeed is true and duration is not 0
            _currentSpeed = firstPersonMovement.transitionSpeed && firstPersonMovement.transitionDuration > 0f
                ? Mathf.Lerp(_currentSpeed, targetSpeed, Time.deltaTime / firstPersonMovement.transitionDuration)
                : targetSpeed;
        }

        protected virtual void LateUpdate()
        {
            if (!HasRequiredObjects())
                return;

            TryTilt();
            TryPan();
            TryMove();

            jumpButtonUp = false;
        }
        
        protected virtual bool CanJump() => canJump && IsGrounded();
        
        public virtual void Jump()
        {
            if (jumpButtonUp)
                _jumpKeyReleased = true;
            
            if (!jumpButton || !CanJump())
                return;

            // Don't let us jump repeatedly if jump key has not been released
            if (!firstPersonMovement.jumpRepeatedly && !_jumpKeyReleased)
                return;
    
            // convert the jumpForce to an upward speed
            _verticalSpeed = firstPersonMovement.jumpForce;
            _isJumping = true;
            _jumpKeyReleased = false;
        }

        private Vector3 _lastGroundedMoveDirection = Vector3.zero;

        protected virtual void TryMove()
        {
            if (!canMove)
                return;

            var moveDirection = MoveDirection();

            if (canFly && (_flightMode || flyUp > 0))
            {
                _verticalSpeed = 0;
                _flightMode = IsFlying(ref moveDirection);
            }

            if (!_flightMode || !canFly)
                FallToEarth(ref moveDirection);
            else if (floatWhenFlying && flyDown == 0 && flyUp == 0)
            {
                moveDirection.y += FloatEffect;
            }

            moveController.Move(moveDirection * Time.deltaTime);
        }


        protected virtual Vector3 MoveDirection()
        {
            var moveDirection = Vector3.zero;

            var intendedDirection = Vector3.zero;
            intendedDirection.x = moveRight;
            intendedDirection.z = moveForward;

            if (firstPersonMovement.moveToLookDirection && !lockFlyingLookAtMove)
            {
                // Move in the direction the camera is facing
                intendedDirection = camera.transform.rotation * intendedDirection;
            }
            else
            {
                // Move in the player's forward and rightward direction
                intendedDirection = transform.TransformDirection(intendedDirection);
            }

            if (IsGrounded() || _flightMode)
            {
                moveDirection = intendedDirection * _currentSpeed;

                // Preserve this grounded move direction
                _lastGroundedMoveDirection = moveDirection;
            }
            else if (firstPersonMovement.changeDirectionMidJump)
            {
                moveDirection = intendedDirection * _currentSpeed;
            }
            else
            {
                // The object is in air and changeDirectionInAir is false.
                // Continue the movement in last grounded direction
                moveDirection = _lastGroundedMoveDirection;
            }
            
            return moveDirection;
        }

        protected virtual void FallToEarth(ref Vector3 moveDirection)
        {
            // Decrease the vertical speed by the force of gravity times delta time.
            float gravityMultiplierToApply = gravityMultiplier;
            if (_jumpKeyReleased && firstPersonMovement.shortJumpIfButtonReleased)
                gravityMultiplierToApply *= 2.0f;

            _verticalSpeed += Physics.gravity.y * gravityMultiplierToApply * Time.deltaTime;

            // Add the vertical speed due to gravity or any previous jump to the move direction
            moveDirection.y += _verticalSpeed;

            // Reset vertical speed to zero when on the ground and not jumping
            if (!_isJumping && IsGrounded())
            {
                _verticalSpeed = 0.0f;
            }

            // Check if we're on the ground using our custom IsGrounded function
            if (IsGrounded())
            {
                _isJumping = false;

                // If the jump key is not pressed and we're on the ground, the jump key was released after the jump.
                if (!jumpButton)
                    jumpButtonUp = true;
            }
        }

        protected virtual bool IsFlying(ref Vector3 moveDirection)
        {
            if (!canFly)
                return false;

            // Handle the flight conditions
            if (flyUp > 0)
            {
                moveDirection.y += firstPersonMovement.verticalSpeed * Time.deltaTime * flyUp;
                _flightMode = true;  // Set flight mode to true
                return true;
            }
            else if (!IsGrounded() && DistanceToGround() < firstPersonMovement.minHeight)
            {
                // If we are in the air and lower than minHeight from the ground, we should fall
                Debug.Log($"Should fall because distance to ground is {DistanceToGround()}");
                return false; // Return false because we're not actively flying, we're falling
            }
            else if (flyDown > 0)
            {
                moveDirection.y -= firstPersonMovement.verticalSpeed * Time.deltaTime * flyDown;
                return true; // Return true here because we're actively flying downwards
            }
            else if (flyUp <= 0 && DistanceToGround() < firstPersonMovement.minHeight)
            {
                // Exit flight mode if not trying to fly up and close to the ground
                return false;
            }

            // Maintain the current flight mode by default
            return true;
        }


        public virtual float DistanceToGround()
        {
            RaycastHit hit;
            // Offset the raycast origin to the bottom of the CharacterController
            Vector3 rayOrigin = transform.position;
            rayOrigin.y -= moveController.height / 2f;
            if (Physics.Raycast(rayOrigin, Vector3.down, out hit, Mathf.Infinity))
            {
                return hit.distance;
            }
            else
            {
                // Arbitrary high value if nothing was hit by the raycast
                return Mathf.Infinity;
            }
        }


        public virtual bool IsGrounded() {
            float sphereRadius = moveController.radius;
            float distanceToGround = moveController.bounds.extents.y - sphereRadius;
            RaycastHit hit;

            if (Physics.SphereCast(transform.position, sphereRadius, -Vector3.up, out hit, distanceToGround + 0.1f)) {
                // Calculate the angle of the slope
                float groundAngle = Vector3.Angle(Vector3.up, hit.normal);

                // Check if the slope angle is less than the CharacterController's slope limit
                return groundAngle <= moveController.slopeLimit;
            }

            return false;
        }

        
        protected virtual void TryPan()
        {
            if (!canPan || lockPan)
                return;
            
            Vector3 newAngles = panTransform.localEulerAngles;
            newAngles.y += PanAmount();
            panTransform.eulerAngles = newAngles;
        }
        
        protected virtual float PanAmount() => turnRight * firstPersonMovement.speedPan * Time.deltaTime;

        protected virtual void TryTilt()
        {
            if (!canTilt || lockTilt)
                return;

            if (!firstPersonMovement.invertTilt)
                lookUp *= -1;
            
            var tiltAngle = tiltTransform.eulerAngles.x;
            tiltAngle = tiltAngle > 180 ? tiltAngle - 360 : tiltAngle;
            tiltAngle += lookUp * firstPersonMovement.speedTilt * Time.deltaTime;
            tiltAngle = Mathf.Clamp(tiltAngle, firstPersonMovement.minTilt, firstPersonMovement.maxTilt);
            var eulerAngles = tiltTransform.eulerAngles;
            var centeredRotation = Quaternion.Euler(tiltAngle, eulerAngles.y, eulerAngles.z);
            tiltTransform.rotation = centeredRotation;
        }

        public object SaveState()
        {
            var data = new FirstPersonMovementControlSaveData
            {
                canMove = this.canMove,
                canTilt = this.canTilt,
                canPan = this.canPan,
                canJump = this.canJump,
                canFly = this.canFly,
                dictionaries = this.dictionaries,
                _currentSpeed = this._currentSpeed,
                _isJumping = this._isJumping,
                _jumpDirection = this._jumpDirection,
                _jumpKeyReleased = this._jumpKeyReleased,
                _verticalSpeed = this._verticalSpeed,
                _flightMode = this._flightMode,
                floatWhenFlying = floatWhenFlying,
                floatAmplitude = floatAmplitude,
                floatFrequency = floatFrequency
            };
    
            return data;
        }

        public void LoadState(string jsonEncodedState)
        {
            // Decode the JSON data into a FirstPersonMovementControlSaveData object
            var data = JsonUtility.FromJson<FirstPersonMovementControlSaveData>(jsonEncodedState);
            
            // Set the values from data to the current object instance
            this.canMove = data.canMove;
            this.canTilt = data.canTilt;
            this.canPan = data.canPan;
            this.canJump = data.canJump;
            this.canFly = data.canFly;
            this.dictionaries = data.dictionaries;
            this._currentSpeed = data._currentSpeed;
            this._isJumping = data._isJumping;
            this._jumpDirection = data._jumpDirection;
            this._jumpKeyReleased = data._jumpKeyReleased;
            this._verticalSpeed = data._verticalSpeed;
            this._flightMode = data._flightMode;
            floatWhenFlying = data.floatWhenFlying;
            floatAmplitude = data.floatAmplitude;
            floatFrequency = data.floatFrequency;

            // Add any additional logic post-loading if needed
        }


        [SerializeField] private string _saveableObjectId = "First Person Movement Control";
        public string SaveableObjectId() => _saveableObjectId;
        
        public void PreSaveActions()
        {
            throw new System.NotImplementedException();
        }

        public void PostSaveActions()
        {
            throw new System.NotImplementedException();
        }

        public void PreLoadActions()
        {
            throw new System.NotImplementedException();
        }

        public void PostLoadActions()
        {
            throw new System.NotImplementedException();
        }
    }
}
