using TMPro;
using UnityEngine;
using Zones;

namespace Popups.Zone_Popups
{
    public class FarmZonePopup : Popup<FarmZonePopup>
    {
        [SerializeField] private TMP_InputField _zoneName;
        
        private FarmZone _zone;
        
        public static void Show(FarmZone zone)
        {
            Open(() => Instance.Init(zone), false);
        }
        
        public override void OnBackPressed()
        {
            _zone.UnclickZone();
            Hide();
        }

        private void Init(FarmZone zone)
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
