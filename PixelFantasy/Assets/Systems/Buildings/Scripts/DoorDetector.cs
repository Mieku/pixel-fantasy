using System;
using Characters;
using UnityEngine;
using UnityEngine.Events;

namespace Systems.Buildings.Scripts
{
    [RequireComponent(typeof(BoxCollider2D))]
    public class DoorDetector : MonoBehaviour
    {
        private BoxCollider2D _boxCollider;

        public UnityEvent<Kinling> OnKinlingInDoorway;
        public UnityEvent<Kinling> OnKinlingLeftDoorway;

        private void Awake()
        {
            _boxCollider = GetComponent<BoxCollider2D>();
        }
        
        private void OnTriggerEnter2D(Collider2D other)
        {
            Kinling kinling = other.GetComponent<Kinling>();
            if (kinling != null)
            {
                OnKinlingInDoorway?.Invoke(kinling);
            }
        }
        
        private void OnTriggerExit2D(Collider2D other)
        {
            Kinling kinling = other.GetComponent<Kinling>();
            if (kinling != null)
            {
                OnKinlingLeftDoorway?.Invoke(kinling);
            }
        }
    }
}
