using System;
using System.Collections.Generic;
using ScriptableObjects;
using Sirenix.OdinInspector;
using Systems.Mood.Scripts;
using Systems.Stats.Scripts;
using Systems.Traits.Scripts;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

namespace Systems.SmartObjects.Scripts
{
    public enum EInteractionType
    {
        Instantaneous = 0,
        OverTime = 1,
    }

    [System.Serializable]
    public class InteractionStatChange
    {
        public AIStat LinkedStat;
        public float Value;
    }
    
    public abstract class BaseInteraction : MonoBehaviour
    {
        [SerializeField] protected string _displayName;
        [SerializeField] protected float _duration = 0f;
        [SerializeField] protected UnitAction _performingAnimation;
        [SerializeField] protected List<InteractionStatChange> _statChanges;
        [SerializeField] protected bool _allowAmbientStats;
        [SerializeField] protected bool _beginInteractionAtSmartObject = true;
        [SerializeField] protected Emotion _interactionEmotion;

        protected List<AmbientStatChange> _ambientStatChanges = new List<AmbientStatChange>();

        public string DisplayName => _displayName;
        
        public float Duration => _duration;
        public bool BeginInteractionAtSmartObject => _beginInteractionAtSmartObject;
        protected UnityAction<BaseInteraction> _onCompleted;

        public virtual List<InteractionStatChange> GetStatChanges()
        {
            List<InteractionStatChange> results = new List<InteractionStatChange>();
            foreach (var statChange in _statChanges)
            {
                results.Add(statChange);
            }

            if (_allowAmbientStats)
            {
                foreach (var ambientStatChange in _ambientStatChanges)
                {
                    var stats = ambientStatChange.StatChanges;
                    foreach (var statChange in stats)
                    {
                        results.Add(statChange);
                    }
                }
            }
                
            return results;
        }

        public abstract bool CanPerform(CommonAIBase potentialPerformer);
        
        public abstract bool LockInteration(CommonAIBase performer);
        
        public abstract bool Perform(CommonAIBase performer, UnityAction<BaseInteraction> onCompleted);

        public abstract void CancelInteration(CommonAIBase performer);

        public abstract bool UnlockInteraction(CommonAIBase performer);

        protected virtual void OnInteractionCompleted(CommonAIBase performer, UnityAction<BaseInteraction> onCompleted)
        {
            _onCompleted = onCompleted;
            ApplyEmotion(performer);
        }

        public bool ContainsPositiveStatChange(AIStat stat)
        {
            foreach (var statChange in _statChanges)
            {
                if (statChange.LinkedStat == stat && statChange.Value > 0)
                {
                    return true;
                }
            }

            return false;
        }

        protected void ApplyStatChanges(CommonAIBase performer, float proportion)
        {
            foreach (var statChange in _statChanges)
            {
                performer.UpdateIndividualStat(statChange.LinkedStat, statChange.Value * proportion, StatTrait.ETargetType.Impact);
            }
        }

        protected void ApplyEmotion(CommonAIBase performer)
        {
            if (_interactionEmotion != null)
            {
                performer.Unit.KinlingMood.ApplyEmotion(_interactionEmotion);
            }
        }

        public void RegisterAmbientStat(AmbientStatChange ambientStatChange)
        {
            if (_ambientStatChanges.Contains(ambientStatChange))
            {
                return;
            }
            
            _ambientStatChanges.Add(ambientStatChange);
        }

        public void DeregisterAmbientStat(AmbientStatChange ambientStatChange)
        {
            if (!_ambientStatChanges.Contains(ambientStatChange))
            {
                return;
            }

            _ambientStatChanges.Remove(ambientStatChange);
        }
    }
}
