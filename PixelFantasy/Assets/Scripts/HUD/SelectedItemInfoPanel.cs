using System;
using System.Collections.Generic;
using Gods;
using Items;
using TMPro;
using UnityEngine;

namespace HUD
{
    public class SelectedItemInfoPanel : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _selectedItemName;
        [SerializeField] private GameObject _optionButtonPrefab;
        [SerializeField] private Transform _optionBtnParent;

        private SelectionData _selectionData;
        private List<OptionButton> _displayedOptions = new List<OptionButton>();
        
        public void ShowItemDetails(SelectionData selectionData)
        {
            ClearOptions();
            _selectionData = selectionData;
            RefreshDetails();
            gameObject.SetActive(true);
            CreateOptions();
        }

        public void HideItemDetails()
        {
            _selectionData = null;
            gameObject.SetActive(false);
        }

        private void RefreshDetails()
        {
            _selectedItemName.text = _selectionData.ItemName;
        }

        private void ClearOptions()
        {
            foreach (var optionButton in _displayedOptions)
            {
                Destroy(optionButton.gameObject);
            }
            _displayedOptions.Clear();
        }

        private void RefreshOptions()
        {
            ClearOptions();
            CreateOptions();
        }

        private void CreateOptions()
        {
            foreach (var option in _selectionData.Options)
            {
                switch (option)
                {
                    case Option.Allow:
                        CreateAllowOption();
                        break;
                    case Option.Deconstruct:
                        CreateDeconstructOption();
                        break;
                    case Option.CutTree:
                        CreateCutTreeOption();
                        break;
                    case Option.CutPlant:
                        CreateCutPlantOption();
                        break;
                    case Option.HarvestFruit:
                        CreateHarvestFruitOption();
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }

        private void CreateAllowOption()
        {
            Sprite icon = null;
            
            var optionObj = Instantiate(_optionButtonPrefab, _optionBtnParent);
            var optionBtn = optionObj.GetComponent<OptionButton>();
            
            var owner = _selectionData.Owner;
            var item = owner.GetComponent<Item>();
            if (item != null)
            {
                if (item.IsAllowed())
                {
                    icon = Librarian.Instance.GetSprite("Lock");
                }
                else
                {
                    icon = Librarian.Instance.GetSprite("Key");
                }
            }

            optionBtn.Init("Allow", icon, AllowPressed);

            _displayedOptions.Add(optionBtn);
        }

        private void AllowPressed(OptionButton optionButton)
        {
            var owner = _selectionData.Owner;
            var item = owner.GetComponent<Item>();
            if (item != null)
            {
                if (item.IsAllowed())
                {
                    item.ToggleAllowed(false);
                    optionButton.Icon = Librarian.Instance.GetSprite("Key");
                }
                else
                {
                    item.ToggleAllowed(true);
                    optionButton.Icon = Librarian.Instance.GetSprite("Lock");
                }
            }
        }

        private void CreateDeconstructOption()
        {
            // If the structure is already build, deconstruct. If not, cancel!
            var optionObj = Instantiate(_optionButtonPrefab, _optionBtnParent);
            var optionBtn = optionObj.GetComponent<OptionButton>();
            
            var owner = _selectionData.Owner;
            var structure = owner.GetComponent<Structure>();
            if (structure != null)
            {
                if (structure.IsBuilt())
                {
                    if (!structure.IsDeconstucting())
                    {
                        optionBtn.Init("Deconstruct", Librarian.Instance.GetSprite("Hammer"), DeconstructPressed);
                    }
                    else
                    {
                        optionBtn.Init("Cancel", Librarian.Instance.GetSprite("X"), CancelDeconstructPressed);
                    }
                }
                else
                {
                    // Cancel
                    optionBtn.Init("Cancel", Librarian.Instance.GetSprite("X"), CancelConstructionPressed);
                }
            }
            
            _displayedOptions.Add(optionBtn);
        }

        private void DeconstructPressed(OptionButton optionButton)
        {
            var owner = _selectionData.Owner;
            var structure = owner.GetComponent<Structure>();
            if (structure != null)
            {
                structure.CreateDeconstructionTask();
                ClearOptions();
                CreateOptions();
            }
        }

        private void CancelDeconstructPressed(OptionButton optionButton)
        {
            var owner = _selectionData.Owner;
            var structure = owner.GetComponent<Structure>();
            if (structure != null)
            {
                structure.CancelDeconstruction();
                ClearOptions();
                CreateOptions();
            }
        }

        private void CancelConstructionPressed(OptionButton optionButton)
        {
            var owner = _selectionData.Owner;
            var structure = owner.GetComponent<Structure>();
            if (structure != null)
            {
                structure.CancelConstruction();
                HideItemDetails();
            }
        }

        private void CreateCutTreeOption()
        {
            Sprite icon = Librarian.Instance.GetSprite("Axe");
            
            var optionObj = Instantiate(_optionButtonPrefab, _optionBtnParent);
            var optionBtn = optionObj.GetComponent<OptionButton>();
            var owner = _selectionData.Owner;
            var tree = owner.GetComponent<TreeResource>();

            if (tree != null)
            {
                if (tree.QueuedToCut)
                {
                    optionBtn.Init("Cut Tree", "X", CancelCutTreeOptionPressed);
                }
                else
                {
                    optionBtn.Init("Cut Tree", icon, CutTreeOptionPressed);
                }
            }

            _displayedOptions.Add(optionBtn);
        }

        private void CutTreeOptionPressed(OptionButton optionButton)
        {
            var owner = _selectionData.Owner;
            var tree = owner.GetComponent<TreeResource>();
            if (tree != null)
            {
                tree.CreateCutTreeTask();
                RefreshOptions();
            }
        }
        
        private void CancelCutTreeOptionPressed(OptionButton optionButton)
        {
            var owner = _selectionData.Owner;
            var tree = owner.GetComponent<TreeResource>();
            if (tree != null)
            {
                tree.CancelCutTreeTask();
                RefreshOptions();
            }
        }

        private void CreateCutPlantOption()
        {
            Sprite icon = Librarian.Instance.GetSprite("Scythe");
            
            var optionObj = Instantiate(_optionButtonPrefab, _optionBtnParent);
            var optionBtn = optionObj.GetComponent<OptionButton>();
            var owner = _selectionData.Owner;
            var plant = owner.GetComponent<GrowingResource>();

            if (plant != null)
            {
                if (plant.QueuedToCut)
                {
                    optionBtn.Init("Cut Plant", "X", CancelCutPlantOptionPressed);
                }
                else
                {
                    optionBtn.Init("Cut Plant", icon, CutPlantOptionPressed);
                }
            }

            _displayedOptions.Add(optionBtn);
        }
        
        private void CutPlantOptionPressed(OptionButton optionButton)
        {
            var owner = _selectionData.Owner;
            var plant = owner.GetComponent<GrowingResource>();
            if (plant != null)
            {
                plant.CreateCutPlantTask();
                RefreshOptions();
            }
        }
        
        private void CancelCutPlantOptionPressed(OptionButton optionButton)
        {
            var owner = _selectionData.Owner;
            var plant = owner.GetComponent<GrowingResource>();
            if (plant != null)
            {
                plant.CancelCutPlantTask();
                RefreshOptions();
            }
        }

        private void CreateHarvestFruitOption()
        {
            var owner = _selectionData.Owner;
            var plant = owner.GetComponent<GrowingResource>();
            if (!plant.HasFruitAvailable) return; // No fruit, no option!
            
            var optionObj = Instantiate(_optionButtonPrefab, _optionBtnParent);
            var optionBtn = optionObj.GetComponent<OptionButton>();
            Sprite icon = Librarian.Instance.GetSprite("Scythe");
            
            if (plant != null)
            {
                if (plant.QueuedToHarvest)
                {
                    optionBtn.Init("Harvest Fruit", "X", CancelHarvestFruitOptionPressed);
                }
                else
                {
                    optionBtn.Init("Harvest Fruit", icon, HarvestFruitOptionPressed);
                }
            }

            _displayedOptions.Add(optionBtn);
        }

        private void HarvestFruitOptionPressed(OptionButton optionButton)
        {
            var owner = _selectionData.Owner;
            var plant = owner.GetComponent<GrowingResource>();
            if (plant != null)
            {
                plant.CreateHarvestFruitTask();
                RefreshOptions();
            }
        }
        
        private void CancelHarvestFruitOptionPressed(OptionButton optionButton)
        {
            var owner = _selectionData.Owner;
            var plant = owner.GetComponent<GrowingResource>();
            if (plant != null)
            {
                plant.CancelHarvestFruitTask();
                RefreshOptions();
            }
        }
    }
}
