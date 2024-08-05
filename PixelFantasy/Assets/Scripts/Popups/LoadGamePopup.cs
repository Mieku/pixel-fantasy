using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using DataPersistence;
using Player;
using Sirenix.OdinInspector;
using Systems.Game_Setup.Scripts;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Popups
{
    public class LoadGamePopup : Popup<LoadGamePopup>
    {
        [SerializeField, BoxGroup("Current Selection")] private TMP_InputField _saveNameInputField;
        [SerializeField, BoxGroup("Current Selection")] private TextMeshProUGUI _saveDateText;
        [SerializeField, BoxGroup("Current Selection")] private TextMeshProUGUI _versionText;
        [SerializeField, BoxGroup("Current Selection")] private RawImage _screenshotImg;
        [SerializeField, BoxGroup("Current Selection")] private GameObject _hasSelectedSaveHandle;
        [SerializeField, BoxGroup("Current Selection")] private GameObject _noSelectedSaveHandle;
        
        [SerializeField, BoxGroup("Load Options")] private Transform _loadOptionsParent;
        [SerializeField, BoxGroup("Load Options")] private LoadOption _loadOptionPrefab;
        
        [SerializeField, BoxGroup("Misc")] private Toggle _autoSaveToggle;
        
        private Action<DataPersistenceManager.SaveData> _onLoadSelectedCallback;
        private Action _onLoadCancelledCallback;
        private SaveHeader _selectedSaveHeader;
        private List<LoadOption> _displayedLoadOptions = new List<LoadOption>();
        
        public static void Show(Action<DataPersistenceManager.SaveData> onLoadSelected, Action onCancelled)
        {
            Open(() => Instance.Refresh(onLoadSelected, onCancelled), true);
        }

        private void Refresh(Action<DataPersistenceManager.SaveData> onLoadSelected, Action onCancelled)
        {
            gameObject.SetActive(true);
            
            _onLoadSelectedCallback = onLoadSelected;
            _onLoadCancelledCallback = onCancelled;
            
            _loadOptionPrefab.gameObject.SetActive(false);
            
            _autoSaveToggle.SetIsOnWithoutNotify(PlayerSettings.AutoSaveEnabled);
            
            RefreshLoadOptions();
            OnLoadOptionSelected(_displayedLoadOptions.First().SaveHeader);
        }

        private void RefreshLoadOptions()
        {
            ClearLoadOptions();
            var allSaveHeaders = DataPersistenceManager.Instance.GetAllSaveHeaders();
            foreach (var header in allSaveHeaders)
            {
                var loadOption = Instantiate(_loadOptionPrefab, _loadOptionsParent);
                loadOption.Init(header, OnLoadOptionSelected);
                loadOption.gameObject.SetActive(true);
                _displayedLoadOptions.Add(loadOption);
            }
        }

        private void OnLoadOptionSelected(SaveHeader saveHeader)
        {
            foreach (var loadOption in _displayedLoadOptions)
            {
                if (loadOption.SaveHeader == saveHeader)
                {
                    loadOption.SetHighlight(true);       
                }
                else
                {
                    loadOption.SetHighlight(false);
                }
            }
            
            SelectSaveData(saveHeader);
        }

        private void SelectSaveData(SaveHeader saveHeader)
        {
            _selectedSaveHeader = saveHeader;

            if (saveHeader != null)
            {
                ToggleDisplayCurrentSaveDetails(true);
                _saveNameInputField.SetTextWithoutNotify(_selectedSaveHeader.SaveName);
                _saveDateText.text = $"Saved: {_selectedSaveHeader.SaveDate.ToString("MM/dd/yyyy h:mm tt", CultureInfo.CurrentCulture)}";
                _versionText.text = $"Version: {_selectedSaveHeader.GameVersion}";
                LoadScreenshot(_selectedSaveHeader.ScreenshotPath);
            }
            else
            {
                ToggleDisplayCurrentSaveDetails(false);
            }
        }
        
        private void ToggleDisplayCurrentSaveDetails(bool show)
        {
            if (show)
            {
                _hasSelectedSaveHandle.SetActive(true);
                _noSelectedSaveHandle.SetActive(false);
            }
            else
            {
                _hasSelectedSaveHandle.SetActive(false);
                _noSelectedSaveHandle.SetActive(true);
            }
        }
        
        private void LoadScreenshot(string filePath)
        {
            if (File.Exists(filePath))
            {
                byte[] fileData = File.ReadAllBytes(filePath);
                Texture2D texture = new Texture2D(2, 2);
                texture.LoadImage(fileData);

                _screenshotImg.gameObject.SetActive(true);
                _screenshotImg.texture = texture;
            }
            else
            {
                _screenshotImg.gameObject.SetActive(false);
                Debug.LogError("Screenshot file not found: " + filePath);
            }
        }

        private void ClearLoadOptions()
        {
            foreach (var loadOption in _displayedLoadOptions)
            {
                Destroy(loadOption.gameObject);
            }
            _displayedLoadOptions.Clear();
        }

        #region Button Hooks

        public void OnCancelPressed()
        {
            _onLoadCancelledCallback?.Invoke();
            Hide();
        }

        public void OnLoadPressed()
        {
            if (_selectedSaveHeader == null) return;
            Hide();
            
            var saveData = DataPersistenceManager.Instance.LoadSaveFromHeader(_selectedSaveHeader);
            _onLoadSelectedCallback?.Invoke(saveData);
        }

        public void OnDeletePressed()
        {
            if(_selectedSaveHeader == null) return; 
            
            ConfirmationPopup.Show( "Are you sure you want to delete this save?" ,(confirmation) =>
            {
                if (confirmation)
                {
                    DataPersistenceManager.Instance.DeleteSave(_selectedSaveHeader);
                    RefreshLoadOptions();

                    if (_displayedLoadOptions.Count > 0)
                    {
                        OnLoadOptionSelected(_displayedLoadOptions.First().SaveHeader);
                    }
                    else
                    {
                        OnLoadOptionSelected(null);
                    }
                }
            });
        }

        public void OnAutoSaveToggled(bool value)
        {
            PlayerSettings.AutoSaveEnabled = value;
        }
        
        public void OnSaveNameChanged(string value)
        {
            DataPersistenceManager.Instance.ChangeSaveName(_selectedSaveHeader, value, (updatedHeader) =>
            {
                RefreshLoadOptions();
                OnLoadOptionSelected(updatedHeader);
            });
        }

        #endregion

        public override void OnBackPressed()
        {
            OnCancelPressed();
        }
    }
}
