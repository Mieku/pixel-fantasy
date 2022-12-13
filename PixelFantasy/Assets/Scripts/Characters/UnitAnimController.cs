using System;
using Characters.Interfaces;
using Gods;
using ScriptableObjects;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Characters
{
    public class UnitAnimController : MonoBehaviour, ICharacterAnimController
    {
        [SerializeField] private HairData _hairData; // TODO: Separate this into its own class that has all the appearance data
        [SerializeField] private SpriteRenderer _hairRenderer;
        
        [SerializeField] private Animator _baseAnim;
        [SerializeField] private Animator _topAnim;
        [SerializeField] private Animator _bottomAnim;
        [SerializeField] private Animator _toolAnim;
        [SerializeField] private Animator _handsAnim;
        [SerializeField] private Animator _fxAnim;
        [SerializeField] private Animator _blushAnim;

        [SerializeField] private SpriteRenderer _baseRenderer;
        [SerializeField] private SpriteRenderer _topRenderer;
        [SerializeField] private SpriteRenderer _bottomRenderer;
        [SerializeField] private SpriteRenderer _toolRenderer;
        [SerializeField] private SpriteRenderer _handsRenderer;
        [SerializeField] private SpriteRenderer _fxRenderer;
        [SerializeField] private SpriteRenderer _blushRenderer;
        
        private static readonly int Velocity = Animator.StringToHash("Velocity");
        private const string DOING = "IsDoing";
        private const string AXE = "IsCutting";
        private const string BUILD = "IsBuilding";
        private const string DIG = "IsDigging";
        private const string WATER = "IsWatering";
        private const string MINE = "IsMining";
        
        private const string UP = "IsUp";
        private const string DOWN = "IsDown";
        
        private UnitAction _curUnitAction;

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
            SetHairDirection(UnitActionDirection.Side);
        }

        private void OnSpeedUpdated(float speedMod)
        {
            _baseAnim.speed = speedMod;
            _topAnim.speed = speedMod;
            _bottomAnim.speed = speedMod;
            _toolAnim.speed = speedMod;
            _handsAnim.speed = speedMod;
            _fxAnim.speed = speedMod;
            _blushAnim.speed = speedMod;
        }

        private void SetHairDirection(UnitActionDirection dir)
        {
            switch (dir)
            {
                case UnitActionDirection.Side:
                    _hairRenderer.sprite = _hairData.Side;
                    break;
                case UnitActionDirection.Up:
                    _hairRenderer.sprite = _hairData.Back;
                    break;
                case UnitActionDirection.Down:
                    _hairRenderer.sprite = _hairData.Front;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(dir), dir, null);
            }
        }
        
        public void SetUnitAction(UnitAction unitAction, UnitActionDirection direction)
        {
            _curUnitAction = unitAction;
            ClearAllActions();
            
            SetHairDirection(direction);

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
                case UnitAction.Axe:
                    SetUnitAction(AXE);
                    break;
                case UnitAction.Building:
                    SetUnitAction(BUILD);
                    break;
                case UnitAction.Digging:
                    SetUnitAction(DIG);
                    break;
                case UnitAction.Watering:
                    SetUnitAction(WATER);
                    break;
                case UnitAction.Mining:
                    SetUnitAction(MINE);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(unitAction), unitAction, null);
            }
        }

        private void SetUnitAction(string parameter, bool isActive = true)
        {
            _baseAnim.SetBool(parameter, isActive);
            _topAnim.SetBool(parameter, isActive);
            _bottomAnim.SetBool(parameter, isActive);
            _toolAnim.SetBool(parameter, isActive);
            _handsAnim.SetBool(parameter, isActive);
            _fxAnim.SetBool(parameter, isActive);
            _blushAnim.SetBool(parameter, isActive);
        }

        private void ClearAllActions()
        {
            SetUnitAction(DOING, false);
            SetUnitAction(AXE, false);
            SetUnitAction(BUILD, false);
            SetUnitAction(DIG, false);
            SetUnitAction(WATER, false);
            SetUnitAction(MINE, false);
        }
        
        public void SetMovementVelocity(Vector2 velocityVector)
        {
            if (TimeManager.Instance.GameSpeed == GameSpeed.Paused) return;
            
            SetVelocity(velocityVector.magnitude);
            _baseAnim.SetFloat(Velocity, velocityVector.magnitude);
            _topAnim.SetFloat(Velocity, velocityVector.magnitude);
            _bottomAnim.SetFloat(Velocity, velocityVector.magnitude);
            _toolAnim.SetFloat(Velocity, velocityVector.magnitude);
            _handsAnim.SetFloat(Velocity, velocityVector.magnitude);
            _fxAnim.SetFloat(Velocity, velocityVector.magnitude);
            _blushAnim.SetFloat(Velocity, velocityVector.magnitude);

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

            _baseRenderer.transform.localScale = new Vector3(scaleModX, 1, 1);
            _topRenderer.transform.localScale = new Vector3(scaleModX, 1, 1);
            _bottomRenderer.transform.localScale = new Vector3(scaleModX, 1, 1);
            _toolRenderer.transform.localScale = new Vector3(scaleModX, 1, 1);
            _handsRenderer.transform.localScale = new Vector3(scaleModX, 1, 1);
            _fxRenderer.transform.localScale = new Vector3(scaleModX, 1, 1);
            _blushRenderer.transform.localScale = new Vector3(scaleModX, 1, 1);
        }

        private void SetVelocity(float velocity)
        {
            _baseAnim.SetFloat(Velocity, velocity);
            _topAnim.SetFloat(Velocity, velocity);
            _bottomAnim.SetFloat(Velocity, velocity);
            _toolAnim.SetFloat(Velocity, velocity);
            _handsAnim.SetFloat(Velocity, velocity);
            _fxAnim.SetFloat(Velocity, velocity);
            _blushAnim.SetFloat(Velocity, velocity);
        }
    }

    public enum UnitAction
    {
        Nothing,
        Doing,
        Axe,
        Building,
        Digging,
        Watering,
        Mining,
    }

    public enum UnitActionDirection
    {
        Side,
        Up,
        Down,
    }
}
