using System.Collections;
using InfinityPBR.Modules;
using UnityEngine;
using static InfinityPBR.Modules.Timeboard;

/*
 * This script will subscribe to the Timeboard, and receive changes. Other scripts can inherit
 * from this, and override the methods if they'd like to perform custom operations, such as checking
 * the Topic/Subject of the events, or the data contained in the events.
 *
 * This will automatically subscribe and unsubscribe when the script is enabled or disabled.
 */

namespace InfinityPBR
{
    public class TimeboardFollower : MonoBehaviour, IFollowBlackboard
    {
        protected bool _setup; // Track if we've setup yet.
        
        /*
         * OnEnable, if the static reference to Timeboard is available, we will Subscribe() to the
         * Timeboard.Blackboard. If it is not yet available, we will start a coroutine to wait for
         * it to be available.
         *
         * Note: We always Unsubscribe() before Subscribing() to ensure that we are only ever subscribed
         * once.
         */
        
        protected virtual void OnEnable()
        {
            // If we have the reference, then Subscribe() and return;
            if (timeboard)
            {
                Subscribe();
                return;
            }
            
            // Start the coroutine to wait for the reference to be available
            StartCoroutine(SubscribeToTimeboard());
        }
        protected virtual void OnDisable() => Unsubscribe(); // Unsubscribe when this is disabled.
        
        protected virtual IEnumerator SubscribeToTimeboard()
        {
            // If we don't have the reference, wait one frame
            while (timeboard == null)
                yield return null;

            Subscribe();
            _setup = true;
        }

        protected virtual void Subscribe()
        {
            Unsubscribe(); // Always Unsubscribe() first!!
            timeboard.Blackboard.Subscribe(this);
        }

        protected virtual void Unsubscribe() => timeboard.Blackboard.Unsubscribe(this);

        public virtual void ReceiveChange(BlackboardNote blackboardNote) { }

        public virtual void ReceiveEvent(BlackboardEvent blackboardEvent) { }

    }
}
