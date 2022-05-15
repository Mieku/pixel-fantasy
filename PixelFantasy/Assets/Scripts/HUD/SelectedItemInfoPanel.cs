using System;
using System.Collections.Generic;
using Controllers;
using Gods;
using Interfaces;
using Items;
using TMPro;
using UnityEngine;

namespace HUD
{
    public class SelectedItemInfoPanel : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _selectedItemNameGeneric;
        [SerializeField] private TextMeshProUGUI _detailsGeneric;
        [SerializeField] private GameObject _genericPanel;
        [SerializeField] private GameObject _unitPanel;
        
        private SelectionData _selectionData;

        //[SerializeField] private GameObject _optionButtonPrefab;
        //[SerializeField] private Transform _optionBtnParent;
        //private List<OptionButton> _displayedOptions = new List<OptionButton>();
        
        public void ShowItemDetails(SelectionData selectionData)
        {
            //ClearOptions();
            _selectionData = selectionData;
            RefreshDetails();
            _genericPanel.SetActive(true);
            HUDOrders.Instance.DisplayOrders(selectionData);
            //CreateOptions();
        }

        public void HideItemDetails()
        {
            _selectionData = null;
            _genericPanel.SetActive(false);
            HUDOrders.Instance.HideOrders();
        }

        private void RefreshDetails()
        {
            _selectedItemNameGeneric.text = _selectionData.ItemName;
        }

        // private void ClearOptions()
        // {
        //     foreach (var optionButton in _displayedOptions)
        //     {
        //         Destroy(optionButton.gameObject);
        //     }
        //     _displayedOptions.Clear();
        // }

        // private void RefreshOptions()
        // {
        //     ClearOptions();
        //     CreateOptions();
        // }

        // private void CreateOptions()
        // {
        //     foreach (var option in _selectionData.Options)
        //     {
        //         switch (option)
        //         {
        //             case Option.Allow:
        //                 CreateAllowOption();
        //                 break;
        //             case Option.Deconstruct:
        //                 CreateDeconstructOption();
        //                 break;
        //             case Option.CutTree:
        //                 CreateCutTreeOption();
        //                 break;
        //             case Option.CutPlant:
        //                 CreateCutPlantOption();
        //                 break;
        //             case Option.HarvestFruit:
        //                 CreateHarvestFruitOption();
        //                 break;
        //             default:
        //                 throw new ArgumentOutOfRangeException();
        //         }
        //     }
        // }



        // private void CreateDeconstructOption()
        // {
        //     // If the structure is already build, deconstruct. If not, cancel!
        //     var optionObj = Instantiate(_optionButtonPrefab, _optionBtnParent);
        //     var optionBtn = optionObj.GetComponent<OptionButton>();
        //     
        //     var owner = _selectionData.Owner;
        //     
        //     // Structure
        //     var structure = owner.GetComponent<Structure>();
        //     if (structure != null)
        //     {
        //         if (structure.IsBuilt())
        //         {
        //             if (!structure.IsDeconstucting())
        //             {
        //                 optionBtn.Init("Deconstruct", Librarian.Instance.GetSprite("Hammer"), DeconstructPressed);
        //             }
        //             else
        //             {
        //                 optionBtn.Init("Cancel", Librarian.Instance.GetSprite("X"), CancelDeconstructPressed);
        //             }
        //         }
        //         else
        //         {
        //             // Cancel
        //             optionBtn.Init("Cancel", Librarian.Instance.GetSprite("X"), CancelConstructionPressed);
        //         }
        //     }
        //     
        //     // Floor
        //     var floor = owner.GetComponent<Floor>();
        //     if (floor != null)
        //     {
        //         optionBtn.Init("Cancel", Librarian.Instance.GetSprite("X"), CancelConstructionPressed);
        //     }
        //     
        //     _displayedOptions.Add(optionBtn);
        // }

        // private void DeconstructPressed(OptionButton optionButton)
        // {
        //     var owner = _selectionData.Owner;
        //     var structure = owner.GetComponent<Structure>();
        //     if (structure != null)
        //     {
        //         structure.CreateDeconstructionTask();
        //         ClearOptions();
        //         CreateOptions();
        //     }
        // }

        // private void CancelDeconstructPressed(OptionButton optionButton)
        // {
        //     var owner = _selectionData.Owner;
        //     var structure = owner.GetComponent<Structure>();
        //     if (structure != null)
        //     {
        //         structure.CancelDeconstruction();
        //         ClearOptions();
        //         CreateOptions();
        //     }
        // }

        // private void CancelConstructionPressed(OptionButton optionButton)
        // {
        //     var owner = _selectionData.Owner;
        //     var structure = owner.GetComponent<Structure>();
        //     if (structure != null)
        //     {
        //         structure.CancelConstruction();
        //         HideItemDetails();
        //     }
        //     
        //     var floor = owner.GetComponent<Floor>();
        //     if (floor != null)
        //     {
        //         floor.CancelConstruction();
        //         HideItemDetails();
        //     }
        // }



        // private void CreateCutPlantOption()
        // {
        //     Sprite icon = Librarian.Instance.GetSprite("Scythe");
        //     
        //     var optionObj = Instantiate(_optionButtonPrefab, _optionBtnParent);
        //     var optionBtn = optionObj.GetComponent<OptionButton>();
        //     var owner = _selectionData.Owner;
        //     var plant = owner.GetComponent<GrowingResource>();
        //
        //     if (plant != null)
        //     {
        //         if (plant.QueuedToCut)
        //         {
        //             optionBtn.Init("Cut Plant", "X", CancelCutPlantOptionPressed);
        //         }
        //         else
        //         {
        //             optionBtn.Init("Cut Plant", icon, CutPlantOptionPressed);
        //         }
        //     }
        //
        //     _displayedOptions.Add(optionBtn);
        // }
        
        // private void CutPlantOptionPressed(OptionButton optionButton)
        // {
        //     var owner = _selectionData.Owner;
        //     var plant = owner.GetComponent<GrowingResource>();
        //     if (plant != null)
        //     {
        //         plant.CreateCutPlantTask();
        //         RefreshOptions();
        //     }
        // }
        
        // private void CancelCutPlantOptionPressed(OptionButton optionButton)
        // {
        //     var owner = _selectionData.Owner;
        //     var plant = owner.GetComponent<GrowingResource>();
        //     if (plant != null)
        //     {
        //         plant.CancelCutPlantTask();
        //         RefreshOptions();
        //     }
        // }




    }
}
