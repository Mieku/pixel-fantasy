using System;
using Data.Item;
using Items;
using Sirenix.OdinInspector;
using Systems.Details.Components;
using TMPro;
using UnityEngine;

namespace Systems.Details.Generic_Details.Scripts
{
    public class StorageSettingsDetails : MonoBehaviour
    {
        [BoxGroup("Controls"), SerializeField] private TMP_InputField _searchInput;
        [BoxGroup("Controls"), SerializeField] private RangeSlider _durabilitySlider;
        [BoxGroup("Controls"), SerializeField] private TextMeshProUGUI _durabilityDetails;
        [BoxGroup("Controls"), SerializeField] private RangeSlider _qualitySlider;
        [BoxGroup("Controls"), SerializeField] private TextMeshProUGUI _qualityDetails;
        [BoxGroup("Controls"), SerializeField] private TMP_Dropdown _priorityDropdown;
        
        private IStorage _storage;
        private StoragePlayerSettings _settings => _storage.PlayerSettings;
        
        public void Show(IStorage storage)
        {
            gameObject.SetActive(true);
            _storage = storage;
            
            Refresh();
        }

        public void Hide()
        {
            gameObject.SetActive(false);
            _storage = null;
        }

        private void Refresh()
        {
            RefreshDurabilityDisplay();
            RefreshQualityDisplay();
            RefreshPriorityDisplay();
        }

        #region Controls

        public void OnAllowAllPressed()
        {
            
        }

        public void OnClearAllPressed()
        {
            
        }

        public void OnDurabilitySliderChanged(float min, float max)
        {
            _settings.DurabilityRange.x = (int)min;
            _settings.DurabilityRange.y = (int)max;
            
            _durabilityDetails.text = $"{_settings.DurabilityMin}%  -  {_settings.DurabilityMax}%";
        }

        public void OnQualitySliderChanged(float min, float max)
        {
            _settings.QualityRange.x = (int)min;
            _settings.QualityRange.y = (int)max;
            
            _qualityDetails.text = $"{_settings.QualityMin.GetDescription()}  -  {_settings.QualityMax.GetDescription()}";
        }

        public void OnSearchCharInput(string value)
        {
            
        }

        public void OnPriorityChanged(int value)
        {
            _settings.UsePriority = (EUsePriority)value;
        }

        private void RefreshDurabilityDisplay()
        {
            _durabilitySlider.SetValues(_settings.DurabilityRange.x, _settings.DurabilityRange.y, false);
            _durabilityDetails.text = $"{_settings.DurabilityMin}%  -  {_settings.DurabilityMax}%";
        }

        private void RefreshQualityDisplay()
        {
            _qualitySlider.SetValues(_settings.QualityRange.x, _settings.QualityRange.y, false);
            _qualityDetails.text = $"{_settings.QualityMin.GetDescription()}  -  {_settings.QualityMax.GetDescription()}";
        }

        private void RefreshPriorityDisplay()
        {
            int currentPriority = (int)_settings.UsePriority;
            _priorityDropdown.SetValueWithoutNotify(currentPriority);
        }

        #endregion
    }
}
