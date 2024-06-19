using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Systems.Game_Setup.Scripts
{
    public class ChooseWorldPanel : MonoBehaviour
    {
        public OverworldPreview.OverworldSelectionData SelectionData;

        [SerializeField] private OverworldPreview _overworldPreview;
        [SerializeField] private TilePlanner _tilePlanner;
        [SerializeField] private RawImage _localMap;

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
            yield return new WaitForSeconds(2);
            _overworldPreview.IsEnabled = true;
        }

        public void GetSelectionData(OverworldPreview.OverworldSelectionData data)
        {
            SelectionData = data;
            //_overworldPreview.SetPlacementLocked(true);

            _tilePlanner.GenerateArea(data, OnTextureGenerated);
        }

        private void OnTextureGenerated(Texture2D texture)
        {
            if (_localMap != null)
            {
                _localMap.gameObject.SetActive(true);
                _localMap.texture = texture;

                // Force the UI to update
                Canvas.ForceUpdateCanvases();
            }
        }
    }
}