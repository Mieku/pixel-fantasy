using System;
using System.Collections;
using UnityEngine;
using static InfinityPBR.Modules.MainBlackboard;

namespace InfinityPBR.Modules
{
    [Serializable]
    public class BlackboardFollower : MonoBehaviour, IFollowBlackboard
    {
        /*
         * OnEnable, if the static reference to MainBlackboard is available, we will Subscribe() to the
         * MainBlackboard.blackboard. If it is not yet available, we will start a coroutine to wait for
         * it to be available.
         *
         * Note: We always Unsubscribe() before Subscribing() to ensure that we are only ever subscribed
         * once.
         */
        
        protected virtual void OnEnable()
        {
            // If we have the reference, then Subscribe() and return;
            if (blackboard)
            {
                Subscribe();
                return;
            }
            
            // Start the coroutine to wait for the reference to be available
            StartCoroutine(SubscribeToBlackboard());
        }

        protected virtual void OnDisable() => Unsubscribe();
        
        protected virtual IEnumerator SubscribeToBlackboard()
        {
            // If we don't have the reference, wait one frame
            while (blackboard == null)
                yield return null;

            Subscribe();
        }

        private void Subscribe()
        {
            Unsubscribe(); // Always unsubscribe first, to ensure there is only one subscription
            blackboard.Subscribe(this);
        }
        private void Unsubscribe() => blackboard.Unsubscribe(this);
        
        // You will need to override this method in your child classes
        public virtual void ReceiveChange(BlackboardNote blackboardNote)
        {
           
        }

        // You will need to override this method in your child classes
        public virtual void ReceiveEvent(BlackboardEvent blackboardEvent)
        {
            
        }
    }
}
