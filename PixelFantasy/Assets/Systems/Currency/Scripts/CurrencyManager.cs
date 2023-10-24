using Managers;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Systems.Currency.Scripts
{
    public class CurrencyManager : Singleton<CurrencyManager>
    {
        [SerializeField] private Sprite _glimraIcon;

        private int _totalGlimra;

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

        [Button("Add Glimra")]
        public void DebugAddGlimra()
        {
            AddGlimra(10);
        }

        [Button("Remove Glimra")]
        public void DebugRemoveGlimra()
        {
            RemoveGlimra(5);
        }
    }
}
