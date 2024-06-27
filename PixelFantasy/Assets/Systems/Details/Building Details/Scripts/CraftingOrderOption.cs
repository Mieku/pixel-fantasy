using System;
using System.Collections.Generic;
using Systems.Details.Generic_Details.Scripts;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Systems.Details.Building_Details.Scripts
{
    public class CraftingOrderOption : MonoBehaviour
    {
        [SerializeField] private Image _itemIcon;
        [SerializeField] private GameObject _selectedHandle;
        [SerializeField] private GameObject _hoverPanel;

        [SerializeField] private TextMeshProUGUI _itemName;
        [SerializeField] private Transform _detailsLayout;
        [SerializeField] private GameObject _detailsTextGO;
        [SerializeField] private CostDisplay _costDisplayPrefab;
        [SerializeField] private Transform _costLayout;

        public ItemSettings Item { get; private set; }

        private Action<ItemSettings> _onSelectedCallback;
        private List<CostDisplay> _displayedCosts = new List<CostDisplay>();
        private List<TextMeshProUGUI> _textEntries = new List<TextMeshProUGUI>();

        public void Init(ItemSettings item, Action<ItemSettings> onSelectedCallback)
        {
            Item = item;
            _itemIcon.sprite = Item.ItemSprite;
            _selectedHandle.SetActive(false);
            _hoverPanel.SetActive(false);
            _costDisplayPrefab.gameObject.SetActive(false);
            _detailsTextGO.SetActive(false);
            _onSelectedCallback = onSelectedCallback;
        }
        
        public void OnOptionPressed()
        {
            _onSelectedCallback.Invoke(Item);
        }

        private void RefreshHoverPanel()
        {
            _itemName.text = Item.ItemName;
            
            // Delete current displayed costs
            foreach (var displayedCost in _displayedCosts)
            {
                Destroy(displayedCost.gameObject);
            }
            _displayedCosts.Clear();
            
            // Delete text entries
            foreach (var entry in _textEntries)
            {
                Destroy(entry.gameObject);
            }
            _textEntries.Clear();

            var craftedItem = Item as CraftedItemSettings;
            if (craftedItem != null)
            {
                if (craftedItem.CraftRequirements.MinCraftingSkillLevel > 0)
                {
                    var skillDetails = craftedItem.CraftRequirements.CraftingSkill.GetDescription() + " lvl " +
                                    craftedItem.CraftRequirements.MinCraftingSkillLevel;
                    CreateTextEntry("Skill", skillDetails);
                }
                
                CreateTextEntry("Work", craftedItem.CraftRequirements.WorkCost + "");
                
                var costs = craftedItem.CraftRequirements.GetMaterialCosts();
                foreach (var cost in costs)
                {
                    var display = Instantiate(_costDisplayPrefab, _costLayout);
                    display.Init(cost);
                    display.gameObject.SetActive(true);
                    _displayedCosts.Add(display);
                }
            }

            var craftedMeal = Item as MealSettings;
            if (craftedMeal != null)
            {
                if (craftedMeal.MealRequirements.MinCraftingSkillLevel > 0)
                {
                    var skillDetails = craftedMeal.MealRequirements.CraftingSkill.GetDescription() + " lvl " +
                                       craftedMeal.MealRequirements.MinCraftingSkillLevel;
                    CreateTextEntry("Skill", skillDetails);
                }
                
                CreateTextEntry("Work", craftedMeal.MealRequirements.WorkCost + "");
                
                var ingredients = craftedMeal.MealRequirements.GetIngredients();
                foreach (var ingredient in ingredients)
                {
                    var display = Instantiate(_costDisplayPrefab, _costLayout);
                    display.Init(ingredient);
                    display.gameObject.SetActive(true);
                    _displayedCosts.Add(display);
                }
            }
        }

        private void CreateTextEntry(string header, string content)
        {
            var entry = Instantiate(_detailsTextGO, _detailsLayout).GetComponent<TextMeshProUGUI>();
            entry.gameObject.SetActive(true);
            entry.text = $"<color=#FEBA2D>{header}:</color> <b>{content}</b>";
            _textEntries.Add(entry);
        }

        public void OnHoverStart()
        {
            _hoverPanel.SetActive(true);
            _selectedHandle.SetActive(true);
            RefreshHoverPanel();
        }

        public void OnHoverEnd()
        {
            _hoverPanel.SetActive(false);
            _selectedHandle.SetActive(false);
        }
    }
}
