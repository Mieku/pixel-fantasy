using Characters;
using Systems.Details.Controls_Details.Scripts;
using Systems.Details.Generic_Details.Scripts;
using Systems.Details.Kinling_Details;
using Systems.Notifications.Scripts;
using UnityEngine;

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
        }

        public void ShowItemDetails(PlayerInteractable playerInteractable)
        {
            HideAllDetails();
            _notificationLogger.Hide();
            
            _genericDetails.Show(playerInteractable);
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
