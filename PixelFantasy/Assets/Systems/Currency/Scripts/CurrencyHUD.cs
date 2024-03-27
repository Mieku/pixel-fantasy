// using System;
// using HUD;
// using UnityEngine;
// using UnityEngine.Serialization;
//
// namespace Systems.Currency.Scripts
// {
//     public class CurrencyHUD : MonoBehaviour
//     {
//         [SerializeField] private CurrencyDisplay _coinsDisplay;
//
//         private void Awake()
//         {
//             GameEvents.OnCoinsTotalChanged += GameEvent_OnCoinsTotalChanged;
//             GameEvents.OnCoinsIncomeChanged += GameEvent_OnCoinsIncomeChanged;
//         }
//
//         private void OnDestroy()
//         {
//             GameEvents.OnCoinsTotalChanged -= GameEvent_OnCoinsTotalChanged;
//             GameEvents.OnCoinsIncomeChanged -= GameEvent_OnCoinsIncomeChanged;
//         }
//
//         private void GameEvent_OnCoinsTotalChanged()
//         {
//             _coinsDisplay.UpdateDisplay(true);
//         }
//
//         private void GameEvent_OnCoinsIncomeChanged()
//         {
//             _coinsDisplay.UpdateDisplay(false);
//         }
//     }
// }
