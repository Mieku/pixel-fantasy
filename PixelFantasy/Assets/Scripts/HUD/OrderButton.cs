// using System;
// using HUD.Tooltip;
// using UnityEngine;
// using UnityEngine.PlayerLoop;
// using UnityEngine.UI;
//
// namespace HUD
// {
//     public class OrderButton : MonoBehaviour
//     {
//         [SerializeField] private Image _icon;
//         [SerializeField] private Image _slotBG;
//         [SerializeField] private Sprite _defaultBG, _activeBG;
//
//         private Action _onPressed;
//         private TooltipTrigger _tooltipTrigger;
//
//         public void Init(Sprite icon, Action onPressed, bool isActive, string orderName)
//         {
//             _icon.sprite = icon;
//             _onPressed = onPressed;
//             SetActive(isActive);
//             _tooltipTrigger = GetComponent<TooltipTrigger>();
//             _tooltipTrigger.Content = orderName;
//         }
//
//         public void OnButtonPressed()
//         {
//             _onPressed.Invoke();
//         }
//
//         public void SetActive(bool isActive)
//         {
//             if (isActive)
//             {
//                 _slotBG.sprite = _activeBG;
//             }
//             else
//             {
//                 _slotBG.sprite = _defaultBG;
//             }
//         }
//     }
// }
