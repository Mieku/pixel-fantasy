/*
 *	DATABRAIN
 *	(c) 2023 Giant Grey
 *	www.databrain.cc
 *	
 */
using UnityEngine;
using UnityEngine.Events;

namespace Databrain.Runtime.Components
{
    [RequireComponent(typeof(SphereCollider))]
    public class TriggerActionAtRadius : MonoBehaviour
    {
        public LayerMask playerLayerMask;
        public bool triggerOnce;
        public bool destroyOnTrigger;
        public UnityEvent unityEvent;

        private bool triggered = false;

        private void Awake()
        {
            triggered = false;
        }


        public void OnTriggerEnter(Collider other)
        {
            if (triggered && triggerOnce)
                return;

            if (playerLayerMask == (playerLayerMask | (1 << other.gameObject.layer)))
            {
                unityEvent?.Invoke();
                triggered = true;

                if (destroyOnTrigger)
                {
                    Destroy(gameObject);
                }
            }
        }
    }
}
