using System;
using Buildings;
using Characters;
using Controllers;
using Interfaces;
using Items;
using Systems.Details.Building_Details.Scripts;
using Systems.Details.Generic_Details.Scripts;
using Systems.Notifications.Scripts;
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

        // [Header("Building Details")] 
        // [SerializeField] private BuildingDetails _buildingDetails;
        
        [Header("Notification Log")]
        [SerializeField] private NotificationLogger _notificationLogger;
        
        private void Start()
        {
            HideAllDetails();
        }

        public void ShowUnitDetails(Kinling kinling)
        {
            HideAllDetails();
            _notificationLogger.Hide();

            _kinlingDetails.Show(kinling);
        }

        // public void ShowBuildingDetails(Building building, bool openConstructionTab = false)
        // {
        //     HideAllDetails();
        //     _notificationLogger.Hide();
        //
        //     if (openConstructionTab)
        //     {
        //         _buildingDetails.Show(building, BuildingDetails.EDetailsTab.Construction);
        //     }
        //     else
        //     {
        //         _buildingDetails.Show(building);
        //     }
        //     
        // }
        
        public void HideAllDetails()
        {
            _genericDetails.Hide();
            _kinlingDetails.Hide();
            // _buildingDetails.Hide();
            
            _notificationLogger.Show();
        }

        public void ShowItemDetails(IClickableObject clickableObject)
        {
            HideAllDetails();
            _notificationLogger.Hide();
            
            _genericDetails.Show(clickableObject);
        }
    }
}
