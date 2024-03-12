// using System;
// using Items;
// using ScriptableObjects;
// using UnityEngine;
// using UnityEngine.Serialization;
// using UnityEngine.UI;
//
// namespace Popups.Kinling_Info_Popup
// {
//     public class GearContentSlot : MonoBehaviour
//     {
//         [SerializeField] private GameObject _slotIcon;
//         [SerializeField] private Image _itemIcon;
//         [SerializeField] private GameObject _selector;
//         [FormerlySerializedAs("_equipmentType")] [SerializeField] private GearType _gearType;
//
//         public Action<GearType, GearContentSlot> OnPressedCallback;
//         
//         private GearState _gearState;
//
//         public void AssignEquipment(GearState state)
//         {
//             _gearState = state;
//             Refresh();
//         }
//
//         private void Refresh()
//         {
//             if (_gearState != null && _gearState.GearSettings != null)
//             {
//                 _slotIcon.SetActive(false);
//                 _itemIcon.gameObject.SetActive(true);
//                 _itemIcon.sprite = _gearState.GearSettings.ItemSprite;
//             }
//             else
//             {
//                 _slotIcon.SetActive(true);
//                 _itemIcon.gameObject.SetActive(false);
//             }
//         }
//         
//         public void OnPressed()
//         {
//             if (OnPressedCallback != null)
//             {
//                 OnPressedCallback.Invoke(_gearType, this);
//             }
//         }
//
//         public void DisplaySelected(bool isSelected)
//         {
//             _selector.SetActive(isSelected);
//         }
//     }
// }
