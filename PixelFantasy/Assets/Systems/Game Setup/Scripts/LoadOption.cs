using System;
using System.Globalization;
using DataPersistence;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Systems.Game_Setup.Scripts
{
    public class LoadOption : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _saveNameText;
        [SerializeField] private TextMeshProUGUI _gameDateText;
        [SerializeField] private TextMeshProUGUI _saveDateText;
        [SerializeField] private TextMeshProUGUI _versionText;
        [SerializeField] private Image _bgImg;
        [SerializeField] private Sprite _activeBG;
        [SerializeField] private Sprite _defaultBG;
        
        public SaveHeader SaveHeader;

        private Action<SaveHeader> _onLoadOptionSelected;

        public void Init(SaveHeader saveHeader, Action<SaveHeader> onLoadOptionSelected)
        {
            SaveHeader = saveHeader;
            _onLoadOptionSelected = onLoadOptionSelected;
            
            _saveNameText.text = SaveHeader.SaveName;
            _gameDateText.text = SaveHeader.GameDate;
            _saveDateText.text = $"Saved: {SaveHeader.SaveDate.ToString("MM/dd/yyyy h:mm tt", CultureInfo.CurrentCulture)}";
            _versionText.text = $"Version: {SaveHeader.GameVersion}";
            
            SetHighlight(false);
        }

        public void OnOptionPressed()
        {
            _onLoadOptionSelected?.Invoke(SaveHeader);
        }

        public void SetHighlight(bool showHighlight)
        {
            if (showHighlight)
            {
                _bgImg.sprite = _activeBG;
            }
            else
            {
                _bgImg.sprite = _defaultBG;
            }
        }
    }
}
