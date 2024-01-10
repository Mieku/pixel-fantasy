using System;
using System.Collections.Generic;
using Systems.Stats.Scripts;
using UnityEngine;

namespace Characters
{
    public class KinlingStats : MonoBehaviour
    {
        [Header("Stats")] 
        [SerializeField] protected Stat _foodStat;
        [SerializeField] protected Stat _energyStat;

        [SerializeField] private List<StatChange> _registeredStatChanges = new List<StatChange>();

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
            DecayStats();
            StatChangePerMin();
        }

        public void Initialize()
        {
            _foodStat.Initialize();
            _energyStat.Initialize();
        }

        private void DecayStats()
        {
            _foodStat.MinuteTickDecayStat();
            _energyStat.MinuteTickDecayStat();
        }

        private void StatChangePerMin()
        {
            foreach (var statChange in _registeredStatChanges)
            {
                var stat = GetStatByType(statChange.StatType);
                if (stat != null)
                {
                    if (statChange.AmountPerMin > 0)
                    {
                        stat.IncreaseStat(statChange.AmountPerMin);
                    }
                    else if (statChange.AmountPerMin < 0)
                    {
                        stat.DecreaseStat(statChange.AmountPerMin);
                    }
                }
            }
        }

        public void RegisterStatChangePerHour(string sourceID, StatType statType, float changePerHour)
        {
            StatChange change = new StatChange(sourceID, statType, changePerHour);
            RegisterStatChangePerHour(change);
        }

        public void RegisterStatChangePerHour(StatChange change)
        {
            _registeredStatChanges.Add(change);
        }

        public void DeregisterStatChangePerHour(string sourceID)
        {
            foreach (var statChange in _registeredStatChanges)
            {
                if (statChange.SourceID == sourceID)
                {
                    _registeredStatChanges.Remove(statChange);
                    return;
                }
            }
            
            Debug.LogError($"Could not find Stat Change ID: {sourceID}");
        }

        public void DeregisterStatChangePerHour(StatChange change)
        {
            var result = _registeredStatChanges.Remove(change);
            if (!result)
            {
                Debug.LogError($"Could not find Stat Change ID: {change.SourceID}");
            }
        }

        public Stat GetStatByType(StatType statType)
        {
            switch (statType)
            {
                case StatType.Food:
                    return _foodStat;
                case StatType.Energy:
                    return _energyStat;
                default:
                    throw new ArgumentOutOfRangeException(nameof(statType), statType, null);
            }
        }
    }

    [Serializable]
    public class StatChange
    {
        public string SourceID;
        public float AmountPerHour;
        public StatType StatType;
        public float AmountPerMin => AmountPerHour / 60f;

        public StatChange(string sourceID, StatType statType, float amountPerHour)
        {
            SourceID = sourceID;
            StatType = statType;
            AmountPerHour = amountPerHour;
        }
    }

    public enum StatType
    {
        Food = 0,
        Energy = 1,
    }
}
