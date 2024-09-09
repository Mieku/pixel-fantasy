using DataPersistence;
using Items;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Systems.Details.Generic_Details.Scripts
{
    public class FuelDetails : MonoBehaviour
    {
        [SerializeField] private Image _fuelFillImage;
        [SerializeField] private TextMeshProUGUI _fuelFillText;
        [SerializeField] private Image _fuelIcon;
        [SerializeField] private TextMeshProUGUI _fuelAmountText;
        [SerializeField] private Toggle _allowRefuelToggle;
        
        private IFueledFurniture _fueledFurniture;
        private FuelSettings _fuelSettings;

        public void Show(IFueledFurniture fueledFurniture)
        {
            _fueledFurniture = fueledFurniture;

            _fuelSettings = _fueledFurniture.GetFuelSettings();

            _fuelIcon.sprite = _fuelSettings.ItemForFuel.ItemSprite;
            int fuelAvailable = _fueledFurniture.GetAmountFuelAvailable();
            _fuelAmountText.text = $"{fuelAvailable}/{_fuelSettings.MaxAdditionalStoredFuelAmount}";
            
            float remainingFuelPercent = _fueledFurniture.GetRemainingBurnPercent();
            _fuelFillText.text = $"{(int)(remainingFuelPercent * 100)}%";
            _fuelFillImage.fillAmount = remainingFuelPercent;
            
            _allowRefuelToggle.SetIsOnWithoutNotify(_fueledFurniture.IsRefuellingAllowed());
        }

        public void OnAllowRefuelingToggled(bool value)
        {
            _fueledFurniture.SetRefuellingAllowed(value);
        }

        private void Update()
        {
            float remainingFuelPercent = _fueledFurniture.GetRemainingBurnPercent();
            _fuelFillText.text = $"{(int)(remainingFuelPercent * 100)}%";
            _fuelFillImage.fillAmount = remainingFuelPercent;
            
            int fuelAvailable = _fueledFurniture.GetAmountFuelAvailable();
            _fuelAmountText.text = $"{fuelAvailable}/{_fuelSettings.MaxAdditionalStoredFuelAmount}";
        }
    }
}
