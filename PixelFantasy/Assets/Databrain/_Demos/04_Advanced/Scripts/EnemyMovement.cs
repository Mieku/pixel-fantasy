using UnityEngine;

namespace Databrain.Examples
{
    public class EnemyMovement : MonoBehaviour
    {
        public float walkRadius;

        private Vector3 destinationPoint = Vector3.zero;
        private float smoothTime = 0.5f;
        private float speed = 2;
        private Vector3 velocity;

        // Start is called before the first frame update
        void Start()
        {
            NewRandomPoint();
        }


        void NewRandomPoint()
        {
            Vector3 randomPoint = Random.insideUnitSphere * walkRadius;
            destinationPoint = new Vector3(randomPoint.x + transform.position.x, 1, randomPoint.z + transform.position.z);
        }

        // Update is called once per frame
        void Update()
        {
            transform.position = Vector3.SmoothDamp(transform.position, destinationPoint, ref velocity, smoothTime, speed);

            var _distance = Vector3.Distance(transform.position, destinationPoint);
            if (_distance <= 1)
            {
                NewRandomPoint();
            }
        }
    }
}