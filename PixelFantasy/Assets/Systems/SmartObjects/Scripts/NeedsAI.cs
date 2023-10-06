using System;
using System.Collections.Generic;
using System.Linq;
using Characters;
using Managers;
using ScriptableObjects;
using Systems.Blackboard.Scripts;
using Systems.Stats.Scripts;
using Systems.Traits.Scripts;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Systems.SmartObjects.Scripts
{
    public class NeedsAI : CommonAIBase
    {
        [SerializeField] protected float _defaultInteractionScore = 0f;
        [SerializeField] protected int _interactionPickSize = 3;
        [SerializeField] private bool _avoidInUseObjects = true;

        protected override void Start()
        {
            base.Start();
        }

        protected override void Update()
        {
            base.Update();
        }

        
        private float ScoreInteraction(BaseInteraction interaction)
        {
            if (interaction.GetStatChanges().Count == 0)
            {
                return _defaultInteractionScore;
            }

            float score = 0f;
            foreach (var change in interaction.GetStatChanges())
            {
                score += ScoreChange(change.LinkedStat, change.Value);
            }

            return score;
        }

        private float ScoreChange(AIStat linkedStat, float changeAmount)
        {
            float currentValue = GetStatValue(linkedStat);
            float changeAmountModified = ApplyTraitsTo(linkedStat, StatTrait.ETargetType.Score, changeAmount);
            float intensity = linkedStat.CalculateIntensity(currentValue);
            float score = changeAmountModified * intensity;
            return score;
        }

        class ScoredInteraction
        {
            public SmartObject TargetObject;
            public BaseInteraction Interaction;
            public float Score;
        }
        
        public bool PickBestInteraction(Action<BaseInteraction> onInteractionComplete, AIStat focusStat)
        {
            List<GameObject> objectsInUse = null;
            HouseholdBlackboard.TryGetGeneric(EBlackboardKey.Household_ObjectsInUse, out objectsInUse, null);

            List<ScoredInteraction> unsortedInteractions = new List<ScoredInteraction>();
            foreach (var smartObject in SmartObjectManager.Instance.RegisteredSmartObjects)
            {
                foreach (var interaction in smartObject.Interactions)
                {
                    if (!interaction.CanPerform(this)) continue;
                    if (focusStat != null)
                    {
                        if (!interaction.ContainsPositiveStatChange(focusStat))
                        {
                            continue;
                        }
                    }

                    // Skip if someone else is using
                    if(_avoidInUseObjects && objectsInUse != null && objectsInUse.Contains(interaction.gameObject))
                        continue;

                    float score = ScoreInteraction(interaction);
                    
                    unsortedInteractions.Add(new ScoredInteraction()
                    {
                        TargetObject = smartObject,
                        Interaction = interaction,
                        Score = score,
                    });
                }
            }

            if (unsortedInteractions.Count == 0)
                return false;

            var sortedInteractions = unsortedInteractions.OrderByDescending(scoredInteraction => scoredInteraction.Score).ToList();
            int maxIndex = Mathf.Min(_interactionPickSize, sortedInteractions.Count);

            var selectedIndex = Random.Range(0, maxIndex);
            
            var selectedObject = sortedInteractions[selectedIndex].TargetObject;
            var selectedInteraction = sortedInteractions[selectedIndex].Interaction;

            _onInteractionComplete = onInteractionComplete;
            _curInteraction = selectedInteraction;
            _curInteraction.LockInteration(this);
            
            if (_curInteraction.BeginInteractionAtSmartObject)
            {
                if(!_navAgent.SetMovePosition(selectedObject.InteractionPoint, IsAtDestination))
                {
                    Debug.LogError($"Could not move to {selectedObject.DisplayName}");
                    _curInteraction = null;
                }
            }
            else
            {
                _curInteraction.Perform(this, OnInteractionFinished);
            }

            return true;
        }
    }
}
