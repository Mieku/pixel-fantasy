using System;
using System.Collections.Generic;
using Buildings;
using Characters;
using Controllers;
using Data.Item;
using Data.Zones;
using Interfaces;
using Items;
using ScriptableObjects;
using Systems.Details.Build_Details.Scripts;
using Systems.Details.Building_Details.Scripts;
using Systems.Details.Generic_Details.Scripts;
using Systems.Details.Kinling_Details;
using Systems.Notifications.Scripts;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;

namespace HUD
{
    public class SelectedItemInfoPanel : MonoBehaviour
    {
        [Header("Generic Details")] 
        [SerializeField] private ZoneDetails _zoneDetails;
        //[SerializeField] private GenericDetailsUI _genericDetails;

        [Header("Kinling Details")] 
        [SerializeField] private KinlingDetails _kinlingDetails;

        [FormerlySerializedAs("_buildDetails")]
        [Header("Build Details")] 
        [SerializeField] private BuildFurnitureDetailsUI _buildFurnitureDetails;
        [SerializeField] private BuildStructureDetailsUI _buildStructureDetails;
        
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
        
        public void HideAllDetails()
        {
            //_genericDetails.Hide();
            _zoneDetails.Hide();
            _kinlingDetails.Hide();
            _buildFurnitureDetails.Hide();
            _buildStructureDetails.Hide();
            
            _notificationLogger.Show();
        }

        public void ShowItemDetails(IClickableObject clickableObject)
        {
            HideAllDetails();
            _notificationLogger.Hide();
        }

        public void ShowZoneDetails(ZoneData zoneData)
        {
            HideAllDetails();
            _notificationLogger.Hide();
            
            _zoneDetails.Show(zoneData);
        }

        public void ShowBuildFurnitureDetails(string header, List<FurnitureSettings> options )
        {
            HideAllDetails();
            _notificationLogger.Hide();
            
            _buildFurnitureDetails.Show(header, options);
        }

        public void ShowBuildStructureDetails()
        {
            HideAllDetails();
            _notificationLogger.Hide();
            
            _buildStructureDetails.Show();
        }
    }
}
