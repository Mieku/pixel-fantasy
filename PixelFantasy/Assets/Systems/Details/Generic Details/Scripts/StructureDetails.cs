using System.Collections.Generic;
using HUD;
using Items;
using Managers;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Systems.Details.Generic_Details.Scripts
{
    public class StructureDetails : MonoBehaviour
    {
        [SerializeField] private GameObject _constructionProgressHandle;
        [SerializeField] private GameObject _remainingResourcesHandle;
        [SerializeField] private DetailsTextEntry _textEntryPrefab;
        [SerializeField] private Image _constructionProgressFill;
        [SerializeField] private TextMeshProUGUI _constructionProgressDetails;
        [SerializeField] private ResourceCost _remainingResourcePrefab;
        [SerializeField] private Transform _remainingParent;
        
        private GenericDetails _parentDetails;
        private Construction _structure;
        private bool _isActive;
        private List<ResourceCost> _displayedRemainingResources = new List<ResourceCost>();
        
        public void Show(Construction structure, GenericDetails parentDetails)
        {
            _structure = structure;
            gameObject.SetActive(true);
            _textEntryPrefab.gameObject.SetActive(false);
            _remainingResourcePrefab.gameObject.SetActive(false);
            _parentDetails = parentDetails;
            
            _parentDetails.ItemName.color = Librarian.Instance.GetColour("Common Quality");
            _parentDetails.ItemName.text = _structure.DisplayName;

            ClearDisplayedResources();
            if (_structure.RuntimeData.State != EConstructionState.Built)
            {
                _constructionProgressHandle.SetActive(true);
                _remainingResourcesHandle.SetActive(true);
                
                var totalCosts = _structure.RuntimeData.Settings.CraftRequirements.GetMaterialCosts();
                foreach (var remaining in totalCosts)
                {
                    var remainingAmount = _structure.RuntimeData.RemainingMaterialCosts
                        .Find(r => r.Item == remaining.Item);
                    int totalAmount = totalCosts.Find(cost => cost.Item == remaining.Item).Quantity;
                    
                    if (remainingAmount != null)
                    {
                        var costDisplay = Instantiate(_remainingResourcePrefab, _remainingParent);
                        costDisplay.gameObject.SetActive(true);
                        costDisplay.Init(remainingAmount, totalAmount);
                        _displayedRemainingResources.Add(costDisplay);
                    }
                }

                RefreshConstructionInfo();
            }
            else
            {
                _constructionProgressHandle.SetActive(false);
                _remainingResourcesHandle.SetActive(false);
            }
            
            _structure.OnChanged += StructureChanged;
            _isActive = true;
        }

        private void StructureChanged()
        {
            if(!_isActive) return;
            
            RefreshTextEntries();
            
            if (_structure?.RuntimeData.State != EConstructionState.Built)
            {
                RefreshConstructionInfo();
            }
            else
            {
                _constructionProgressHandle.SetActive(false);
                _remainingResourcesHandle.SetActive(false);
            }
        }
        
        public void Hide()
        {
            if (_structure != null)
            {
                _structure.OnChanged -= StructureChanged;
            }

            _structure = null;
            gameObject.SetActive(false);
            _isActive = false;
            ClearDisplayedResources();
        }
        
        private void Update()
        {
            if(!_isActive) return;
            
            _parentDetails.SetDurabilityFill(_structure.RuntimeData.Durability, _structure.RuntimeData.MaxDurability);
        }

        private void RefreshConstructionInfo()
        {
            if (_structure.RuntimeData.State != EConstructionState.Built)
            {
                var progress = _structure.RuntimeData.ConstructionPercent;
                _constructionProgressFill.fillAmount = progress;
                _constructionProgressDetails.text = $"{(int)(progress * 100)}%";

                foreach (var displayedRemaining in _displayedRemainingResources)
                {
                    var remaining = _structure.RuntimeData.RemainingMaterialCosts
                        .Find(r => r.Item == displayedRemaining.ItemSettings);
                    
                    if (remaining != null)
                    {
                        displayedRemaining.RefreshAmount(remaining);
                    }
                }
            }
            else
            {
                ClearDisplayedResources();
                _constructionProgressHandle.SetActive(false);
                _remainingResourcesHandle.SetActive(false);
            }
        }

        private void RefreshTextEntries()
        {
            
        }

        private void ClearDisplayedResources()
        {
            foreach (var displayedRemainingResource in _displayedRemainingResources)
            {
                Destroy(displayedRemainingResource.gameObject);
            }
            _displayedRemainingResources.Clear();
        }
    }
}
