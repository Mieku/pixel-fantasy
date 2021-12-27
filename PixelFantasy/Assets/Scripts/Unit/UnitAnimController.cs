using System;
using Character.Interfaces;
using UnityEngine;

namespace Character
{
    // TODO: This is just a placeholder, need something more dynamic for later
    public class UnitAnimController : MonoBehaviour, ICharacterAnimController
    {
        private Animator anim;
        private WalkDirection prevWalkDir;
        private SpriteRenderer spriteRenderer;
        
        private static readonly int WalkSide = Animator.StringToHash("WalkSide");
        private static readonly int WalkUp = Animator.StringToHash("WalkUp");
        private static readonly int WalkDown = Animator.StringToHash("WalkDown");
        private static readonly int IdleSide = Animator.StringToHash("IdleSide");
        private static readonly int IdleUp = Animator.StringToHash("IdleUp");
        private static readonly int IdleDown = Animator.StringToHash("IdleDown");

        private void Awake()
        {
            anim = GetComponentInChildren<Animator>();
            spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        }

        public void SetMovementVelocity(Vector2 velocityVector)
        {
            if (velocityVector == Vector2.zero)
            {
                SetWalkDirection(WalkDirection.Idle);
                return;
            }

            var xAbs = Mathf.Abs(velocityVector.x);
            var yAbs = Mathf.Abs(velocityVector.y);
            
            // Horizontal
            if (xAbs >= yAbs)
            {
                if (velocityVector.x > 0)
                {
                    spriteRenderer.flipX = false;
                    SetWalkDirection(WalkDirection.Right);
                }
                else
                {
                    spriteRenderer.flipX = true;
                    SetWalkDirection(WalkDirection.Left);
                }
            }
            else // Vertical
            {
                if (velocityVector.y > 0)
                {
                    SetWalkDirection(WalkDirection.Up);
                }
                else
                {
                    SetWalkDirection(WalkDirection.Down);
                }
            }
        }

        private void SetWalkDirection(WalkDirection walkDir)
        {
            switch (walkDir)
            {
                case WalkDirection.Idle:
                    switch (prevWalkDir)
                    {
                        default:
                        case WalkDirection.Idle:
                        case WalkDirection.Down:
                            anim.SetTrigger(IdleDown);
                            break;
                        case WalkDirection.Up:
                            anim.SetTrigger(IdleUp);
                            break;
                        case WalkDirection.Left:
                        case WalkDirection.Right:
                            anim.SetTrigger(IdleSide);
                            break;
                    }
                    break;
                case WalkDirection.Down:
                    anim.SetTrigger(WalkDown);
                    prevWalkDir = WalkDirection.Down;
                    break;
                case WalkDirection.Up:
                    anim.SetTrigger(WalkUp);
                    prevWalkDir = WalkDirection.Up;
                    break;
                case WalkDirection.Left:
                    anim.SetTrigger(WalkSide);
                    prevWalkDir = WalkDirection.Left;
                    break;
                case WalkDirection.Right:
                    anim.SetTrigger(WalkSide);
                    prevWalkDir = WalkDirection.Right;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(walkDir), walkDir, null);
            }
        }

        private enum WalkDirection
        {
            Idle,
            Down,
            Up,
            Left,
            Right
        }
    }
}
