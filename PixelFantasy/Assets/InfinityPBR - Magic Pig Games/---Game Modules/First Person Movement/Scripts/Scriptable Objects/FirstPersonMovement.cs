using System;
using UnityEngine;
using UnityEngine.Serialization;

namespace InfinityPBR.Modules
{
    [HelpURL("https://infinitypbr.gitbook.io/infinity-pbr/game-modules-systems/first-person-movement")]
    [Serializable]
    [CreateAssetMenu(fileName = "FirstPersonMovement", menuName = "Game Modules/Create/First Person Movement", order = 1)]
    public class FirstPersonMovement : ModulesScriptableObject
    {
        // Global Enablement
        public bool canPan = true;
        public bool canTilt = true;
        public bool canMove = true;
        public bool canJump = true;
        public bool canFly = true;
        
        // Normal Min/Max values (Note special min/max values may exist elsewhere)
        public float minTilt = -80f;
        public float maxTilt = 80f;
        public bool invertTilt = true;

        // Speed
        public bool defaultRun = true;
        public float speedTilt = 360f;
        public float speedPan = 360f;
        public float walkSpeed = 2f;
        public float runSpeed = 4f;
        public bool transitionSpeed = true;
        public float transitionDuration = 0.3f; // Duration of the transition from walking to running and vice versa
        
        // Jump
        public float jumpForce = 10f; // Adjust this value as needed
        public bool shortJumpIfButtonReleased = true;
        public bool jumpRepeatedly = false;
        public bool changeDirectionMidJump = false; // By default, we don't allow direction change in mid-air
        
        // Flight
        public float flightSpeedModifier = 1.5f;
        public float verticalSpeed = 150f;
        public float minHeight = 1f;
        public bool moveToLookDirection = true;
        
        [HideInInspector] public int menubarIndex;
        
        public void Caching()
        {
            
        }
    }
}