using System;
using Characters;
using Managers;
using Popups.Change_Job_Popup;
using TMPro;
using UnityEngine;
using UnityEngine.UI;


namespace Popups.Kinling_Info_Popup
{
    public class KinlingInfoPopup : Popup<KinlingInfoPopup>
    {
        [SerializeField] private TextMeshProUGUI _kinlingNameDisp;
        [SerializeField] private TextMeshProUGUI _jobName;
        [SerializeField] private Image _jobIcon;
        [SerializeField] private Image _jobExpFill;

        [Header("Tabs")]
        [SerializeField] private Image _moodTab;
        [SerializeField] private Image _healthTab;
        [SerializeField] private Image _jobsTab;
        [SerializeField] private Image _needsTab;
        [SerializeField] private Image _wantsTab;
        [SerializeField] private Image _socialTab;
        [SerializeField] private Image _gearTab;
        [SerializeField] private Sprite _tabSelectedSpr, _tabUnselectedSpr;
        
        [Header("Content")]
        [SerializeField] private KinlingInfoMoodContent _moodContent;
        [SerializeField] private GameObject _healthContent;
        [SerializeField] private KinlingInfoJobContent _jobsContent;
        [SerializeField] private KinlingInfoNeedsContent _needsContent;
        [SerializeField] private GameObject _wantsContent;
        [SerializeField] private KinlingInfoSocialContent _socialContent;
        [SerializeField] private KinlingInfoGearContent _gearContent;

        private static Unit _selectedUnit;
        private const float _refreshRateS = 1f;
        private float _refreshTimer;
        
        public static void Show(Unit unit)
        {
            Open(() => Instance.Init(unit), false);
        }
    
        public override void OnBackPressed()
        {
            HideAllTabContent();
            Hide();
        }

        private void Init(Unit unit)
        {
            _selectedUnit = unit;
            Refresh();
            ShowTabContent(KinlingInfoTab.Needs);
        }
        
        private void Refresh()
        {
            _kinlingNameDisp.text = _selectedUnit.GetUnitState().FullName;
            _jobName.text = _selectedUnit.GetUnitState().CurrentJob.JobNameWithTitle;
            if (_selectedUnit.GetUnitState().CurrentJob.JobData.JobIcon != null)
            {
                _jobIcon.sprite = _selectedUnit.GetUnitState().CurrentJob.JobData.JobIcon;
            }
            else
            {
                _jobIcon.sprite = Librarian.Instance.GetSprite("Question Mark");
            }

            var percentExp = _selectedUnit.GetUnitState().CurrentJob.CurrentLevelProgress();
            _jobExpFill.fillAmount = percentExp;
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
                    _moodTab.sprite = _tabSelectedSpr;
                    _moodContent.Show(_selectedUnit);
                    break;
                case KinlingInfoTab.Health:
                    _healthTab.sprite = _tabSelectedSpr;
                    _healthContent.SetActive(true);
                    break;
                case KinlingInfoTab.Job:
                    _jobsTab.sprite = _tabSelectedSpr;
                    _jobsContent.Show(_selectedUnit);
                    break;
                case KinlingInfoTab.Needs:
                    _needsTab.sprite = _tabSelectedSpr;
                    _needsContent.Show(_selectedUnit);
                    break;
                case KinlingInfoTab.Wants:
                    _wantsTab.sprite = _tabSelectedSpr;
                    _wantsContent.SetActive(true);
                    break;
                case KinlingInfoTab.Social:
                    _socialTab.sprite = _tabSelectedSpr;
                    _socialContent.Show(_selectedUnit);
                    break;
                case KinlingInfoTab.Gear:
                    _gearTab.sprite = _tabSelectedSpr;
                    _gearContent.Show(_selectedUnit);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(tab), tab, null);
            }
        }

        private void HideAllTabContent()
        {
            _moodTab.sprite = _tabUnselectedSpr;
            _healthTab.sprite = _tabUnselectedSpr;
            _jobsTab.sprite = _tabUnselectedSpr;
            _needsTab.sprite = _tabUnselectedSpr;
            _wantsTab.sprite = _tabUnselectedSpr;
            _socialTab.sprite = _tabUnselectedSpr;
            _gearTab.sprite = _tabUnselectedSpr;

            _moodContent.Close();
            _healthContent.SetActive(false);
            _jobsContent.Close();
            _needsContent.Close();
            _wantsContent.SetActive(false);
            _socialContent.Close();
            _gearContent.Close();
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
            ShowTabContent(KinlingInfoTab.Job);
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
        
        public enum KinlingInfoTab
        {
            Mood,
            Health,
            Job,
            Needs,
            Wants,
            Social,
            Gear,
        }
        
        #endregion
    }
}
