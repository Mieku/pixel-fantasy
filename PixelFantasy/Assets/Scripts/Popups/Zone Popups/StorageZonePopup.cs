using TMPro;
using UnityEngine;
using Zones;

namespace Popups.Zone_Popups
{
    public class StorageZonePopup : Popup<StorageZonePopup>
    {
        [SerializeField] private TMP_InputField _zoneName;
        
        private StorageZone _zone;
        
        public static void Show(StorageZone zone)
        {
            Open(() => Instance.Init(zone), false);
        }
        
        public override void OnBackPressed()
        {
            _zone.UnclickZone();
            Hide();
        }

        private void Init(StorageZone zone)
        {
            _zone = zone;
            _zoneName.text = _zone.Name;
        }

        public void EditZoneName()
        {
            var nameInput = _zoneName.text;
            _zone.EditZoneName(nameInput);
        }

        public void DeleteZone()
        {
            _zone.RemoveZone();
            OnBackPressed();
        }
    }
}
