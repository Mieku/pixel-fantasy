using System.Collections.Generic;
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
        [SerializeField] private MultipleDetails _multipleDetails;

        [Header("Kinling Details")] 
        [SerializeField] private KinlingDetails _kinlingDetails;
        
        private void Start()
        {
            HideAllDetails();
            
            _controlsDetails.Show();
        }
        
        public void HideAllDetails()
        {
            _genericDetails.Hide();
            _zoneDetails.Hide();
            _kinlingDetails.Hide();
            _controlsDetails.Hide();
            _multipleDetails.Hide();
        }

        public void ShowItemDetails(PlayerInteractable playerInteractable)
        {
            HideAllDetails();

            if (playerInteractable is Kinling kinling)
            {
                _kinlingDetails.Show(kinling);
            }
            else
            {
                _genericDetails.Show(playerInteractable);
            }
        }

        public void ShowMultipleDetailed(List<PlayerInteractable> playerInteractables)
        {
            HideAllDetails();
            
            _multipleDetails.Show(playerInteractables);
        }

        public void ShowZoneDetails(ZoneData zoneData)
        {
            HideAllDetails();
            
            _zoneDetails.Show(zoneData);
        }
        
        public void ShowBuildStructureDetails(WallSettings wallSettings)
        {
            HideAllDetails();

            _controlsDetails.Show();
            _controlsDetails.OpenBuildStructure(wallSettings);
        }
        
        public void ShowBuildStructureDetails(DoorSettings doorSettings)
        {
            HideAllDetails();
            
            _controlsDetails.Show();
            _controlsDetails.OpenBuildStructure(doorSettings);
        }
        
        public void ShowBuildStructureDetails(FloorSettings floorSettings)
        {
            HideAllDetails();
            
            _controlsDetails.Show();
            _controlsDetails.OpenBuildStructure(floorSettings);
        }

        public void ShowControlsDetails()
        {
            _controlsDetails.Show();
        }
    }
}
