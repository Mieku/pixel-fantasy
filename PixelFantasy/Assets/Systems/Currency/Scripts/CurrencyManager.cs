using System;
using Managers;
using Sirenix.OdinInspector;
using Systems.Notifications.Scripts;
using UnityEngine;
using UnityEngine.Serialization;

namespace Systems.Currency.Scripts
{
    public class CurrencyManager : Singleton<CurrencyManager>
    {
        [SerializeField] private Sprite _coinsIcon;
        [SerializeField] private int _timeToRequestCoins;

        private int _totalCoins;

        public int TotalCoins => _totalCoins;

        protected override void Awake()
        {
            base.Awake();

            GameEvents.HourTick += GameEvent_HourTick;
        }

        private void Start()
        {
            AddCoins(100); // TODO: Remove this!
        }

        private void OnDestroy()
        {
            GameEvents.HourTick -= GameEvent_HourTick;
        }

        private void GameEvent_HourTick(int hour)
        {
            if (hour == _timeToRequestCoins)
            {
                CollectIncome();
            }
        }

        private void CollectIncome()
        {
            int income = GetDailyIncomeAmount();
            _totalCoins += income;
            
            GameEvents.Trigger_OnCoinsTotalChanged();

            if (income < 0)
            {
                NotificationManager.Instance.CreateGeneralLog($"Today's Income: {income}<sprite name=\"Coins\">", LogData.ELogType.Negative);
            }
            else
            {
                NotificationManager.Instance.CreateGeneralLog($"Today's Income: +{income}<sprite name=\"Coins\">", LogData.ELogType.Positive);
            }
        }

        public int GetDailyIncomeAmount()
        {
            int incomeAmount = 0;
            // Get Income From Kinlings
            foreach (var kinling in UnitsManager.Instance.AllKinlings)
            {
                incomeAmount += kinling.DailyIncome();
            }
            
            foreach (var building in BuildingsManager.Instance.AllBuildings)
            {
                incomeAmount += building.DailyUpkeep();
            }

            return incomeAmount;
        }

        public string GetIncomeBreakdown()
        {
            int kinlingIncome = 0;
            foreach (var kinling in UnitsManager.Instance.AllKinlings)
            {
                kinlingIncome += kinling.DailyIncome();
            }

            int buildingUpkeep = 0;
            foreach (var building in BuildingsManager.Instance.AllBuildings)
            {
                buildingUpkeep += building.DailyUpkeep();
            }

            string result = $"Income: +{kinlingIncome}\nBuilding Upkeep: {buildingUpkeep}";
            return result;
        }

        public int GetTotalCoins()
        {
            return _totalCoins;
        }

        public void AddCoins(int amountToAdd)
        {
            _totalCoins += amountToAdd;
            
            GameEvents.Trigger_OnCoinsTotalChanged();
        }

        /// <summary>
        /// Returns false if can't remove it all, doesn't remove any if it can't
        /// </summary>
        public bool RemoveCoins(int amountToRemove)
        {
            if (_totalCoins < amountToRemove)
            {
                return false;
            }

            _totalCoins -= amountToRemove;
            
            GameEvents.Trigger_OnCoinsTotalChanged();

            return true;
        }

        public bool CanAfford(int amountToCheck)
        {
            return amountToCheck <= _totalCoins;
        }

        [Button("Add 10 Coins")]
        public void DebugAddCoins()
        {
            AddCoins(10);
        }

        [Button("Remove 5 Coins")]
        public void DebugRemoveCoins()
        {
            RemoveCoins(5);
        }
    }
}
