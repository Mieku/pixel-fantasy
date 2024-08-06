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
    public class SaveGamePopup : Popup<SaveGamePopup>
    {
        [SerializeField, BoxGroup("Current Selection")] private TMP_InputField _saveNameInputField;
        [SerializeField, BoxGroup("Current Selection")] private TextMeshProUGUI _saveDateText;
        [SerializeField, BoxGroup("Current Selection")] private TextMeshProUGUI _versionText;
        [SerializeField, BoxGroup("Current Selection")] private RawImage _screenshotImg;
        [SerializeField, BoxGroup("Current Selection")] private GameObject _hasSelectedSaveHandle;
        [SerializeField, BoxGroup("Current Selection")] private GameObject _noSelectedSaveHandle;

        [SerializeField, BoxGroup("Load Options")] private TextMeshProUGUI _newSaveText;
        [SerializeField, BoxGroup("Load Options")] private Button _newSaveBtn;
        [SerializeField, BoxGroup("Load Options")] private TextMeshProUGUI _overwriteSaveText;
        [SerializeField, BoxGroup("Load Options")] private Button _overwriteSaveBtn;
        [SerializeField, BoxGroup("Load Options")] private Transform _loadOptionsParent;
        [SerializeField, BoxGroup("Load Options")] private LoadOption _loadOptionPrefab;
        
        private Action _onClosedCallback;
        private SaveHeader _selectedSaveHeader;
        private List<LoadOption> _displayedLoadOptions = new List<LoadOption>();
        
        public static void Show(Action onClosed)
        {
            Open(() => Instance.Refresh(onClosed), true);
        }

        private void Refresh(Action onClosed)
        {
            gameObject.SetActive(true);
            
            _onClosedCallback = onClosed;
            
            _loadOptionPrefab.gameObject.SetActive(false);
            
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
            _onClosedCallback?.Invoke();
            Hide();
        }

        public void OnOverwritePressed()
        {
            if (_selectedSaveHeader == null) return;
            _overwriteSaveBtn.interactable = false;
            
            ConfirmationPopup.Show("Are you sure you want to overwrite your save?", (confirmation) =>
            {
                if (confirmation)
                {
                    DataPersistenceManager.Instance.OverwriteSave(_selectedSaveHeader, OnOverwriteSaveProgress, (header) =>
                    {
                        RefreshLoadOptions();
                        OnLoadOptionSelected(header);
                    });
                }
            });
        }
        
        private void OnOverwriteSaveProgress(float progress)
        {
            string progressString = (progress * 100).ToString("F0") + "%"; // "F0" formats the number with 0 decimal places

            _overwriteSaveText.text = $"({progressString})";

            if (progress >= 0.999)
            {
                _overwriteSaveText.text = "Overwrite";
                _overwriteSaveBtn.interactable = true;
            }
        }

        public void OnNewSavePressed()
        {
            _newSaveBtn.interactable = false;
            StartCoroutine(DataPersistenceManager.Instance.SaveGameCoroutine(OnNewSaveProgress));
        }
        
        private void OnNewSaveProgress(float progress)
        {
            string progressString = (progress * 100).ToString("F0") + "%"; // "F0" formats the number with 0 decimal places

            _newSaveText.text = $"Saving ({progressString})";

            if (progress >= 0.999)
            {
                _newSaveText.text = "Create New Save";
                _newSaveBtn.interactable = true;
                
                RefreshLoadOptions();
                OnLoadOptionSelected(_displayedLoadOptions.First().SaveHeader);
            }
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
