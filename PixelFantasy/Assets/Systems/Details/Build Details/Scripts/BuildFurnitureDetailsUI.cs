using System.Collections.Generic;
using Controllers;
using DataPersistence;
using HUD;
using Managers;
using ScriptableObjects;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Systems.Details.Build_Details.Scripts
{
    public class BuildFurnitureDetailsUI : MonoBehaviour
    {
        [SerializeField] private GameObject _optionsSeperator;
        [SerializeField] private PanelLayoutRebuilder _layoutRebuilder;
        [SerializeField] private GameObject _currentSelectionHandle;
        [SerializeField] private TextMeshProUGUI _itemTitle;
        [SerializeField] private TextMeshProUGUI _itemDescription;
        [SerializeField] private Image _itemImage;
        [SerializeField] private TextMeshProUGUI _statsListText;
        [SerializeField] private TextMeshProUGUI _requiredCraftingLvl;
        [SerializeField] private ResourceCost _resourceCostPrefab;
        [SerializeField] private Transform _resourceCostParent;
        [SerializeField] private Color _unavailableColour;

        [SerializeField] private GameObject _materialOptionGroup;
        [SerializeField] private Transform _materialLayoutParent;
        [SerializeField] private TextMeshProUGUI _materialGroupHeader;
        [SerializeField] private VarientOptionBtn _materialVarientOptionBtnPrefab;

        [SerializeField] private GameObject _optionGroupSeperator;
        [SerializeField] private GameObject _optionalsHandle;
        
        [SerializeField] private GameObject _colourOptionGroup;
        [SerializeField] private Transform _colourLayoutParent;
        [SerializeField] private TextMeshProUGUI _colourGroupHeader;
        [SerializeField] private VarientOptionBtn _colourVarientOptionBtnPrefab;

        [SerializeField] private Transform _optionsLayout;
        [SerializeField] private SelectionOptionBtn _selectionOptionBtnPrefab;

        private List<ResourceCost> _displayedResourceCosts = new List<ResourceCost>();
        private List<VarientOptionBtn> _displayedMaterialOptions = new List<VarientOptionBtn>();
        private List<VarientOptionBtn> _displayedColourOptions = new List<VarientOptionBtn>();
        private List<SelectionOptionBtn> _displayedOptions = new List<SelectionOptionBtn>();
        private FurnitureSettings _selectedFurniture;
        private DyeSettings _selectedDye;
         
        public void Show(List<FurnitureSettings> options)
        {
            gameObject.SetActive(true);
            _currentSelectionHandle.SetActive(false);
            _optionsSeperator.SetActive(false);

            _selectedFurniture = null;
            
            DisplayOptions(options);
            RefreshLayout();
        }

        private void DisplayOptions(List<FurnitureSettings> options)
        {
            ClearOptions();
            _selectionOptionBtnPrefab.gameObject.SetActive(false);

            foreach (var option in options)
            {
                var optionBtn = Instantiate(_selectionOptionBtnPrefab, _optionsLayout);
                optionBtn.gameObject.SetActive(true);
                optionBtn.Init(option, OnOptionSelected);
                _displayedOptions.Add(optionBtn);
            }
        }

        private void OnOptionSelected(SelectionOptionBtn selectedOption)
        {
            foreach (var displayedOption in _displayedOptions)
            {
                displayedOption.Highlight(false);
            }
            selectedOption.Highlight(true);
            
            ShowOptionSelection(selectedOption.FurnitureSettings);
            RefreshLayout();
        }

        private void ClearOptions()
        {
            foreach (var displayedOption in _displayedOptions)
            {
                Destroy(displayedOption.gameObject);
            }
            _displayedOptions.Clear();
        }

        private void ShowOptionSelection(FurnitureSettings furnitureSettings)
        {
            _optionsSeperator.SetActive(true);
            _currentSelectionHandle.SetActive(true);
            _itemDescription.text = furnitureSettings.Description;

            // Material Options
            if (furnitureSettings.Varients is { Count: > 0 })
            {
                ShowMaterialVarients(furnitureSettings);
            }
            else
            {
                _materialOptionGroup.SetActive(false);
                
                // Use default
                ApplyDefault(furnitureSettings);
            }
            
            // Colour Options
            if (furnitureSettings.ColourOptions != null && furnitureSettings.ColourOptions.DyePalettes.Count > 0)
            {
                ShowColourVarients(furnitureSettings);
            }
            else
            {
                _optionGroupSeperator.SetActive(false);
                _colourOptionGroup.SetActive(false);
            }

            if (!_colourOptionGroup.activeSelf && !_materialOptionGroup.activeSelf)
            {
                _optionalsHandle.SetActive(false);
            }
            else
            {
                _optionalsHandle.SetActive(true);
            }
        }

        private void ShowMaterialVarients(FurnitureSettings furnitureSettings)
        {
            _materialOptionGroup.SetActive(true);
            _materialGroupHeader.text = "Material";
            _materialVarientOptionBtnPrefab.gameObject.SetActive(false);
            ClearMaterialVarients();

            var defaultOption = Instantiate(_materialVarientOptionBtnPrefab, _materialLayoutParent);
            defaultOption.gameObject.SetActive(true);
            defaultOption.InitDefault(furnitureSettings, OnDefaultMaterialSelected);
            _displayedMaterialOptions.Add(defaultOption);
            
            foreach (var varient in furnitureSettings.Varients)
            {
                var materialVarient = Instantiate(_materialVarientOptionBtnPrefab, _materialLayoutParent);
                materialVarient.gameObject.SetActive(true);
                materialVarient.Init(varient, furnitureSettings, OnMaterialVarientSelected);
                _displayedMaterialOptions.Add(materialVarient);
            }
            
            OnDefaultMaterialSelected(defaultOption, furnitureSettings);
        }

        private void ClearMaterialVarients()
        {
            foreach (var materialOption in _displayedMaterialOptions)
            {
                Destroy(materialOption.gameObject);
            }
            _displayedMaterialOptions.Clear();
        }

        private void OnMaterialVarientSelected(VarientOptionBtn selectedBtn, FurnitureSettings furnitureSettings)
        {
            foreach (var materialOption in _displayedMaterialOptions)
            {
                materialOption.RemoveHighlight();
            }
            selectedBtn.ShowHighlight();
            
            ApplyVarient(selectedBtn.Variant);
            RefreshLayout();
        }

        private void OnDefaultMaterialSelected(VarientOptionBtn selectedBtn, FurnitureSettings furnitureSettings)
        {
            foreach (var materialOption in _displayedMaterialOptions)
            {
                materialOption.RemoveHighlight();
            }
            selectedBtn.ShowHighlight();

            ApplyDefault(furnitureSettings);
            RefreshLayout();
        }

        private void ShowColourVarients(FurnitureSettings furnitureSettings)
        {
            _optionGroupSeperator.SetActive(true);
            _colourOptionGroup.SetActive(true);
            _colourGroupHeader.text = furnitureSettings.ColourOptions.ColourOptionsHeader;
            _colourVarientOptionBtnPrefab.gameObject.SetActive(false);
            ClearColourVarients();

            if (furnitureSettings.DefaultDye != null)
            {
                var colourVarient = Instantiate(_colourVarientOptionBtnPrefab, _colourLayoutParent);
                colourVarient.gameObject.SetActive(true);
                colourVarient.Init(furnitureSettings.DefaultDye, furnitureSettings, OnColourVarientSelected);
                _displayedColourOptions.Add(colourVarient);
            }

            foreach (var colourOption in furnitureSettings.ColourOptions.DyePalettes)
            {
                var colourVarient = Instantiate(_colourVarientOptionBtnPrefab, _colourLayoutParent);
                colourVarient.gameObject.SetActive(true);
                colourVarient.Init(colourOption, furnitureSettings, OnColourVarientSelected);
                _displayedColourOptions.Add(colourVarient);
            }
            
            // Select the first one
            OnColourVarientSelected(_displayedColourOptions[0], furnitureSettings);
        }

        private void ClearColourVarients()
        {
            foreach (var displayedColourOption in _displayedColourOptions)
            {
                Destroy(displayedColourOption.gameObject);
            }
            _displayedColourOptions.Clear();
        }
        
        private void OnColourVarientSelected(VarientOptionBtn selectedBtn, FurnitureSettings furnitureSettings)
        {
            foreach (var displayedColourOption in _displayedColourOptions)
            {
                displayedColourOption.RemoveHighlight();
            }
            selectedBtn.ShowHighlight();
            ApplyColour(selectedBtn.DyePalette);
            RefreshLayout();
        }

        private void ApplyColour(DyeSettings dye)
        {
            _selectedDye = dye;
            TriggerFurniturePlacement(_selectedFurniture);
        }

        private void ApplyVarient(FurnitureVariant variant)
        {
            _itemImage.sprite = variant.FurnitureSettings.ItemSprite;
            _itemTitle.text = variant.FurnitureSettings.ItemName;
            RefreshCraftingRequirements(variant.FurnitureSettings.CraftRequirements);
            RefreshStatsDisplay(variant.FurnitureSettings.MaxDurability);
            
            _selectedFurniture = variant.FurnitureSettings;
            TriggerFurniturePlacement(variant.FurnitureSettings);
        }

        private void ApplyDefault(FurnitureSettings furnitureSettings)
        {
            _itemImage.sprite = furnitureSettings.ItemSprite;
            _itemTitle.text = furnitureSettings.ItemName;
            RefreshCraftingRequirements(furnitureSettings.CraftRequirements);
            RefreshStatsDisplay(furnitureSettings.MaxDurability);

            _selectedFurniture = furnitureSettings;
            
            TriggerFurniturePlacement(furnitureSettings);
        }

        private void RefreshStatsDisplay(int durability)
        {
            string statsList = $"Durability: {durability}\nStats: Not Implemented";
            _statsListText.text = statsList;
        }

        private void RefreshCraftingRequirements(CraftRequirements craftRequirements)
        {
            if (craftRequirements.MinCraftingSkillLevel > 0)
            {
                string craftMsg = $"Required {craftRequirements.CraftingSkill.GetDescription()}: {craftRequirements.MinCraftingSkillLevel}";
                _requiredCraftingLvl.gameObject.SetActive(true);
                _requiredCraftingLvl.text = craftMsg;
            }
            else
            {
                _requiredCraftingLvl.gameObject.SetActive(false);
            }

            foreach (var displayedResourceCost in _displayedResourceCosts)
            {
                Destroy(displayedResourceCost.gameObject);
            }
            _displayedResourceCosts.Clear();
            
            _resourceCostPrefab.gameObject.SetActive(false);
            foreach (var costAmount in craftRequirements.GetMaterialCosts())
            {
                var cost = Instantiate(_resourceCostPrefab, _resourceCostParent);
                cost.gameObject.SetActive(true);
                cost.Init(costAmount);
                _displayedResourceCosts.Add(cost);
            }
        }
        
        public void Hide()
        {
            gameObject.SetActive(false);
            ClearOptions();
        }
        
        private void RefreshLayout()
         {
             _layoutRebuilder.RefreshLayout();
         }
        
        private void TriggerFurniturePlacement(FurnitureSettings furnitureSettings)
        {
            Spawner.Instance.CancelInput();
            PlayerInputController.Instance.ChangeState(PlayerInputState.BuildFurniture, furnitureSettings.ItemName);
            Spawner.Instance.PlanFurniture(furnitureSettings, furnitureSettings.DefaultDirection, _selectedDye);
        }
    }
}
