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
    }
}
