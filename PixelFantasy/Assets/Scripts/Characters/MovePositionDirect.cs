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

        public void SetMovePosition(Vector3 movePosition, Action onReachedMovePosition)
        {
            this.movePosition = movePosition;
            movingToPos = true;
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
