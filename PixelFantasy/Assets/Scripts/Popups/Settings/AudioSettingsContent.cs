using System;
using Player;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Popups.Settings
{
    public class AudioSettingsContent : SettingsContent
    {
        [SerializeField] private Slider _masterVolumeSlider;
        [SerializeField] private TextMeshProUGUI _masterVolumeDisplay;
        
        [SerializeField] private Slider _musicVolumeSlider;
        [SerializeField] private TextMeshProUGUI _musicVolumeDisplay;
        
        [SerializeField] private Slider _effectsVolumeSlider;
        [SerializeField] private TextMeshProUGUI _effectsVolumeDisplay;
        
        [SerializeField] private Slider _ambientVolumeSlider;
        [SerializeField] private TextMeshProUGUI _ambientVolumeDisplay;
        
        public override void OnShow()
        {
            var masterVol = $"{(PlayerSettings.MasterVolume * 100):0}%";
            var musicVol = $"{(PlayerSettings.MusicVolume * 100):0}%";
            var effectsVol = $"{(PlayerSettings.EffectsVolume * 100):0}%";
            var ambientVol = $"{(PlayerSettings.AmbientVolume * 100):0}%";

            _masterVolumeSlider.value = PlayerSettings.MasterVolume;
            _masterVolumeDisplay.text = masterVol;
            
            _musicVolumeSlider.value = PlayerSettings.MusicVolume;
            _musicVolumeDisplay.text = musicVol;
            
            _effectsVolumeSlider.value = PlayerSettings.EffectsVolume;
            _effectsVolumeDisplay.text = effectsVol;
            
            _ambientVolumeSlider.value = PlayerSettings.AmbientVolume;
            _ambientVolumeDisplay.text = ambientVol;
        }

        public void OnMasterVolumeChanged(Single value)
        {
            PlayerSettings.MasterVolume = value;
            
            var masterVol = $"{(value * 100):0}%";
            _masterVolumeDisplay.text = masterVol;
        }
        
        public void OnMusicVolumeChanged(Single value)
        {
            PlayerSettings.MusicVolume = value;
            
            var masterVol = $"{(value * 100):0}%";
            _musicVolumeDisplay.text = masterVol;
        }
        
        public void OnEffectsVolumeChanged(Single value)
        {
            PlayerSettings.EffectsVolume = value;
            
            var masterVol = $"{(value * 100):0}%";
            _effectsVolumeDisplay.text = masterVol;
        }
        
        public void OnAmbientVolumeChanged(Single value)
        {
            PlayerSettings.AmbientVolume = value;
            
            var masterVol = $"{(value * 100):0}%";
            _ambientVolumeDisplay.text = masterVol;
        }
    }
}
