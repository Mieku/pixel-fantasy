using System;
using HUD;
using UnityEngine;

namespace Systems.Currency.Scripts
{
    public class CurrencyHUD : MonoBehaviour
    {
        [SerializeField] private CurrencyDisplay _glimraDisplay;

        private void Awake()
        {
            GameEvents.OnGlimraTotalChanged += GameEvent_OnGlimraTotalChanged;
        }

        private void OnDestroy()
        {
            GameEvents.OnGlimraTotalChanged -= GameEvent_OnGlimraTotalChanged;
        }

        private void GameEvent_OnGlimraTotalChanged(int totalGlimra)
        {
            _glimraDisplay.UpdateAmount(totalGlimra);
        }
    }
}
