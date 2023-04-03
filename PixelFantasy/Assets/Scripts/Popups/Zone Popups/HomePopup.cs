using Buildings;
using TMPro;
using UnityEngine;
using Zones;

namespace Popups.Zone_Popups
{
    public class HomePopup : Popup<HomePopup>
    {
        [SerializeField] private TMP_InputField _buildingName;
        
        private Home _home;
        
        public static void Show(Home home)
        {
            Open(() => Instance.Init(home), false);
        }
        
        public override void OnBackPressed()
        {
            Hide();
        }

        private void Init(Home home)
        {
            _home = home;
        }

        public void EditZoneName()
        {
            // var nameInput = _zoneName.text;
            // _zone.EditZoneName(nameInput);
        }

        public void EnterBuildingPressed()
        {
            _home.ViewInterior();
        }

        public void DeconstructPressed()
        {
            
        }
    }
}
