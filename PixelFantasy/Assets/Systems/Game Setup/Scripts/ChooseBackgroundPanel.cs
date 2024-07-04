using Systems.Appearance.Scripts;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Systems.Game_Setup.Scripts
{
    public class ChooseBackgroundPanel : MonoBehaviour
    {
        [SerializeField] private NewGameSection _newGameSection;
        [SerializeField] private TMP_InputField _settlementNameInput;
        [SerializeField] private TMP_Dropdown _mapSizeDropdown;
        [SerializeField] private TMP_Dropdown _worldSizeDropdown;
        [SerializeField] private TextMeshProUGUI _writeupText;

        [SerializeField] private RaceSettings _humanRace;

        private string _settlementName;
        
        private const string _humanSettlersWriteup = "Humans have a deep connection to the land, cultivating fields and tending livestock with dedication. Their villages, often in serene landscapes, reflect their preference for a harmonious lifestyle. While lacking extraordinary traits, humans excel in resilience and resourcefulness.\n\nYou begin your journey as a group of five human settlers in a new land. Equipped with farming knowledge, you set out to transform the untouched wilderness into the thriving settlement of {settlement_name}.";
        
        public void Show()
        {
            gameObject.SetActive(true);

            if (string.IsNullOrEmpty(_settlementName))
            {
                _settlementName = GenerateRandomSettlementName();
                _settlementNameInput.SetTextWithoutNotify(_settlementName);
            }
            
            RefreshWriteup();
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }

        private string GenerateRandomSettlementName()
        {
            return _humanRace.GetRandomSettlementName();
        }

        private void RefreshWriteup()
        {
            string msg = _humanSettlersWriteup;
            msg = msg.Replace("{settlement_name}", _settlementName);
            _writeupText.text = msg;
        }
        
        public void OnBackPressed()
        {
            _newGameSection.OnBack();
        }

        public void OnContinuePressed()
        {
            _newGameSection.PlayerData.SettlementName = _settlementName;
            _newGameSection.OnContinue();
        }
        
        public void OnSettlementNameChanged(string value)
        {
            _settlementName = value;
            RefreshWriteup();
        }

        public void GenerateRandomSettlementNamePressed()
        {
            _settlementName = GenerateRandomSettlementName();
            _settlementNameInput.SetTextWithoutNotify(_settlementName);
            RefreshWriteup();
        }

        public void OnMapSizeDropdownChanged(int value)
        {
            
        }

        public void OnWorldSizeDropdownChanged(int value)
        {
            
        }
    }
}
