using System;
using Characters.Interfaces;
using UnityEngine;

namespace Characters
{
    [RequireComponent(typeof(Rigidbody2D))]
    public class MoveVelocity : MonoBehaviour, IMoveVelocity
    {
        [SerializeField] private float moveSpeed;

        private Vector3 velocityVector;
        private new Rigidbody2D rigidbody2D;
        private ICharacterAnimController charAnimController;

        private void Awake()
        {
            rigidbody2D = GetComponent<Rigidbody2D>();
            charAnimController = GetComponent<ICharacterAnimController>();
        }

        public void SetVelocity(Vector3 velocityVector)
        {
            this.velocityVector = velocityVector;
        }

        private void FixedUpdate()
        {
            rigidbody2D.velocity = velocityVector * moveSpeed;
            charAnimController.SetMovementVelocity(velocityVector);
        }
    }
}
