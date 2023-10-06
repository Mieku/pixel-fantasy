using System;
using System.Collections.Generic;
using Characters;
using Managers;
using Systems.Blackboard.Scripts;
using Systems.Stats.Scripts;
using Systems.Traits.Scripts;
using TaskSystem;
using UnityEngine;

namespace Systems.SmartObjects.Scripts
{
    [Serializable]
    public class AIStatConfiguration
    {
        [field: SerializeField] public AIStat LinkedStat { get; private set; }
        [field: SerializeField] public bool OverrideDefaults { get; private set; } = false;
        [field: SerializeField, Range(0f, 1f)] public float Override_InitialValue { get; protected set; } = 0.5f;
        [field: SerializeField, Range(0f, 1f)] public float Override_DecayRate { get; protected set; } = 0.005f;

        public bool IsStatCritial(float currentValue)
        {
            var criticalPoint = LinkedStat.CriticalThreshold.ThresholdValue;
            return currentValue < criticalPoint;
        }
    }
    
    [RequireComponent(typeof(UnitAgent))]
    public class CommonAIBase : MonoBehaviour
    {
        [Header("General")]
        [SerializeField] private int _householdID = 1;
        [field: SerializeField] private AIStatConfiguration[] _stats;

        protected UnitAgent _navAgent;
        public Unit Unit { get; protected set; }
        protected Dictionary<AIStat, float> _decayRates = new Dictionary<AIStat, float>();
        protected Action<BaseInteraction> _onInteractionComplete;
        
        public Blackboard.Scripts.Blackboard IndividualBlackboard { get; protected set; }
        public Blackboard.Scripts.Blackboard HouseholdBlackboard { get; protected set; }

        protected BaseInteraction _curInteraction
        {
            get
            {
                BaseInteraction interaction = null;
                IndividualBlackboard.TryGetGeneric(EBlackboardKey.Character_FocusObject, out interaction, null);
                return interaction;
            }
            set
            {
                BaseInteraction previousInteraction = null;
                IndividualBlackboard.TryGetGeneric(EBlackboardKey.Character_FocusObject, out previousInteraction, null);
                
                IndividualBlackboard.SetGeneric(EBlackboardKey.Character_FocusObject, value);

                List<GameObject> objectsInUse = null;
                HouseholdBlackboard.TryGetGeneric(EBlackboardKey.Household_ObjectsInUse, out objectsInUse, null);
                
                // Are we starting to use something?
                if (value != null)
                {
                    // Need to create list?
                    if (objectsInUse == null)
                        objectsInUse = new List<GameObject>();
                    
                    // Not already in list? add and update the blackboard
                    if (!objectsInUse.Contains(value.gameObject))
                    {
                        objectsInUse.Add(value.gameObject);
                        HouseholdBlackboard.SetGeneric(EBlackboardKey.Household_ObjectsInUse, objectsInUse);
                    }
                }
                else if(objectsInUse != null) // We've stopped using something
                {
                    // Attempt to remove and update the blackboard if changed
                    if(objectsInUse.Remove(previousInteraction.gameObject))
                        HouseholdBlackboard.SetGeneric(EBlackboardKey.Household_ObjectsInUse, objectsInUse);
                }
            }
        }
        
        protected virtual void Awake()
        {
            _navAgent = GetComponent<UnitAgent>();
            Unit = GetComponent<Unit>();

            GameEvents.MinuteTick += MinuteTick;
        }

        protected virtual void OnDestroy()
        {
            GameEvents.MinuteTick -= MinuteTick;
        }
        
        protected virtual void Start()
        {
            IndividualBlackboard = BlackboardManager.Instance.GetIndividualBlackboard(this);
            HouseholdBlackboard = BlackboardManager.Instance.GetSharedBlackboard(_householdID);

            // Setup the Stats
            foreach (var statConfig in _stats)
            {
                var linkedStat = statConfig.LinkedStat;
                float initialValue = statConfig.OverrideDefaults
                    ? statConfig.Override_InitialValue
                    : linkedStat.InitialValue;
                float decayRate = statConfig.OverrideDefaults
                    ? statConfig.Override_DecayRate
                    : linkedStat.DecayRate;

                _decayRates[linkedStat] = decayRate;
                IndividualBlackboard.SetStat(linkedStat, initialValue);
            }
        }
        
        protected virtual void IsAtDestination()
        {
            _curInteraction.Perform(this, OnInteractionFinished);
        }

        protected virtual void OnInteractionFinished(BaseInteraction interaction)
        {
            interaction.UnlockInteraction(this);
            _onInteractionComplete.Invoke(interaction);
            _curInteraction = null;
        }

        public void UpdateIndividualStat(AIStat linkedStat, float amount, StatTrait.ETargetType targetType)
        {
            float adjustedAmount = ApplyTraitsTo(linkedStat, targetType, amount);
            float newValue = Mathf.Clamp01(GetStatValue(linkedStat) + adjustedAmount);

            IndividualBlackboard.SetStat(linkedStat, newValue);
        }
        
        protected float ApplyTraitsTo(AIStat targetStat, StatTrait.ETargetType targetType, float currentValue)
        {
            foreach (var trait in Unit.GetStatTraits())
            {
                currentValue = trait.Apply(targetStat, targetType, currentValue);
            }

            return currentValue;
        }

        public float GetStatValue(AIStat linkedStat)
        {
            return IndividualBlackboard.GetStat(linkedStat);
        }
        
        public bool AreAnyNeedsCritical(out AIStat criticalStat)
        {
            foreach (var statConfig in _stats)
            {
                if (statConfig.IsStatCritial(GetStatValue(statConfig.LinkedStat)))
                {
                    criticalStat = statConfig.LinkedStat;
                    return true;
                }
            }

            criticalStat = null;
            return false;
        }

        protected virtual void Update()
        {
            
        }

        protected void MinuteTick()
        {
            foreach (var statConfig in _stats)
            {
                UpdateIndividualStat(statConfig.LinkedStat, -_decayRates[statConfig.LinkedStat], StatTrait.ETargetType.DecayRate);
            }
        }

        public AIStatConfiguration[] AllStatConfigurations => _stats;
    }
}
