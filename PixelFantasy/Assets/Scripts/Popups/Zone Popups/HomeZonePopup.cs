using TMPro;
using UnityEngine;
using Zones;

namespace Popups.Zone_Popups
{
    public class HomeZonePopup : Popup<HomeZonePopup>
    {
        [SerializeField] private TMP_InputField _zoneName;
        
        private HomeZone _zone;
        
        public static void Show(HomeZone zone)
        {
            Open(() => Instance.Init(zone), false);
        }
        
        public override void OnBackPressed()
        {
            _zone.UnclickZone();
            Hide();
        }

        private void Init(HomeZone zone)
        {
            _zone = zone;
            _zoneName.text = _zone.Name;
        }

        public void EditZoneName()
        {
            var nameInput = _zoneName.text;
            Debug.Log("Edited Name to be: " + nameInput);
            _zone.EditZoneName(nameInput);
        }
    }
}
