using Managers;
using TMPro;
using UnityEngine;
using Zones;

namespace Popups.Zone_Popups
{
    public class FarmZonePopup : Popup<FarmZonePopup>
    {
        [SerializeField] private TMP_InputField _zoneName;
        [SerializeField] private TMP_Dropdown _cropDD;

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
            
            RefreshAvailableCrops();
        }

        public void EditZoneName()
        {
            var nameInput = _zoneName.text;
            Debug.Log("Edited Name to be: " + nameInput);
            _zone.EditZoneName(nameInput);
        }

        public void ApplyBtnPressed()
        {
            string chosenCropName = _cropDD.options[_cropDD.value].text;

            if (chosenCropName == "None")
            {
                _zone.AssignCrop(null);
            }
            else
            {
                var chosenCrop = Librarian.Instance.GetCropData(chosenCropName);
                _zone.AssignCrop(chosenCrop);
            }
        }

        private void RefreshAvailableCrops()
        {
            var crops = Librarian.Instance.GetAllCropData();
            foreach (var crop in crops)
            {
                _cropDD.options.Add(new TMP_Dropdown.OptionData(crop.CropName, crop.HarvestedItem.ItemSprite));
            }
            
            var curAssignedCrop = _zone.GetAssignedCrop();
            string curCropName = "";
            if (curAssignedCrop != null)
            {
                curCropName = curAssignedCrop.CropName;
                var index = _cropDD.options.FindIndex(data => data.text == curCropName);
                _cropDD.SetValueWithoutNotify(index);
            }
        }
    }
}
