using System;
using Characters.Interfaces;
using Managers;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Serialization;

namespace Characters
{
    public class KinlingAnimController : MonoBehaviour, ICharacterAnimController
    {
        public KinlingAppearance Appearance;
        
        [SerializeField] private Animator _anim;
        [SerializeField] private NavMeshAgent _navMeshAgent;

        private static readonly int Velocity = Animator.StringToHash("Velocity");
        private const string DOING = "isDoing";
        private const string DIG = "isDigging";
        private const string WATER = "isWatering";
        private const string SWING = "isSwinging";
        private const string SLEEP = "isSleeping";
        private const string SIT = "isSitting";

        private const string UP = "isUp";
        private const string DOWN = "isDown";

        private void Awake()
        {
            GameEvents.OnGameSpeedChanged += OnSpeedUpdated;
        }

        private void OnDestroy()
        {
            GameEvents.OnGameSpeedChanged -= OnSpeedUpdated;
        }

        private void Start()
        {
            Appearance.SetDirection(UnitActionDirection.Side);
        }

        private void OnSpeedUpdated(float speedMod)
        {
            _anim.speed = speedMod;
        }

        public void SetEyesClosed(bool setClosed)
        {
            Appearance.SetEyesClosed(setClosed);
        }

        public void SetUnitAction(UnitAction unitAction)
        {
            switch (unitAction)
            {
                case UnitAction.Nothing:
                    ClearAllActions();
                    break;
                case UnitAction.Doing:
                    SetUnitAction(DOING);
                    break;
                case UnitAction.Digging:
                    SetUnitAction(DIG);
                    break;
                case UnitAction.Watering:
                    SetUnitAction(WATER);
                    break;
                case UnitAction.Swinging:
                    SetUnitAction(SWING);
                    break;
                case UnitAction.Sleeping:
                    SetUnitAction(SLEEP);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(unitAction), unitAction, null);
            }
        }

        private UnitActionDirection ConvertPlacementDirection(PlacementDirection placementDirection)
        {
            switch (placementDirection)
            {
                case PlacementDirection.South:
                    return UnitActionDirection.Down;
                case PlacementDirection.North:
                    return UnitActionDirection.Up;
                case PlacementDirection.West:
                    FlipRendererX(true);
                    return UnitActionDirection.Side;
                case PlacementDirection.East:
                    FlipRendererX(false);
                    return UnitActionDirection.Side;
                default:
                    throw new ArgumentOutOfRangeException(nameof(placementDirection), placementDirection, null);
            }
        }

        public void SetUnitAction(UnitAction unitAction, PlacementDirection direction)
        {
            // Flip the renderer
            var actionDir = ConvertPlacementDirection(direction);
            SetUnitAction(unitAction, actionDir);
        }

        public void SetUnitAction(UnitAction unitAction, UnitActionDirection direction)
        {
            ClearAllActions();

            Appearance.SetDirection(direction);

            switch (direction)
            {
                case UnitActionDirection.Side:
                    SetUnitAction(UP, false);
                    SetUnitAction(DOWN, false);
                    break;
                case UnitActionDirection.Up:
                    SetUnitAction(UP, true);
                    SetUnitAction(DOWN, false);
                    break;
                case UnitActionDirection.Down:
                    SetUnitAction(UP, false);
                    SetUnitAction(DOWN, true);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(direction), direction, null);
            }

            switch (unitAction)
            {
                case UnitAction.Nothing:
                    ClearAllActions();
                    break;
                case UnitAction.Doing:
                    SetUnitAction(DOING);
                    break;
                case UnitAction.Digging:
                    SetUnitAction(DIG);
                    break;
                case UnitAction.Watering:
                    SetUnitAction(WATER);
                    break;
                case UnitAction.Swinging:
                    SetUnitAction(SWING);
                    break;
                case UnitAction.Sleeping:
                    SetUnitAction(SLEEP);
                    break;
                default:
                    ClearAllActions();
                    throw new ArgumentOutOfRangeException(nameof(unitAction), unitAction, null);
            }
        }

        private void SetUnitAction(string parameter, bool isActive = true)
        {
            _anim.SetBool(parameter, isActive);
        }

        private void ClearAllActions()
        {
            SetUnitAction(DOING, false);
            SetUnitAction(DIG, false);
            SetUnitAction(WATER, false);
            SetUnitAction(SWING, false);
            SetUnitAction(SLEEP, false);
        }

        public void SetMovementVelocity(Vector2 velocityVector)
        {
            if (TimeManager.Instance.GameSpeed == GameSpeed.Paused) return;

            _anim.SetFloat(Velocity, velocityVector.magnitude);

            if (velocityVector != Vector2.zero)
            {
                FlipRendererX(velocityVector.x <= 0);
            }
        }

        /// <summary>
        /// Causes the unit to face towards the target position
        /// </summary>
        public void LookAtPostion(Vector2 targetPos)
        {
            FlipRendererX(targetPos.x < transform.position.x);
        }

        private void FlipRendererX(bool shouldFlip)
        {
            int scaleModX;
            if (shouldFlip)
            {
                scaleModX = -1;
            }
            else
            {
                scaleModX = 1;
            }

            transform.localScale = new Vector3(scaleModX, 1, 1);
        }

        private void Update()
        {
            RefreshAnimVector();
        }

        private void RefreshAnimVector()
        {
            var moveVelo = _navMeshAgent.velocity;
            SetMovementVelocity(moveVelo);
        }
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

    public enum UnitActionDirection
    {
        Side,
        Up,
        Down,
    }
