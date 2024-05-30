using System.Collections.Generic;
using Buildings.Building_Panels;
using Characters;
using Items;
using Managers;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Systems.Details.Generic_Details.Scripts
{
    public class FurnitureDetails : MonoBehaviour
    {
        [SerializeField] private CraftingControls _craftingControls;
        [SerializeField] private GameObject _ownersHandle;
        [SerializeField] private TMP_Dropdown _owner1Dropdown, _owner2Dropdown;
        
        [SerializeField] private GameObject _constructionProgressHandle;
        [SerializeField] private GameObject _remainingResourcesHandle;
        [SerializeField] private Image _constructionProgressFill;
        [SerializeField] private TextMeshProUGUI _constructionProgressDetails;
        [SerializeField] private ResourceCost _remainingResourcePrefab;
        [SerializeField] private Transform _remainingParent;

        [SerializeField] private StorageSettingsDetails _storageSettingsDetails;
        
        private GenericDetails _parentDetails;
        private Furniture _furniture;
        private bool _isActive;
        private List<KinlingData> _ownerReferenceList = new List<KinlingData>();
        private List<ResourceCost> _displayedRemainingResources = new List<ResourceCost>();
        
        public void Show(Furniture furniture, GenericDetails parentDetails)
        {
            gameObject.SetActive(true);
            _parentDetails = parentDetails;
            _furniture = furniture;
            _isActive = true;

            _parentDetails.ItemName.color = _furniture.RuntimeData.GetQualityColour();
            _parentDetails.ItemName.text = $"{_furniture.DisplayName} ({_furniture.RuntimeData.Quality.GetDescription()})";
            
            InitConstructionInfo();
            
            if (_furniture.RuntimeData.State == EFurnitureState.Built)
            {
                RefreshOwners(_furniture as IAssignedFurniture);
            }
            else
            {
                _ownersHandle.SetActive(false);
            }
            
            CheckShowCraftingDetails();
            CheckShowStorageDetails();
            
            _parentDetails.RefreshLayout();
        }

        private void CheckShowStorageDetails()
        {
            var storage = _furniture as Storage;
            if (storage != null && _furniture.RuntimeData.State == EFurnitureState.Built)
            {
                _storageSettingsDetails.Show(storage);
            }
            else
            {
                _storageSettingsDetails.Hide();
            }
        }

        private void CheckShowCraftingDetails()
        {
            var craftingTable = _furniture as CraftingTable;
            if (craftingTable != null && _furniture.RuntimeData.State == EFurnitureState.Built)
            {
                _craftingControls.Show(craftingTable);
            }
            else
            {
                _craftingControls.Hide();
            }
        }

        private void InitConstructionInfo()
        {
            ClearDisplayedResources();
            _remainingResourcePrefab.gameObject.SetActive(false);
            
            if(_furniture.RuntimeData.State != EFurnitureState.Built)
            {
                _constructionProgressHandle.SetActive(true);
                _remainingResourcesHandle.SetActive(true);
                
                var totalCosts = _furniture.RuntimeData.FurnitureSettings.CraftRequirements.GetMaterialCosts();
                foreach (var remaining in totalCosts)
                {
                    var remainingAmount = _furniture.RuntimeData.RemainingMaterialCosts
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
        }

        private void RefreshConstructionInfo()
        {
            if(_furniture.RuntimeData.State != EFurnitureState.Built)
            {
                var progress = _furniture.RuntimeData.ConstructionPercent;
                _constructionProgressFill.fillAmount = progress;
                _constructionProgressDetails.text = $"{(int)(progress * 100)}%";

                foreach (var displayedRemaining in _displayedRemainingResources)
                {
                    var remaining = _furniture.RuntimeData.RemainingMaterialCosts
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
        
        private void ClearDisplayedResources()
        {
            foreach (var displayedRemainingResource in _displayedRemainingResources)
            {
                Destroy(displayedRemainingResource.gameObject);
            }
            _displayedRemainingResources.Clear();
        }

        private void RefreshOwners(IAssignedFurniture assignedFurniture)
        {
            if (assignedFurniture == null)
            {
                _ownersHandle.SetActive(false);
                return;
            }
            
            _ownersHandle.SetActive(true);
            
            var curAssigned = assignedFurniture.GetPrimaryOwner();
            var kinlingsList = KinlingsManager.Instance.AllKinlings;
            _owner1Dropdown.ClearOptions();
            _ownerReferenceList.Clear();
            int curKinlingIndex = -1;
            var options = new List<string>() { "Unassigned" };
            for (int i = 0; i < kinlingsList.Count; i++)
            {
                if (kinlingsList[i] == curAssigned)
                {
                    curKinlingIndex = i + 1;
                }
                options.Add(kinlingsList[i].Fullname);
                _ownerReferenceList.Add(kinlingsList[i]);
            }
            
            _owner1Dropdown.AddOptions(options);
            _owner1Dropdown.SetValueWithoutNotify(curKinlingIndex);

            
            if (assignedFurniture.CanHaveSecondaryOwner())
            {
                var primaryOwner = assignedFurniture.GetPrimaryOwner();
                if (primaryOwner != null)
                {
                    _owner2Dropdown.gameObject.SetActive(true);
                    var secondaryOptions = new List<string>() { "Unassigned" };
                    if (primaryOwner.Partner != null)
                    {
                        secondaryOptions.Add(primaryOwner.Partner.Fullname);
                    }
                    _owner2Dropdown.ClearOptions();
                    _owner2Dropdown.AddOptions(secondaryOptions);

                    if (assignedFurniture.GetSecondaryOwner() == primaryOwner.Partner)
                    {
                        _owner2Dropdown.SetValueWithoutNotify(1);
                    }
                    else
                    {
                        _owner2Dropdown.SetValueWithoutNotify(0);
                    }
                }
                else
                {
                    _owner2Dropdown.gameObject.SetActive(false);
                }
            }
            else
            {
                _owner2Dropdown.gameObject.SetActive(false);
            }
        }

        public void OnPrimaryOwnerChanged(int value)
        {
            var assignedFurniture = (IAssignedFurniture)_furniture;
            if (value == 0) // Unassigned
            {
                assignedFurniture.ReplacePrimaryOwner(null);
            }
            else
            {
                var newOwner = _ownerReferenceList[value - 1];
                assignedFurniture.ReplacePrimaryOwner(newOwner);
            }
            
            RefreshOwners(assignedFurniture);
        }
        
        public void OnSecondaryOwnerChanged(int value)
        {
            var assignedFurniture = (IAssignedFurniture)_furniture;
            if (value == 0) // Unassigned
            {
                assignedFurniture.ReplaceSecondaryOwner(null);
            }
            else
            {
                var newOwner = assignedFurniture.GetPrimaryOwner().Partner;
                assignedFurniture.ReplaceSecondaryOwner(newOwner);
            }
            
            RefreshOwners(assignedFurniture);
        }
        
        public void Hide()
        {
            gameObject.SetActive(false);
            _isActive = false;
        }
        
        private void Update()
        {
            if(!_isActive) return;
            
            _parentDetails.SetDurabilityFill(_furniture.RuntimeData.Durability, _furniture.RuntimeData.FurnitureSettings.MaxDurability);
        }
    }
}
