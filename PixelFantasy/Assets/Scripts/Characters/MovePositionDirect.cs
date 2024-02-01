using System;
using Characters.Interfaces;
using UnityEngine;

namespace Characters
{
    public class MovePositionDirect : MonoBehaviour, IMovePosition
    {
        [SerializeField] private float stopDistanceThreshold = 0.1f;
        
        private Vector3 movePosition;
        private IMoveVelocity moveVelocity;
        private bool movingToPos;
        
        private void Awake()
        {
            moveVelocity = GetComponent<IMoveVelocity>();
        }

        public bool SetMovePosition(Vector2? movePos, Action onReachedMovePosition, Action onImpossiblePosition)
        {
            if (movePos == null)
            {
                onImpossiblePosition?.Invoke();
                return false;
            }
            
            this.movePosition = (Vector2)movePos;
            movingToPos = true;
            return true;
        }
        
        private void FixedUpdate()
        {
            if (movingToPos)
            {
                Vector3 moveDir = (movePosition - transform.position).normalized;
                if (Vector3.Distance(movePosition, transform.position) < stopDistanceThreshold)
                {
                    moveDir = Vector3.zero;
                    movingToPos = false;
                }
                    
                moveVelocity.SetVelocity(moveDir);
            }
            
        }
    }
}
