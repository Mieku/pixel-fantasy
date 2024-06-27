using System;
using System.Collections.Generic;
using Managers;
using QFSW.QC;
using Sirenix.OdinInspector;
using Systems.Appearance.Scripts;
using Systems.Needs.Scripts;
using UnityEngine;
using UnityEngine.Serialization;

namespace Characters
{
    [Serializable]
    public class NeedsData
    {
        [SerializeField] protected Need _foodNeed;
        [SerializeField] protected Need _energyNeed;
        [SerializeField] protected Need _funNeed;
        [SerializeField] protected Need _beautyNeed;
        [SerializeField] protected Need _comfortNeed;

        [SerializeField] private List<NeedChange> _registeredNeedChanges;

        private Kinling _kinling;

        public NeedsData(RacialNeeds racialNeeds)
        {
            _foodNeed = new Need(racialNeeds.Food);
            _energyNeed = new Need(racialNeeds.Energy);
            _funNeed = new Need(racialNeeds.Fun);
            _beautyNeed = new Need(racialNeeds.Beauty);
            _comfortNeed = new Need(racialNeeds.Comfort);

            _registeredNeedChanges = new List<NeedChange>();
        }
        
        public void MinuteTick()
        {
            NeedChangePerMin();
            
            _foodNeed.MinuteTick();
            _energyNeed.MinuteTick();
            _funNeed.MinuteTick();
            _beautyNeed.MinuteTick();
            _comfortNeed.MinuteTick();
        }

        public void Initialize(Kinling kinling)
        {
            _kinling = kinling;
            
            _foodNeed.Initialize(_kinling);
            _energyNeed.Initialize(_kinling);
            _funNeed.Initialize(_kinling);
            _beautyNeed.Initialize(_kinling);
            _comfortNeed.Initialize(_kinling);
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

        public Need GetNeedByType(NeedType needType)
        {
            return needType switch
            {
                NeedType.Food => _foodNeed,
                NeedType.Energy => _energyNeed,
                NeedType.Fun => _funNeed,
                NeedType.Beauty => _beautyNeed,
                NeedType.Comfort => _comfortNeed,
                _ => throw new ArgumentOutOfRangeException(nameof(needType), needType, null)
            };
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
            if (_kinling.RuntimeData.MaturityStage == EMaturityStage.Child) return false;
            
            // Check only once a day
            int currentDay = EnvironmentManager.Instance.GameTime.Day;
            if (currentDay == lastDayChecked) return false;
            
            // Have the sex drive be influenced by mood
            lastDayChecked = currentDay;
            var currentMood = _kinling.MoodData.OverallMood;
            float sexDriveModifier = (1f / 75f) * currentMood;
            float sexDrive = BASE_SEX_DRIVE * sexDriveModifier;
            Debug.Log($"Sex Drive: {sexDrive}");

            bool result = Helper.RollDice(sexDrive);
            return result;
        }

        [Command("set_need_all", "The supported needs are Food, Energy", MonoTargetType.All)]
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
                case "Fun":
                    targetNeed = NeedType.Fun;
                    break;
                case "Beauty":
                    targetNeed = NeedType.Beauty;
                    break;
                case "Comfort":
                    targetNeed = NeedType.Comfort;
                    break;
                default:
                    Debug.LogError("Unknown Need: " + needName);
                    return;
            }

            var need = GetNeedByType(targetNeed);
            need.SetNeed(amount);
        }
        
        [Command("set_need", "The supported needs are Food, Energy, Fun, Beauty, Comfort", MonoTargetType.All)]
        private void CMD_SetNeed(string firstname, string needName, int amount)
        {
            if(_kinling.RuntimeData.Firstname == firstname)
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
                    case "Fun":
                        targetNeed = NeedType.Fun;
                        break;
                    case "Beauty":
                        targetNeed = NeedType.Beauty;
                        break;
                    case "Comfort":
                        targetNeed = NeedType.Comfort;
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
        Fun = 2,
        Beauty = 3,
        Comfort = 4,
    }
}
