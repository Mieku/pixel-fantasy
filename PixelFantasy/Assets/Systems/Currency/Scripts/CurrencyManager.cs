using System;
using Managers;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Systems.Currency.Scripts
{
    public class CurrencyManager : Singleton<CurrencyManager>
    {
        [SerializeField] private Sprite _glimraIcon;
        [SerializeField] private int _timeToRequestGlimra;

        private int _totalGlimra;

        public int TotalGlimra => _totalGlimra;

        protected override void Awake()
        {
            base.Awake();

            GameEvents.HourTick += GameEvent_HourTick;
        }

        private void OnDestroy()
        {
            GameEvents.HourTick -= GameEvent_HourTick;
        }

        private void GameEvent_HourTick(int hour)
        {
            if (hour == _timeToRequestGlimra)
            {
                GameEvents.Trigger_OnGlimraDue();
            }
        }

        public void AddGlimra(int amountToAdd)
        {
            _totalGlimra += amountToAdd;
            
            GameEvents.Trigger_OnGlimraTotalChanged(_totalGlimra);
        }

        /// <summary>
        /// Returns false if can't remove it all, doesn't remove any if it can't
        /// </summary>
        public bool RemoveGlimra(int amountToRemove)
        {
            if (_totalGlimra < amountToRemove)
            {
                return false;
            }

            _totalGlimra -= amountToRemove;
            
            GameEvents.Trigger_OnGlimraTotalChanged(_totalGlimra);

            return true;
        }

        [Button("Add 10 Glimra")]
        public void DebugAddGlimra()
        {
            AddGlimra(10);
        }

        [Button("Remove 5 Glimra")]
        public void DebugRemoveGlimra()
        {
            RemoveGlimra(5);
        }
    }
}
