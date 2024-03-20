using System.Collections.Generic;
using Buildings.Building_Panels;
using Controllers;
using Data.Dye;
using Data.Item;
using Managers;
using Systems.Build_Controls.Scripts;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Systems.Details.Build_Details.Scripts
{
    public class BuildFurnitureDetailsUI : MonoBehaviour
    {
        [SerializeField] private FurnitureCategoryBtn _furnitureCategoryBtn;
        
        [SerializeField] private PanelLayoutRebuilder _layoutRebuilder;
        [SerializeField] private GameObject _panelHandle;
        [SerializeField] private GameObject _currentSelectionHandle;
        [SerializeField] private TextMeshProUGUI _panelTitle;
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
        private FurnitureData _selectedFurniture;
        private DyeData _selectedDye;
         
        public void Show(string header, List<FurnitureData> options)
        {
            _panelHandle.SetActive(true);
            _panelTitle.text = header;
            _currentSelectionHandle.SetActive(false);
            _furnitureCategoryBtn.HighlightSelectedButton(true);

            _selectedFurniture = null;
            
            DisplayOptions(options);
            RefreshLayout();
        }

        private void DisplayOptions(List<FurnitureData> options)
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
            
            ShowOptionSelection(selectedOption.FurnitureData);
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

        private void ShowOptionSelection(FurnitureData furnitureData)
        {
            _currentSelectionHandle.SetActive(true);
            _itemDescription.text = furnitureData.description;

            // Material Options
            if (furnitureData.Varients is { Count: > 0 })
            {
                ShowMaterialVarients(furnitureData);
            }
            else
            {
                _materialOptionGroup.SetActive(false);
                
                // Use default
                ApplyDefault(furnitureData);
            }
            
            // Colour Options
            if (furnitureData.ColourOptions != null && furnitureData.ColourOptions.DyePalettes.Count > 0)
            {
                ShowColourVarients(furnitureData);
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

        private void ShowMaterialVarients(FurnitureData furnitureData)
        {
            _materialOptionGroup.SetActive(true);
            _materialGroupHeader.text = "Material";
            _materialVarientOptionBtnPrefab.gameObject.SetActive(false);
            ClearMaterialVarients();

            var defaultOption = Instantiate(_materialVarientOptionBtnPrefab, _materialLayoutParent);
            defaultOption.gameObject.SetActive(true);
            defaultOption.InitDefault(furnitureData, OnDefaultMaterialSelected);
            _displayedMaterialOptions.Add(defaultOption);
            
            foreach (var varient in furnitureData.Varients)
            {
                var materialVarient = Instantiate(_materialVarientOptionBtnPrefab, _materialLayoutParent);
                materialVarient.gameObject.SetActive(true);
                materialVarient.Init(varient, furnitureData, OnMaterialVarientSelected);
                _displayedMaterialOptions.Add(materialVarient);
            }
            
            OnDefaultMaterialSelected(defaultOption, furnitureData);
        }

        private void ClearMaterialVarients()
        {
            foreach (var materialOption in _displayedMaterialOptions)
            {
                Destroy(materialOption.gameObject);
            }
            _displayedMaterialOptions.Clear();
        }

        private void OnMaterialVarientSelected(VarientOptionBtn selectedBtn, FurnitureData furnitureData)
        {
            foreach (var materialOption in _displayedMaterialOptions)
            {
                materialOption.RemoveHighlight();
            }
            selectedBtn.ShowHighlight();
            
            ApplyVarient(selectedBtn.Variant);
            RefreshLayout();
        }

        private void OnDefaultMaterialSelected(VarientOptionBtn selectedBtn, FurnitureData furnitureData)
        {
            foreach (var materialOption in _displayedMaterialOptions)
            {
                materialOption.RemoveHighlight();
            }
            selectedBtn.ShowHighlight();

            ApplyDefault(furnitureData);
            RefreshLayout();
        }

        private void ShowColourVarients(FurnitureData furnitureData)
        {
            _optionGroupSeperator.SetActive(true);
            _colourOptionGroup.SetActive(true);
            _colourGroupHeader.text = furnitureData.ColourOptions.ColourOptionsHeader;
            _colourVarientOptionBtnPrefab.gameObject.SetActive(false);
            ClearColourVarients();

            if (furnitureData.DefaultDye != null)
            {
                var colourVarient = Instantiate(_colourVarientOptionBtnPrefab, _colourLayoutParent);
                colourVarient.gameObject.SetActive(true);
                colourVarient.Init(furnitureData.DefaultDye, furnitureData, OnColourVarientSelected);
                _displayedColourOptions.Add(colourVarient);
            }

            foreach (var colourOption in furnitureData.ColourOptions.DyePalettes)
            {
                var colourVarient = Instantiate(_colourVarientOptionBtnPrefab, _colourLayoutParent);
                colourVarient.gameObject.SetActive(true);
                colourVarient.Init(colourOption, furnitureData, OnColourVarientSelected);
                _displayedColourOptions.Add(colourVarient);
            }
            
            // Select the first one
            OnColourVarientSelected(_displayedColourOptions[0], furnitureData);
        }

        private void ClearColourVarients()
        {
            foreach (var displayedColourOption in _displayedColourOptions)
            {
                Destroy(displayedColourOption.gameObject);
            }
            _displayedColourOptions.Clear();
        }
        
        private void OnColourVarientSelected(VarientOptionBtn selectedBtn, FurnitureData furnitureData)
        {
            foreach (var displayedColourOption in _displayedColourOptions)
            {
                displayedColourOption.RemoveHighlight();
            }
            selectedBtn.ShowHighlight();
            ApplyColour(selectedBtn.DyePalette);
            RefreshLayout();
        }

        private void ApplyColour(DyeData dye)
        {
            _selectedDye = dye;
            TriggerFurniturePlacement(_selectedFurniture);
        }

        private void ApplyVarient(FurnitureVariant variant)
        {
            _itemImage.sprite = variant.FurnitureData.ItemSprite;
            _itemTitle.text = variant.FurnitureData.ItemName;
            RefreshCraftingRequirements(variant.FurnitureData.CraftRequirements);
            RefreshStatsDisplay(variant.FurnitureData.MaxDurability);
            
            _selectedFurniture = variant.FurnitureData;
            TriggerFurniturePlacement(variant.FurnitureData);
        }

        private void ApplyDefault(FurnitureData furnitureData)
        {
            _itemImage.sprite = furnitureData.ItemSprite;
            _itemTitle.text = furnitureData.ItemName;
            RefreshCraftingRequirements(furnitureData.CraftRequirements);
            RefreshStatsDisplay(furnitureData.Durability);

            _selectedFurniture = furnitureData;
            
            TriggerFurniturePlacement(furnitureData);
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
            foreach (var costAmount in craftRequirements.GetResourceCosts())
            {
                var cost = Instantiate(_resourceCostPrefab, _resourceCostParent);
                cost.gameObject.SetActive(true);
                cost.Init(costAmount);
                _displayedResourceCosts.Add(cost);
            }
        }
        
        public void Hide()
        {
            _panelHandle.SetActive(false);
            ClearOptions();
            _furnitureCategoryBtn.HighlightSelectedButton(false);
        }
        
        private void RefreshLayout()
         {
             _layoutRebuilder.RefreshLayout();
         }
        
        private void TriggerFurniturePlacement(FurnitureData furnitureData)
        {
            Spawner.Instance.CancelInput();
            PlayerInputController.Instance.ChangeState(PlayerInputState.BuildFurniture, furnitureData.ItemName);
            Spawner.Instance.PlanFurniture(furnitureData, furnitureData.DefaultDirection, _selectedDye);
        }
    }
}
