using System.Collections.Generic;
using Characters;
using Data.Item;
using Data.Structure;
using Data.Zones;
using Interfaces;
using Systems.Details.Build_Details.Scripts;
using Systems.Details.Controls_Details.Scripts;
using Systems.Details.Generic_Details.Scripts;
using Systems.Details.Kinling_Details;
using Systems.Notifications.Scripts;
using UnityEngine;
using UnityEngine.Serialization;

namespace HUD
{
    public class SelectedItemInfoPanel : MonoBehaviour
    {
        [Header("Controls")] 
        [SerializeField] private ControlsDetails _controlsDetails;
        
        [Header("Generic Details")] 
        [SerializeField] private ZoneDetails _zoneDetails;
        [SerializeField] private GenericDetails _genericDetails;

        [Header("Kinling Details")] 
        [SerializeField] private KinlingDetails _kinlingDetails;
        
        [Header("Notification Log")]
        [SerializeField] private NotificationLogger _notificationLogger;
        
        private void Start()
        {
            HideAllDetails();
            _notificationLogger.Hide();
            
            _controlsDetails.Show();
        }

        public void ShowUnitDetails(Kinling kinling)
        {
            HideAllDetails();
            _notificationLogger.Hide();

            _kinlingDetails.Show(kinling);
        }
        
        public void HideAllDetails()
        {
            _genericDetails.Hide();
            _zoneDetails.Hide();
            _kinlingDetails.Hide();
            _notificationLogger.Hide();
            _controlsDetails.Hide();
            
            //_notificationLogger.Show();
        }

        public void ShowItemDetails(IClickableObject clickableObject)
        {
            HideAllDetails();
            _notificationLogger.Hide();
            
            _genericDetails.Show(clickableObject);
        }

        public void ShowZoneDetails(ZoneData zoneData)
        {
            HideAllDetails();
            _notificationLogger.Hide();
            
            _zoneDetails.Show(zoneData);
        }
        
        public void ShowBuildStructureDetails(WallSettings wallSettings)
        {
            HideAllDetails();
            _notificationLogger.Hide();

            _controlsDetails.Show();
            _controlsDetails.OpenBuildStructure(wallSettings);
        }
        
        public void ShowBuildStructureDetails(DoorSettings doorSettings)
        {
            HideAllDetails();
            _notificationLogger.Hide();
            
            _controlsDetails.Show();
            _controlsDetails.OpenBuildStructure(doorSettings);
        }
        
        public void ShowBuildStructureDetails(FloorSettings floorSettings)
        {
            HideAllDetails();
            _notificationLogger.Hide();
            
            _controlsDetails.Show();
            _controlsDetails.OpenBuildStructure(floorSettings);
        }

        public void ShowControlsDetails()
        {
            _controlsDetails.Show();
        }
    }
}
