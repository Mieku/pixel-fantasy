using System;
using Characters;
using TMPro;
using UnityEngine;

namespace Popups
{
    public class KinlingInfoPopup : Popup<KinlingInfoPopup>
    {
        [SerializeField] private TextMeshProUGUI _kinlingNameDisp;

        [Header("Tabs")]
        [SerializeField] private GameObject _moodTab, _moodTabOff, _moodContent;
        [SerializeField] private GameObject _healthTab, _healthTabOff, _healthContent;
        [SerializeField] private GameObject _jobsTab, _jobsTabOff, _jobsContent;
        [SerializeField] private GameObject _needsTab, _needsTabOff, _needsContent;
        [SerializeField] private GameObject _wantsTab, _wantsTabOff, _wantsContent;
        [SerializeField] private GameObject _socialTab, _socialTabOff, _socialContent;
        [SerializeField] private GameObject _gearTab, _gearTabOff, _gearContent;
        [SerializeField] private GameObject _statsTab, _statsTabOff, _statsContent;

        [Header("Stats")] 
        [SerializeField] private TextMeshProUGUI _speedDisp;
        [SerializeField] private TextMeshProUGUI _productivityDisp;
        [SerializeField] private TextMeshProUGUI _healingDisp;
        [SerializeField] private TextMeshProUGUI _aimDisp;
        [SerializeField] private TextMeshProUGUI _toughnessDisp;
        [SerializeField] private TextMeshProUGUI _combatDisp;
        
        private static Unit _selectedUnit;
        private const float _refreshRateS = 1f;
        private float _refreshTimer;
        
        public static void Show(Unit unit)
        {
            Open(() => Instance.Init(unit), false);
        }
    
        public override void OnBackPressed()
        {
            Hide();
        }

        private void Init(Unit unit)
        {
            _selectedUnit = unit;
            Refresh();
            ShowTabContent(KinlingInfoTab.Mood);
        }
        
        private void Refresh()
        {
            _kinlingNameDisp.text = _selectedUnit.GetUnitState().FullName;
            
            RefreshStatsContent();
        }

        private void RefreshStatsContent()
        {
            _speedDisp.text = ConvertStatToString(_selectedUnit.GetUnitState().SpeedModifier);
            _productivityDisp.text = ConvertStatToString(_selectedUnit.GetUnitState().ProductivityModifier);
            _healingDisp.text = ConvertStatToString(_selectedUnit.GetUnitState().HealingModifier);
            _aimDisp.text = ConvertStatToString(_selectedUnit.GetUnitState().AimModifier);
            _toughnessDisp.text = ConvertStatToString(_selectedUnit.GetUnitState().ToughnessModifier);
            _combatDisp.text = ConvertStatToString(_selectedUnit.GetUnitState().CombatModifier);
        }

        private string ConvertStatToString(float statModifier)
        {
            string value = $"<color=\"white\">+{statModifier * 100}%</color>";
            if (statModifier > 0)
            {
                value = $"<color=\"green\">+{statModifier * 100}%</color>";
            } else if (statModifier < 0)
            {
                value = $"<color=\"red\">{statModifier * 100}%</color>";
            }
            
            return value;
        }

        private void Update()
        {
            if (_selectedUnit != null)
            {
                _refreshTimer += Time.deltaTime;
                if (_refreshTimer > _refreshRateS)
                {
                    _refreshTimer = 0;
                    Refresh();
                }
            }
        }

        #region Tab Controls

        private void ShowTabContent(KinlingInfoTab tab)
        {
            HideAllTabContent();
            
            switch (tab)
            {
                case KinlingInfoTab.Mood:
                    _moodTab.SetActive(true);
                    _moodTabOff.SetActive(false);
                    _moodContent.SetActive(true);
                    break;
                case KinlingInfoTab.Health:
                    _healthTab.SetActive(true);
                    _healthTabOff.SetActive(false);
                    _healthContent.SetActive(true);
                    break;
                case KinlingInfoTab.Jobs:
                    _jobsTab.SetActive(true);
                    _jobsTabOff.SetActive(false);
                    _jobsContent.SetActive(true);
                    break;
                case KinlingInfoTab.Needs:
                    _needsTab.SetActive(true);
                    _needsTabOff.SetActive(false);
                    _needsContent.SetActive(true);
                    break;
                case KinlingInfoTab.Wants:
                    _wantsTab.SetActive(true);
                    _wantsTabOff.SetActive(false);
                    _wantsContent.SetActive(true);
                    break;
                case KinlingInfoTab.Social:
                    _socialTab.SetActive(true);
                    _socialTabOff.SetActive(false);
                    _socialContent.SetActive(true);
                    break;
                case KinlingInfoTab.Gear:
                    _gearTab.SetActive(true);
                    _gearTabOff.SetActive(false);
                    _gearContent.SetActive(true);
                    break;
                case KinlingInfoTab.Stats:
                    _statsTab.SetActive(true);
                    _statsTabOff.SetActive(false);
                    _statsContent.SetActive(true);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(tab), tab, null);
            }
        }

        private void HideAllTabContent()
        {
            _moodTab.SetActive(false);
            _healthTab.SetActive(false);
            _jobsTab.SetActive(false);
            _needsTab.SetActive(false);
            _wantsTab.SetActive(false);
            _socialTab.SetActive(false);
            _gearTab.SetActive(false);
            _statsTab.SetActive(false);
            
            _moodTabOff.SetActive(true);
            _healthTabOff.SetActive(true);
            _jobsTabOff.SetActive(true);
            _needsTabOff.SetActive(true);
            _wantsTabOff.SetActive(true);
            _socialTabOff.SetActive(true);
            _gearTabOff.SetActive(true);
            _statsTabOff.SetActive(true);
            
            _moodContent.SetActive(false);
            _healthContent.SetActive(false);
            _jobsContent.SetActive(false);
            _needsContent.SetActive(false);
            _wantsContent.SetActive(false);
            _socialContent.SetActive(false);
            _gearContent.SetActive(false);
            _statsContent.SetActive(false);
        }

        public void MoodTabPressed()
        {
            ShowTabContent(KinlingInfoTab.Mood);
        }
        
        public void HealthTabPressed()
        {
            ShowTabContent(KinlingInfoTab.Health);
        }
        
        public void JobsTabPressed()
        {
            ShowTabContent(KinlingInfoTab.Jobs);
        }
        
        public void NeedsTabPressed()
        {
            ShowTabContent(KinlingInfoTab.Needs);
        }
        
        public void WantsTabPressed()
        {
            ShowTabContent(KinlingInfoTab.Wants);
        }
        
        public void SocialTabPressed()
        {
            ShowTabContent(KinlingInfoTab.Social);
        }
        
        public void GearTabPressed()
        {
            ShowTabContent(KinlingInfoTab.Gear);
        }
        
        public void StatsTabPressed()
        {
            ShowTabContent(KinlingInfoTab.Stats);
        }
        
        public enum KinlingInfoTab
        {
            Mood,
            Health,
            Jobs,
            Needs,
            Wants,
            Social,
            Gear,
            Stats,
        }
        
        #endregion
    }
}
