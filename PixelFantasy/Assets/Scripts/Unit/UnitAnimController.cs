using System;
using Character.Interfaces;
using UnityEngine;

namespace Character
{
    public class UnitAnimController : MonoBehaviour, ICharacterAnimController
    {
        [SerializeField] private Animator baseAnim;
        [SerializeField] private Animator hairAnim;
        [SerializeField] private Animator toolAnim;

        [SerializeField] private SpriteRenderer baseRenderer;
        [SerializeField] private SpriteRenderer hairRenderer;
        [SerializeField] private SpriteRenderer toolRenderer;
        
        private static readonly int Velocity = Animator.StringToHash("Velocity");
        private const string DOING = "IsDoing";
        private const string AXE = "IsCutting";
        private const string BUILD = "IsBuilding";
        private const string DIG = "IsDigging";

        public void SetUnitAction(UnitAction unitAction)
        {
            ClearAllActions();
            
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
                default:
                    throw new ArgumentOutOfRangeException(nameof(unitAction), unitAction, null);
            }
        }

        private void SetUnitAction(string parameter, bool isActive = true)
        {
            baseAnim.SetBool(parameter, isActive);
            hairAnim.SetBool(parameter, isActive);
            toolAnim.SetBool(parameter, isActive);
        }

        private void ClearAllActions()
        {
            SetUnitAction(DOING, false);
            SetUnitAction(AXE, false);
            SetUnitAction(BUILD, false);
            SetUnitAction(DIG, false);
        }
        
        public void SetMovementVelocity(Vector2 velocityVector)
        {
            SetVelocity(velocityVector.magnitude);
            baseAnim.SetFloat(Velocity, velocityVector.magnitude);

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
            baseRenderer.flipX = shouldFlip;
            hairRenderer.flipX = shouldFlip;
            toolRenderer.flipX = shouldFlip;
        }

        private void SetVelocity(float velocity)
        {
            baseAnim.SetFloat(Velocity, velocity);
            hairAnim.SetFloat(Velocity, velocity);
            toolAnim.SetFloat(Velocity, velocity);
        }
    }

    public enum UnitAction
    {
        Nothing,
        Doing,
        Axe,
        Building,
        Digging,
    }
}
