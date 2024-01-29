using System;
using System.Collections.Generic;
using System.Timers;
using Managers;
using QFSW.QC;
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

        private Unit _kinling;

        private void Awake()
        {
            _kinling = GetComponent<Unit>();
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

        private const float BASE_SEX_DRIVE = 50f;
        private int lastDayChecked = -1;
        public bool CheckSexDrive()
        {
            // Is adult
            if (_kinling.MaturityStage == EMaturityStage.Child) return false;
            
            // Check only once a day
            int currentDay = EnvironmentManager.Instance.GameTime.Day;
            if (currentDay == lastDayChecked) return false;
            
            // Have the sex drive be influenced by mood
            lastDayChecked = currentDay;
            var currentMood = _kinling.KinlingMood.OverallMood;
            float sexDriveModifier = (1f / 75f) * currentMood;
            float sexDrive = BASE_SEX_DRIVE * sexDriveModifier;
            Debug.Log($"Sex Drive: {sexDrive}");

            bool result = Helper.RollDice(sexDrive);
            return result;
        }

        [Command("set_need_all", "The supported needs are Food, Energy, Water", MonoTargetType.All)]
        private void CMD_SetNeedAll(string needName, int amount)
        {
            NeedType targetNeed;
            switch (needName)
            {
                case "Food":
                    targetNeed = NeedType.Food;
                    break;
                case "Energy":
                    targetNeed = NeedType.Energy;
                    break;
                case "Water":
                    targetNeed = NeedType.Water;
                    break;
                default:
                    Debug.LogError("Unknown Need: " + needName);
                    return;
            }

            var need = GetNeedByType(targetNeed);
            need.SetNeed(amount);
        }
        
        [Command("set_need", "The supported needs are Food, Energy, Water", MonoTargetType.All)]
        private void CMD_SetNeed(string firstname, string needName, int amount)
        {
            if(_kinling.FirstName == firstname)
            {
                NeedType targetNeed;
                switch (needName)
                {
                    case "Food":
                        targetNeed = NeedType.Food;
                        break;
                    case "Energy":
                        targetNeed = NeedType.Energy;
                        break;
                    case "Water":
                        targetNeed = NeedType.Water;
                        break;
                    default:
                        Debug.LogError("Unknown Need: " + needName);
                        return;
                }

                var need = GetNeedByType(targetNeed);
                need.SetNeed(amount);
            }
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
