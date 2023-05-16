using System;
using System.Collections.Generic;
using Buildings;
using Characters;
using Controllers;
using Interfaces;
using Items;
using Popups;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace HUD
{
    public class SelectedItemInfoPanel : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _selectedItemNameGeneric;
        [SerializeField] private TextMeshProUGUI _detailsGeneric;
        [SerializeField] private GameObject _genericPanel;

        [Header("Kinling Details")] 
        [SerializeField] private KinlingDetailsUI _kinlingDetails;

        [Header("Building Details")] 
        [SerializeField] private BuildingDetailsUI _buildingDetails;
        
        private SelectionData _selectionData;

        private void Start()
        {
            HideItemDetails();
        }

        public void ShowUnitDetails(Unit unit)
        {
            _genericPanel.SetActive(false);
            _buildingDetails.Hide();

            _kinlingDetails.Show(unit);
        }

        public void ShowBuildingDetails(Building building)
        {
            _genericPanel.SetActive(false);
            _kinlingDetails.Hide();

            _buildingDetails.Show(building);
        }

        public void HideAllDetails()
        {
            _selectionData = null;
            _kinlingDetails.Hide();
            _buildingDetails.Hide();
            _genericPanel.SetActive(false);
        }

        public void ShowItemDetails(SelectionData selectionData)
        {
            HideAllDetails();
            _selectionData = selectionData;
            RefreshDetails();
            HUDOrders.Instance.DisplayOrders(selectionData);
        }

        public void HideItemDetails()
        {
            HideAllDetails();
            HUDOrders.Instance.HideOrders();
        }

        private void RefreshDetails()
        {
            _selectedItemNameGeneric.text = _selectionData.ItemName;
        }
    }
}
