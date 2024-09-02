using System;
using Characters;
using Managers;
using Sirenix.OdinInspector;
using Systems.Input_Management;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.U2D.Animation;

namespace Systems.Appearance.Scripts
{
    public class Avatar : MonoBehaviour
    {
        public SpriteRenderer Appearance;
        public Animator Animator;
        public AudioSource AudioSource;
        public NavMeshAgent Agent;
        public SpriteLibrary SpriteLibrary;
        public Kinling Kinling;
        
        private bool _isFlipped;

        private static readonly int Velocity = Animator.StringToHash("Velocity");
        private const string DOING = "IsDoing";
        private const string DIG = "IsDigging";
        private const string WATER = "IsWatering";
        private const string SWING = "IsSwinging";
        private const string SLEEP = "IsSleeping";
        private const string SIT = "IsSitting";
        
        private void Awake()
        {
            GameEvents.OnGameSpeedChanged += OnSpeedUpdated;
        }

        private void OnDestroy()
        {
            GameEvents.OnGameSpeedChanged -= OnSpeedUpdated;
        }

        private void OnSpeedUpdated(float speedMod)
        {
            Animator.speed = speedMod;
        }
        
        private void Update()
        {
            RefreshAnimVector();
        }

        private void RefreshAnimVector()
        {
            var moveVelo = Agent.velocity;
            SetMovementVelocity(moveVelo);
        }

        public AvatarLayer.EAppearanceDirection GetDirection()
        {
            return Kinling.RuntimeData.Avatar.CurrentDirection;
        }
        
        [Button("Set Direction")]
        public void SetDirection(AvatarLayer.EAppearanceDirection direction)
        {
            Kinling.RuntimeData.Avatar.CurrentDirection = direction;

            switch (direction)
            {
                case AvatarLayer.EAppearanceDirection.Down:
                    SpriteLibrary.spriteLibraryAsset = Kinling.RuntimeData.Avatar.DownSpriteLibraryAsset;
                    break;
                case AvatarLayer.EAppearanceDirection.Up:
                    SpriteLibrary.spriteLibraryAsset = Kinling.RuntimeData.Avatar.UpSpriteLibraryAsset;
                    break;
                case AvatarLayer.EAppearanceDirection.Right:
                    SpriteLibrary.spriteLibraryAsset = Kinling.RuntimeData.Avatar.SideSpriteLibraryAsset;
                    Appearance.flipX = false;
                    break;
                case AvatarLayer.EAppearanceDirection.Left:
                    SpriteLibrary.spriteLibraryAsset = Kinling.RuntimeData.Avatar.SideSpriteLibraryAsset;
                    Appearance.flipX = true;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(direction), direction, null);
            }
        }

        public void RefreshAppearanceLibrary()
        {
            SetDirection(Kinling.RuntimeData.Avatar.CurrentDirection);
        }
        
        private AvatarLayer.EAppearanceDirection ConvertPlacementDirection(PlacementDirection placementDirection)
        {
            switch (placementDirection)
            {
                case PlacementDirection.South:
                    return AvatarLayer.EAppearanceDirection.Down;
                case PlacementDirection.North:
                    return AvatarLayer.EAppearanceDirection.Up;
                case PlacementDirection.West:
                    return AvatarLayer.EAppearanceDirection.Left;
                case PlacementDirection.East:
                    return AvatarLayer.EAppearanceDirection.Right;
                default:
                    throw new ArgumentOutOfRangeException(nameof(placementDirection), placementDirection, null);
            }
        }
        
        public void SetUnitAction(UnitAction unitAction, PlacementDirection direction)
        {
            SetUnitAction(unitAction, ConvertPlacementDirection(direction));
        }

        public void SetUnitAction(UnitAction unitAction, AvatarLayer.EAppearanceDirection? direction = null)
        {
            ClearAllActions();
            
            if (direction != null)
            {
                SetDirection((AvatarLayer.EAppearanceDirection)direction);
            }
            
            switch (unitAction)
            {
                case UnitAction.Nothing:
                    ClearAllActions();
                    break;
                case UnitAction.Doing:
                    SetAnimation(DOING);
                    break;
                case UnitAction.Digging:
                    SetAnimation(DIG);
                    break;
                case UnitAction.Watering:
                    SetAnimation(WATER);
                    break;
                case UnitAction.Swinging:
                    SetAnimation(SWING);
                    break;
                case UnitAction.Sleeping:
                    SetAnimation(SLEEP);
                    break;
                default:
                    ClearAllActions();
                    throw new ArgumentOutOfRangeException(nameof(unitAction), unitAction, null);
            }
        }

        private void SetAnimation(string parameter, bool isActive = true)
        {
            Animator.SetBool(parameter, isActive);
        }
        
        private void ClearAllActions()
        {
            SetAnimation(DOING, false);
            SetAnimation(DIG, false);
            SetAnimation(WATER, false);
            SetAnimation(SWING, false);
            SetAnimation(SLEEP, false);
        }

        public void SetMovementVelocity(Vector2 velocityVector)
        {
            if (TimeManager.Instance.GameSpeed == GameSpeed.Paused) return;

            Animator.SetFloat(Velocity, velocityVector.magnitude);
            
            if (velocityVector != Vector2.zero)
            {
                if (velocityVector.x is <= 0.5f and > 0 || velocityVector.x is >= -0.5f and < 0)
                {
                    if (velocityVector.y > 0) // Up
                    {
                        SetDirection(AvatarLayer.EAppearanceDirection.Up);
                    }
                    else if (velocityVector.y < 0) // Down
                    {
                        SetDirection(AvatarLayer.EAppearanceDirection.Down);
                    }
                }
                else
                {
                    if (velocityVector.x > 0) // Right
                    {
                        SetDirection(AvatarLayer.EAppearanceDirection.Right);
                    } 
                    else if (velocityVector.x < 0) // Left
                    {
                        SetDirection(AvatarLayer.EAppearanceDirection.Left);
                    } 
                }
            }
        }

        public void SetEyesClosed(bool isClosed)
        {
            // TODO: build me
        }
    }
    
    public enum UnitAction
    {
        Nothing,
        Doing,
        Digging,
        Watering,
        Swinging,
        Sleeping,
        Sitting,
    }
}
