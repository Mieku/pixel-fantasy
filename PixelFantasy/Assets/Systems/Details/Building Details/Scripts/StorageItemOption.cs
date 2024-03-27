// using System;
// using ScriptableObjects;
// using UnityEngine;
// using UnityEngine.UI;
//
// namespace Systems.Details.Building_Details.Scripts
// {
//     public class StorageItemOption : MonoBehaviour
//     {
//         [SerializeField] private Image _itemIcon;
//         [SerializeField] private GameObject _selectedHandle;
//         [SerializeField] private GameObject _ruleIcon;
//
//         private ItemSettings _itemSettings;
//         private Action<ItemSettings> _onPressedCallback;
//         private bool _isSelected;
//
//         public ItemSettings ItemSettings => _itemSettings;
//         
//         public void Init(ItemSettings itemSettings, bool isSelected, bool hasRule, Action<ItemSettings> onPressedCallback)
//         {
//             _itemSettings = itemSettings;
//             _onPressedCallback = onPressedCallback;
//
//             _itemIcon.sprite = _itemSettings.ItemSprite;
//             SetSelected(isSelected);
//             SetHasRule(hasRule);
//         }
//
//         public void SetSelected(bool isSelected)
//         {
//             _isSelected = isSelected;
//             _selectedHandle.SetActive(_isSelected);
//         }
//
//         public void SetHasRule(bool hasRule)
//         {
//             _ruleIcon.SetActive(hasRule);
//         }
//         
//         public void OnPressed()
//         {
//             _onPressedCallback.Invoke(_itemSettings);
//         }
//     }
// }
