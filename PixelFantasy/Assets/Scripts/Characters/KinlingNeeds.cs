using System;
using System.Collections.Generic;
using Systems.Needs.Scripts;
using UnityEngine;
using UnityEngine.Serialization;

namespace Characters
{
    public class KinlingNeeds : MonoBehaviour
    {
        [Header("Needs")] 
        [SerializeField] protected NeedState _foodNeedState;
        [SerializeField] protected NeedState _waterNeedState;
        [SerializeField] protected NeedState _energyNeedState;

        [SerializeField] private List<NeedChange> _registeredNeedChanges = new List<NeedChange>();

        private void Awake()
        {
            GameEvents.MinuteTick += GameEvent_MinuteTick;
        }

        private void OnDestroy()
        {
            GameEvents.MinuteTick -= GameEvent_MinuteTick;
        }
        
        private void GameEvent_MinuteTick()
        {
            DecayNeeds();
            NeedChangePerMin();
        }

        public void Initialize()
        {
            _foodNeedState.Initialize();
            _energyNeedState.Initialize();
            _waterNeedState.Initialize();
        }

        private void DecayNeeds()
        {
            _foodNeedState.MinuteTickDecayNeed();
            _energyNeedState.MinuteTickDecayNeed();
            _waterNeedState.MinuteTickDecayNeed();
        }

        private void NeedChangePerMin()
        {
            foreach (var needChange in _registeredNeedChanges)
            {
                var need = GetNeedByType(needChange.NeedType);
                if (need != null)
                {
                    if (needChange.AmountPerMin > 0)
                    {
                        need.IncreaseNeed(needChange.AmountPerMin);
                    }
                    else if (needChange.AmountPerMin < 0)
                    {
                        need.DecreaseNeed(needChange.AmountPerMin);
                    }
                }
            }
        }

        public void RegisterNeedChangePerHour(string sourceID, NeedType needType, float changePerHour)
        {
            NeedChange change = new NeedChange(sourceID, needType, changePerHour);
            RegisterNeedChangePerHour(change);
        }

        public void RegisterNeedChangePerHour(NeedChange change)
        {
            _registeredNeedChanges.Add(change);
        }

        public void DeregisterNeedChangePerHour(string sourceID)
        {
            foreach (var needChange in _registeredNeedChanges)
            {
                if (needChange.SourceID == sourceID)
                {
                    _registeredNeedChanges.Remove(needChange);
                    return;
                }
            }
            
            Debug.LogError($"Could not find Need Change ID: {sourceID}");
        }

        public void DeregisterNeedChangePerHour(NeedChange change)
        {
            var result = _registeredNeedChanges.Remove(change);
            if (!result)
            {
                Debug.LogError($"Could not find Need Change ID: {change.SourceID}");
            }
        }

        public NeedState GetNeedByType(NeedType needType)
        {
            switch (needType)
            {
                case NeedType.Food:
                    return _foodNeedState;
                case NeedType.Energy:
                    return _energyNeedState;
                case NeedType.Water:
                    return _waterNeedState;
                default:
                    throw new ArgumentOutOfRangeException(nameof(needType), needType, null);
            }
        }

        public float GetNeedValue(NeedType needType)
        {
            var need = GetNeedByType(needType);
            if (need == null)
            {
                Debug.LogError($"Unable to get need value for {needType}");
                return 0f;
            }

            return need.Value;
        }

        public float IncreaseNeedValue(NeedType needType, float amount)
        {
            var need = GetNeedByType(needType);
            if (need == null)
            {
                Debug.LogError($"Unable to get need for {needType}");
                return 0f;
            }

            return need.IncreaseNeed(amount);
        }
        
        public float DecreaseNeedValue(NeedType needType, float amount)
        {
            var need = GetNeedByType(needType);
            if (need == null)
            {
                Debug.LogError($"Unable to get need for {needType}");
                return 0f;
            }

            return need.DecreaseNeed(amount);
        }

        public bool CheckSexDrive()
        {
            return false; // TODO: Build me!
        }
    }

    [Serializable]
    public class NeedChange
    {
        public string SourceID;
        public float AmountPerHour;
        [FormerlySerializedAs("StatType")] public NeedType NeedType;
        public float AmountPerMin => AmountPerHour / 60f;

        public NeedChange(string sourceID, NeedType needType, float amountPerHour)
        {
            SourceID = sourceID;
            NeedType = needType;
            AmountPerHour = amountPerHour;
        }
    }

    public enum NeedType
    {
        Food = 0,
        Energy = 1,
        Water = 2,
    }
}
