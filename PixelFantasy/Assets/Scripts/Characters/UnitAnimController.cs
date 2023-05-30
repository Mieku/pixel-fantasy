using System;
using Characters.Interfaces;
using Managers;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Serialization;

namespace Characters
{
    public class UnitAnimController : MonoBehaviour, ICharacterAnimController
    {
        public UnitAppearance Appearance;
        
        [SerializeField] private Animator _anim;
        [SerializeField] private NavMeshAgent _navMeshAgent;

        private static readonly int Velocity = Animator.StringToHash("Velocity");
        private const string DOING = "isDoing";
        private const string DIG = "isDigging";
        private const string WATER = "isWatering";
        private const string SWING = "isSwinging";

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

                default:
                    throw new ArgumentOutOfRangeException(nameof(unitAction), unitAction, null);
            }
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
                default:
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
    }

    public enum UnitActionDirection
    {
        Side,
        Up,
        Down,
    }
