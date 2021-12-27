using Character.Interfaces;
using UnityEngine;

namespace Character
{
    public class MoveTransformVelocity : MonoBehaviour, IMoveVelocity
    {
        [SerializeField] private float moveSpeed;

        private Vector3 velocityVector;


        private void Awake()
        {

        }

        public void SetVelocity(Vector3 velocityVector)
        {
            this.velocityVector = velocityVector;
        }

        private void Update()
        {
            transform.position += velocityVector * (moveSpeed * Time.deltaTime);
        }
    }
}
