using Buildings;
using Characters;
using Controllers;
using Interfaces;
using Items;
using Systems.Details.Generic_Details.Scripts;
using TMPro;
using UnityEngine;

namespace HUD
{
    public class SelectedItemInfoPanel : MonoBehaviour
    {
        [Header("Generic Details")] 
        [SerializeField] private GenericDetailsUI _genericDetails;

        [Header("Kinling Details")] 
        [SerializeField] private KinlingDetailsUI _kinlingDetails;

        [Header("Building Details")] 
        [SerializeField] private BuildingDetailsUI _buildingDetails;

        private void Start()
        {
            HideAllDetails();
        }

        public void ShowUnitDetails(Unit unit)
        {
            HideAllDetails();

            _kinlingDetails.Show(unit);
        }

        public void ShowBuildingDetails(Building building)
        {
            HideAllDetails();

            _buildingDetails.Show(building);
        }
        
        public void HideAllDetails()
        {
            _genericDetails.Hide();
            _kinlingDetails.Hide();
            _buildingDetails.Hide();
        }

        // public void ShowItemDetails(SelectionData selectionData)
        // {
        //     HideAllDetails();
        //     
        //     _genericDetails.Show(selectionData);
        //
        //     HUDOrders.Instance.DisplayOrders(selectionData); // TODO: Handle in the panel
        // }

        public void ShowItemDetails(IClickableObject clickableObject)
        {
            HideAllDetails();
            
            _genericDetails.Show(clickableObject);
        }
    }
}
