using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using TMPro;
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
        
        [SerializeField] private ResourceThresholds _treesThresholds;
        [SerializeField] private ResourceThresholds _stoneThresholds;
        [SerializeField] private ResourceThresholds _spaceThresholds;
        [SerializeField] private ResourceThresholds _wildlifeThresholds;

        [SerializeField] private TextMeshProUGUI _treesPlentifulText;
        [SerializeField] private Image _treesFill;
        [SerializeField] private TextMeshProUGUI _stonePlentifulText;
        [SerializeField] private Image _stoneFill;
        [SerializeField] private TextMeshProUGUI _spacePlentifulText;
        [SerializeField] private Image _spaceFill;
        [SerializeField] private TextMeshProUGUI _monstersPlentifulText;
        [SerializeField] private Image _monstersFill;
        
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

        private void OnTextureGenerated(Texture2D texture, List<TileWorldCreatorAsset.BlueprintLayerData> blueprintLayers, LocationStats stats)
        {
            if (_localMap != null)
            {
                _localMap.gameObject.SetActive(true);
                _localMap.texture = texture;
                BlueprintLayers = blueprintLayers;
                
                RefreshAreaResources(stats);

                // Force the UI to update
                Canvas.ForceUpdateCanvases();
            }
        }

        private void RefreshAreaResources(LocationStats stats)
        {
            var treesVeg = _treesThresholds.DetermineThreshold(stats.TreesVeg);
            var stoneOre = _stoneThresholds.DetermineThreshold(stats.Mountains);
            var space = _spaceThresholds.DetermineThreshold(stats.AvailableSpace);
            var monsters = ResourceThresholds.EResourceThreshold.None; // TODO: Build for monsters

            _treesPlentifulText.text = treesVeg.GetDescription();
            _stonePlentifulText.text = stoneOre.GetDescription();
            _spacePlentifulText.text = space.GetDescription();
            _monstersPlentifulText.text = monsters.GetDescription();

            _treesFill.fillAmount = _treesThresholds.DetermineBarFillPercent(stats.TreesVeg);
            _stoneFill.fillAmount = _stoneThresholds.DetermineBarFillPercent(stats.Mountains);
            _spaceFill.fillAmount = _spaceThresholds.DetermineBarFillPercent(stats.AvailableSpace);
            _monstersFill.fillAmount = 0f; // TODO: Add then when monsters are made
        }
    }
    
    [Serializable]
    public class ResourceThresholds
    {
        public float SomeMinimum;
        public float FairMinimum;
        public float LotsMinimum;

        public EResourceThreshold DetermineThreshold(float fillPercent)
        {
            if (fillPercent < SomeMinimum) return EResourceThreshold.None;

            if (fillPercent < FairMinimum) return EResourceThreshold.Some;

            if (fillPercent < LotsMinimum) return EResourceThreshold.Fair;

            return EResourceThreshold.Lots;
        }

        public float DetermineBarFillPercent(float fillPercent)
        {
            var threshold = DetermineThreshold(fillPercent);
            
            switch (threshold)
            {
                case EResourceThreshold.None:
                    return 0f;
                case EResourceThreshold.Some:
                    return .3f;
                case EResourceThreshold.Fair:
                    return .6f;
                case EResourceThreshold.Lots:
                    return 1f;
                default:
                    throw new ArgumentOutOfRangeException(nameof(threshold), threshold, null);
            }
        }

        public enum EResourceThreshold
        {
            [Description("None")] None,
            [Description("Some")] Some,
            [Description("Fair")] Fair,
            [Description("Lots")] Lots
        }
    }
}