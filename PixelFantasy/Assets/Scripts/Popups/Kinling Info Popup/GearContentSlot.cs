using System;
using Items;
using ScriptableObjects;
using UnityEngine;
using UnityEngine.UI;

namespace Popups.Kinling_Info_Popup
{
    public class GearContentSlot : MonoBehaviour
    {
        [SerializeField] private GameObject _slotIcon;
        [SerializeField] private Image _itemIcon;
        [SerializeField] private GameObject _selector;
        [SerializeField] private EquipmentType _equipmentType;

        public Action<EquipmentType, GearContentSlot> OnPressedCallback;
        
        private EquipmentState _equipmentState;

        public void AssignEquipment(EquipmentState state)
        {
            _equipmentState = state;
            Refresh();
        }

        private void Refresh()
        {
            if (_equipmentState.EquipmentData != null)
            {
                _slotIcon.SetActive(false);
                _itemIcon.gameObject.SetActive(true);
                _itemIcon.sprite = _equipmentState.EquipmentData.ItemSprite;
            }
            else
            {
                _slotIcon.SetActive(true);
                _itemIcon.gameObject.SetActive(false);
            }
        }
        
        public void OnPressed()
        {
            if (OnPressedCallback != null)
            {
                OnPressedCallback.Invoke(_equipmentType, this);
            }
        }

        public void DisplaySelected(bool isSelected)
        {
            _selector.SetActive(isSelected);
        }
    }
}
