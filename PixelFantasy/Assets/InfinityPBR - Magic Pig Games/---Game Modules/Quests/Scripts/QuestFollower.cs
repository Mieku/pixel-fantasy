using System;
using System.Collections;
using UnityEngine;
using static InfinityPBR.Modules.QuestEventBoard;

namespace InfinityPBR.Modules
{
    [Serializable]
    public class QuestFollower : MonoBehaviour, IFollowQuests
    {
        /*
         * This is the method you should override! This way, you can handle the QuestEvent however you'd like. Each
         * use case for each Quest may have a different logic you'd like to utilize, so that part is entirely up to you
         * to handle.
         */
        public virtual void ReceiveQuestEvent(QuestEvent questEvent)
        {
            
        }
        
        protected virtual void OnEnable()
        {
            // If we have the reference, then Subscribe() and return;
            if (questEventBoard)
            {
                Subscribe();
                return;
            }
            
            // Start the coroutine to wait for the reference to be available
            StartCoroutine(SubscribeToQuestEventBoard());
        }
        
        protected virtual void OnDisable() => Unsubscribe();
        
        protected virtual IEnumerator SubscribeToQuestEventBoard()
        {
            // If we don't have the reference, wait one frame
            while (questEventBoard == null)
                yield return null;

            Subscribe();
        }
        
        private void Subscribe()
        {
            Unsubscribe();
            questEventBoard.Subscribe(this);
        }
        private void Unsubscribe() => questEventBoard.Unsubscribe(this);
        
        public virtual void ReceiveChange(BlackboardNote blackboardNote)
        {
           
        }

        public virtual void ReceiveEvent(BlackboardEvent blackboardEvent)
        {
            
        }
        
    }
}
