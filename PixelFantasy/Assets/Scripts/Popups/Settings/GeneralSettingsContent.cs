using System;
using Player;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Popups.Settings
{
    public class GeneralSettingsContent : SettingsContent
    {
        [SerializeField] private TMP_Dropdown _autosaveFreqDD;
        [SerializeField] private Slider _maxAutosavesSlider;
        [SerializeField] private TextMeshProUGUI _maxAutosavesCount;
        
        public override void OnShow()
        {
            // Autosave frequency
            RefreshAutosaveFrequencyDisplay();
            
            // Max Autosaves
            _maxAutosavesSlider.value = PlayerSettings.MaxAutoSaves;
            _maxAutosavesCount.text = (int)PlayerSettings.MaxAutoSaves + "";
        }

        private void RefreshAutosaveFrequencyDisplay()
        {
            var freq = PlayerSettings.AutoSaveFrequency;

            switch (freq)
            {
                case 0:
                    _autosaveFreqDD.value = 0;
                    break;
                case 1:
                    _autosaveFreqDD.value = 1;
                    break;
                case 2:
                    _autosaveFreqDD.value = 2;
                    break;
                case 3:
                    _autosaveFreqDD.value = 3;
                    break;
                default:
                    _autosaveFreqDD.value = 0;
                    break;
            }
        }

        public void OnAutosaveFreqDDChanged(int value)
        {
            PlayerSettings.AutoSaveFrequency = value;
        }

        public void OnMaxAutosavesChanged(Single value)
        {
            PlayerSettings.MaxAutoSaves = (int)value;
            _maxAutosavesCount.text = (int)value + "";
        }
    }
}
