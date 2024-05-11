using System;
using System.Collections.Generic;
using Buildings.Building_Panels;
using Items;
using Managers;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Systems.Details.Generic_Details.Scripts
{
    public class ResourceDetails : MonoBehaviour
    {
        [SerializeField] private GameObject _growthDisplayHandle;
        [SerializeField] private GameObject _extractYieldHandle;
        [SerializeField] private GameObject _seperatorHandle;
        [SerializeField] private GameObject _fruitProgressDisplayHandle;
        [SerializeField] private GameObject _harvestYieldHandle;

        [SerializeField] private Image _growthFill;
        [SerializeField] private TextMeshProUGUI _growthPercentDisplay;
        [SerializeField] private Image _fruitFill;
        [SerializeField] private TextMeshProUGUI _fruitPercentDisplay;

        [SerializeField] private Transform _extractYieldLayout;
        [SerializeField] private ResourceCost _yieldDisplayPrefab;
        [SerializeField] private Transform _harvestYieldLayout;
        [SerializeField] private ResourceCost _harvestYieldDisplayPrefab;
        
        private BasicResource _basicResource;
        private GenericDetails _parentDetails;
        private bool _isActive;

        private List<ResourceCost> _displayedYieldDisplay = new List<ResourceCost>();
        private List<ResourceCost> _displayedHarvestYieldDisplay = new List<ResourceCost>();

        public void Show(BasicResource basicResource, GenericDetails parentDetails)
        {
            gameObject.SetActive(true);
            _parentDetails = parentDetails;
            _basicResource = basicResource;
            _isActive = true;

            _yieldDisplayPrefab.gameObject.SetActive(false);
            _harvestYieldDisplayPrefab.gameObject.SetActive(false);

            _parentDetails.ItemName.color = Librarian.Instance.GetColour("Common Quality");
            _parentDetails.ItemName.text = _basicResource.DisplayName;
            
            DisplayExtractionYield();

            var growingResource = _basicResource as GrowingResource;
            if (growingResource != null)
            {
                DisplayGrowingResourceDetails(growingResource);
            }
            
            _isActive = true;
            RefreshGrowthDisplay();
            
            _parentDetails.RefreshLayout();
        }

        public void Hide()
        {
            gameObject.SetActive(false);
            _isActive = false;
        }

        private void Update()
        {
            if(!_isActive) return;

            _parentDetails.SetDurabilityFill(_basicResource.RuntimeData.Health, _basicResource.RuntimeData.MaxHealth);
            
            RefreshGrowthDisplay();
        }

        private void RefreshGrowthDisplay()
        {
            var growingResource = _basicResource as GrowingResource;
            if (growingResource != null)
            {
                var growthPercent = growingResource.RuntimeGrowingResourceData.GetGrowthPercentage();
                _growthDisplayHandle.gameObject.SetActive(true);

                _growthFill.fillAmount = growthPercent;
                _growthPercentDisplay.text = $"{(int)(growthPercent * 100)}%";
                
                RefreshFruitGrowthDisplay(growingResource);
            }
            else
            {
                _growthDisplayHandle.gameObject.SetActive(false);
                _harvestYieldHandle.gameObject.SetActive(false);
                _fruitProgressDisplayHandle.gameObject.SetActive(false);
            }
        }

        private void RefreshFruitGrowthDisplay(GrowingResource growingResource)
        {
            if (growingResource.RuntimeGrowingResourceData.GrowingResourceSettings.HasFruit)
            {
                _fruitProgressDisplayHandle.gameObject.SetActive(true);
                _harvestYieldHandle.gameObject.SetActive(true);

                var fruitGrowthPercent = growingResource.RuntimeGrowingResourceData.GetFruitingPercentage();
                _fruitFill.fillAmount = fruitGrowthPercent;
                _fruitPercentDisplay.text = $"{(int)(fruitGrowthPercent * 100)}%";
            }
            else
            {
                _fruitProgressDisplayHandle.gameObject.SetActive(false);
                _harvestYieldHandle.gameObject.SetActive(false);
            }
        }

        private void DisplayExtractionYield()
        {
            _extractYieldHandle.gameObject.SetActive(true);
            var yield = _basicResource.GetHarvestableItems().GetDropAverages();

            foreach (var displayedYield in _displayedYieldDisplay)
            {
                Destroy(displayedYield.gameObject);
            }
            _displayedYieldDisplay.Clear();

            foreach (var drop in yield)
            {
                var display = Instantiate(_yieldDisplayPrefab, _extractYieldLayout);
                display.gameObject.SetActive(true);
                display.Init(drop);
                _displayedYieldDisplay.Add(display);
            }
        }

        private void DisplayGrowingResourceDetails(GrowingResource growingResource)
        {
            // var growthPercent = growingResource.RuntimeGrowingResourceData.GetGrowthPercentage();
            // _growthDisplayHandle.gameObject.SetActive(true);
            //
            // _growthFill.fillAmount = growthPercent;
            // _growthPercentDisplay.text = $"{growthPercent * 100}%";


            if (growingResource.RuntimeGrowingResourceData.GrowingResourceSettings.HasFruit)
            {
                // _fruitProgressDisplayHandle.gameObject.SetActive(true);
                // _harvestYieldHandle.gameObject.SetActive(true);
                //
                // var fruitGrowthPercent = growingResource.RuntimeGrowingResourceData.GetFruitingPercentage();
                // _fruitFill.fillAmount = fruitGrowthPercent;
                // _fruitPercentDisplay.text = $"{fruitGrowthPercent * 100}%";

                var fruitYield = growingResource.RuntimeGrowingResourceData.GetFruitLoot();
                
                foreach (var displayedYield in _displayedHarvestYieldDisplay)
                {
                    Destroy(displayedYield.gameObject);
                }
                _displayedHarvestYieldDisplay.Clear();

                foreach (var drop in fruitYield)
                {
                    var display = Instantiate(_harvestYieldDisplayPrefab, _harvestYieldLayout);
                    display.gameObject.SetActive(true);
                    display.Init(drop);
                    _displayedHarvestYieldDisplay.Add(display);
                }
            }
            else
            {
                _fruitProgressDisplayHandle.gameObject.SetActive(false);
                _harvestYieldHandle.gameObject.SetActive(false);
            }
        }
    }
}
