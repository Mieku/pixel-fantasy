using System;
using System.Collections.Generic;
using Buildings;
using Characters;
using Controllers;
using Data.Item;
using Interfaces;
using Items;
using ScriptableObjects;
using Systems.Details.Build_Details.Scripts;
using Systems.Details.Building_Details.Scripts;
using Systems.Details.Generic_Details.Scripts;
using Systems.Notifications.Scripts;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;

namespace HUD
{
    public class SelectedItemInfoPanel : MonoBehaviour
    {
        [Header("Generic Details")] 
        //[SerializeField] private GenericDetailsUI _genericDetails;

        [Header("Kinling Details")] 
        [SerializeField] private KinlingDetailsUI _kinlingDetails;

        // [Header("Building Details")] 
        // [SerializeField] private BuildingDetails _buildingDetails;

        [FormerlySerializedAs("_buildDetails")]
        [Header("Build Details")] 
        [SerializeField] private BuildFurnitureDetailsUI _buildFurnitureDetails;
        
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
            //_genericDetails.Hide();
            _kinlingDetails.Hide();
            _buildFurnitureDetails.Hide();
            // _buildingDetails.Hide();
            
            _notificationLogger.Show();
        }

        public void ShowItemDetails(IClickableObject clickableObject)
        {
            HideAllDetails();
            _notificationLogger.Hide();
            
            //_genericDetails.Show(clickableObject);
        }

        public void ShowBuildDetails(string header, List<FurnitureData> options )
        {
            HideAllDetails();
            _notificationLogger.Hide();
            
            _buildFurnitureDetails.Show(header, options);
        }
    }
}
