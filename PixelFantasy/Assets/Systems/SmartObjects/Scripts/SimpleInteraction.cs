using System.Collections.Generic;
using Managers;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

namespace Systems.SmartObjects.Scripts
{
    public class SimpleInteraction : BaseInteraction
    {
        protected class PerformerInfo
        {
            public float ElapsedTime;
            public UnityAction<BaseInteraction> OnCompleted;
        }
        
        [SerializeField] protected EInteractionType _interactionType = EInteractionType.Instantaneous;
        [SerializeField] protected int _maxSimultaneousUsers = 1;

        protected Dictionary<CommonAIBase, PerformerInfo> _currentPerformers = new Dictionary<CommonAIBase, PerformerInfo>();
        public int NumCurrentUsers => _currentPerformers.Count;
        
        protected EInteractionType InteractionType => _interactionType;
        protected List<CommonAIBase> _performersToCleanup = new List<CommonAIBase>();

        public override bool CanPerform(CommonAIBase potentialPerformer)
        {
            return NumCurrentUsers < _maxSimultaneousUsers;
        }

        public override bool LockInteration(CommonAIBase performer)
        {
            if (NumCurrentUsers >= _maxSimultaneousUsers)
            {
                Debug.LogError($"{performer.name} is trying to lock {_displayName} which is already at max users");
                return false;
            }

            if (_currentPerformers.ContainsKey(performer))
            {
                Debug.LogError($"{performer.name} tried to lock {_displayName} multiple times.");
                return false;
            }
            
            _currentPerformers[performer] = null;

            return true;
        }

        public override bool Perform(CommonAIBase performer, UnityAction<BaseInteraction> onCompleted)
        {
            if (!_currentPerformers.ContainsKey(performer))
            {
                Debug.LogError($"{performer.name} is trying to preform an interaction {_displayName} that they have not locked");
                return false;
            }
            
            // Start perform animation
            if (_performingAnimation != UnitAction.Nothing)
            {
                performer.Unit.UnitAnimController.SetUnitAction(_performingAnimation);
            }

            // Check the interaction type
            if (_interactionType == EInteractionType.Instantaneous)
            {
                if (_statChanges.Count > 0)
                {
                    ApplyStatChanges(performer, 1f);
                }

                OnInteractionCompleted(performer, onCompleted);
            }
            else if (_interactionType == EInteractionType.OverTime)
            {
                _currentPerformers[performer] = new PerformerInfo()
                {
                    ElapsedTime = 0, 
                    OnCompleted = onCompleted
                };
            }

            return true;
        }

        protected override void OnInteractionCompleted(CommonAIBase performer, UnityAction<BaseInteraction> onCompleted)
        {
            base.OnInteractionCompleted(performer, onCompleted);
            
            performer.Unit.UnitAnimController.SetUnitAction(UnitAction.Nothing);
            onCompleted.Invoke(this);

            if (!_performersToCleanup.Contains(performer))
            {
                _performersToCleanup.Add(performer);
                Debug.LogWarning($"{performer.name} did not unlock interaction in their OnCompleted handler for {_displayName}");
            }
        }
        
        public override bool UnlockInteraction(CommonAIBase performer)
        {
            if (_currentPerformers.ContainsKey(performer))
            {
                _performersToCleanup.Add(performer);
                return true;
            }
            
            Debug.LogError($"{performer.name} is trying to unlock an interaction {_displayName} they have not locked");
            return false;
        }

        protected virtual void Update()
        {
            // Update any current performers
            foreach (var kvp in _currentPerformers)
            {
                CommonAIBase performer = kvp.Key;
                PerformerInfo performerInfo = kvp.Value;
                
                if(performerInfo == null)
                    continue;

                float previousElaspedTime = performerInfo.ElapsedTime;
                performerInfo.ElapsedTime = Mathf.Min(performerInfo.ElapsedTime + TimeManager.Instance.DeltaTime, _duration);

                if (_statChanges.Count > 0)
                {
                    ApplyStatChanges(performer, (performerInfo.ElapsedTime - previousElaspedTime) / _duration);
                }
                
                // Interaction Complete?
                if (performerInfo.ElapsedTime >= _duration)
                {
                    OnInteractionCompleted(performer, performerInfo.OnCompleted);
                }
            }

            // Cleanup any performers that are finished
            foreach (var performer in _performersToCleanup)
            {
                _currentPerformers.Remove(performer);
            }
            _performersToCleanup.Clear();
        }
    }
}
