using System.Collections;
using System.Collections.Generic;
using TWC;
using UnityEngine;
using UnityEngine.UI;

namespace Systems.Game_Setup.Scripts
{
    public class ChooseWorldPanel : MonoBehaviour
    {
        public OverworldPreview.OverworldSelectionData SelectionData;

        [SerializeField] private NewGameSection _newGameSection;
        [SerializeField] private OverworldPreview _overworldPreview;
        [SerializeField] private TilePlanner _tilePlanner;
        [SerializeField] private RawImage _localMap;
        [SerializeField] private Vector2 _defaultStartPos;
        
        public List<TileWorldCreatorAsset.BlueprintLayerData> BlueprintLayers;

        public void Show()
        {
            gameObject.SetActive(true);
            _localMap.gameObject.SetActive(false);
            StartCoroutine(EnablePreviewSequence());
        }

        public void Hide()
        {
            gameObject.SetActive(false);
            _overworldPreview.IsEnabled = false;
        }

        IEnumerator EnablePreviewSequence()
        {
            yield return new WaitForSeconds(0.25f);
            _overworldPreview.IsEnabled = true;
            if (SelectionData.Position == Vector2.zero)
            {
                _overworldPreview.SelectPosition(_defaultStartPos);
            }
        }
        
        public void OnBackPressed()
        {
            _newGameSection.OnBack();
        }

        public void OnContinuePressed()
        {
            _newGameSection.OnContinue();
        }

        public void GetSelectionData(OverworldPreview.OverworldSelectionData data)
        {
            SelectionData = data;

            _tilePlanner.GenerateArea(data, OnTextureGenerated);
        }

        private void OnTextureGenerated(Texture2D texture, List<TileWorldCreatorAsset.BlueprintLayerData> blueprintLayers)
        {
            if (_localMap != null)
            {
                _localMap.gameObject.SetActive(true);
                _localMap.texture = texture;
                BlueprintLayers = blueprintLayers;

                // Force the UI to update
                Canvas.ForceUpdateCanvases();
            }
        }
    }
}