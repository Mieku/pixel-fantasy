using Buildings;
using HUD.Tooltip;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Systems.Details.Building_Details.Scripts
{
    public class ProductionOption : MonoBehaviour
    {
        [Header("Priority Controls Section")]
        [SerializeField] private TextMeshProUGUI _priorityText;
        [SerializeField] private GameObject _priorityUpBtn;
        [SerializeField] private GameObject _priorityDownBtn;
        [SerializeField] private Image _playPauseBtnIcon;
        [SerializeField] private Sprite _playIcon;
        [SerializeField] private Sprite _pauseIcon;
        [SerializeField] private Color _defaultWhite;
        [SerializeField] private Color _pausedRed;
        [SerializeField] private TooltipTrigger _playPauseTooltip;

        [Header("Progress Section")]
        [SerializeField] private Image _progressFill;
        [SerializeField] private ProductionMaterialDisplay _materialDisplay;
        [SerializeField] private ProductionMaterialDisplay _productDisplay;
        [SerializeField] private Transform _materialParent;
        
        [Header("Limit Controls Section")]
        [SerializeField] private Image _limitBG;
        [SerializeField] private Sprite _activeLimitBGSpr;
        [SerializeField] private Sprite _inactiveLimitBGSpr;
        [SerializeField] private Toggle _enableLimitToggle;
        [SerializeField] private TMP_InputField _limitInputField;
        [SerializeField] private TooltipTrigger _limitToggleTooltip;
        [SerializeField] private TooltipTrigger _limitInputTooltip;

        public ProductionSettings Settings;

        private ProductionBuildingPanel _parentPanel;
        private const int DEFAULT_LIMIT = 10;

        public void Init(ProductionSettings productionSettings, int priority, ProductionBuildingPanel parentPanel)
        {
            Settings = productionSettings;
            _parentPanel = parentPanel;

            _priorityText.text = priority.ToString();
            RefreshPlayPause();

            _priorityUpBtn.SetActive(priority > 1);
            _priorityDownBtn.SetActive(!_parentPanel.IsSettingLastPriority(Settings));
            _productDisplay.Init(Settings.CraftedItem, 1);

            int index = 1;
            foreach (var cost in Settings.CraftedItem.GetResourceCosts())
            {
                if (index == 1)
                {
                    _materialDisplay.Init(cost.Item, cost.Quantity);
                }
                else
                {
                    ProductionMaterialDisplay additionalDisplay = Instantiate(_materialDisplay, _materialParent);
                    additionalDisplay.Init(cost.Item, cost.Quantity);
                    int siblingIndex = _materialDisplay.transform.GetSiblingIndex();
                    additionalDisplay.transform.SetSiblingIndex(siblingIndex);
                }

                index++;
            }

            RefreshLimitDisplay();
        }

        public void RefreshValues(float progressPercent)
        {
            _progressFill.fillAmount = progressPercent;
        }

        private void RefreshPlayPause()
        {
            _playPauseBtnIcon.sprite = Settings.IsPaused ? _playIcon : _pauseIcon;
            _playPauseBtnIcon.color = Settings.IsPaused ? _pausedRed : _defaultWhite;
            _playPauseTooltip.Content = Settings.IsPaused ? "Resume Production" : "Pause Production";
        }

        private void RefreshLimitDisplay()
        {
            if (Settings.HasLimit)
            {
                _limitInputField.SetTextWithoutNotify($"{Settings.Limit}");
                _limitInputTooltip.Header = "Click to assign limit";
                _enableLimitToggle.SetIsOnWithoutNotify(true);
                _limitBG.sprite = _activeLimitBGSpr;
                _limitToggleTooltip.Header = "Disable Limit";
            }
            else // No Limit
            {
                _limitInputField.SetTextWithoutNotify("No Limit");
                _limitInputTooltip.Header = "Click to assign limit";
                _enableLimitToggle.SetIsOnWithoutNotify(false);
                _limitBG.sprite = _inactiveLimitBGSpr;
                _limitToggleTooltip.Header = "Enable Limit";
            }
        }
        
        public void OnIncreaseLimitPressed()
        {
            if (!Settings.HasLimit)
            {
                Settings.Limit = DEFAULT_LIMIT;
                Settings.HasLimit = true;
            }
            else
            {
                Settings.Limit++;
            }
            
            RefreshLimitDisplay();
        }

        public void OnDecreaseLimitPressed()
        {
            if (!Settings.HasLimit)
            {
                Settings.Limit = DEFAULT_LIMIT;
                Settings.HasLimit = true;
            }
            else
            {
                if (Settings.Limit > 0)
                {
                    Settings.Limit--;
                }
            }
            
            RefreshLimitDisplay();
        }
        
        public void OnLimitInputFieldChanged(string valueString)
        {
            bool parseSuccess = int.TryParse(valueString, out var value);
            if (parseSuccess)
            {
                Settings.Limit = value;
                Settings.HasLimit = true;
            }
            else
            {
                Settings.HasLimit = false;
            }
            
            RefreshLimitDisplay();
        }

        public void OnEnableLimitTogglePressed(bool value)
        {
            Settings.HasLimit = value;
            RefreshLimitDisplay();
        }

        public void OnIncreasePriorityPressed()
        {
            _parentPanel.IncreaseProductionPriority(Settings);
        }

        public void OnDecreasePriorityPressed()
        {
            _parentPanel.DecreaseProductionPriority(Settings);
        }

        public void OnPlayPausePressed()
        {
            Settings.IsPaused = !Settings.IsPaused;
            RefreshPlayPause();
        }
    }
}
