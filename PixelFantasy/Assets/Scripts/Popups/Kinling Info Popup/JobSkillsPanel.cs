using Characters;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Popups.Kinling_Info_Popup
{
    public class JobSkillsPanel : MonoBehaviour
    {
        [SerializeField] private Image _panelBG;
        [SerializeField] private TextMeshProUGUI _jobNameText;
        [SerializeField] private Material _greyScaleMat;
        [SerializeField] private Sprite _currentBGSpr;
        [SerializeField] private Sprite _retiredBGSpr;
        [SerializeField] private GameObject _skillsLayout;
        [SerializeField] private GameObject _noSkillsMsg;
        
        [Header("Skill Icons")]
        [SerializeField] private Image _lv1SkillIcon;
        [SerializeField] private Image _lv2SkillIcon;
        [SerializeField] private Image _lv3SkillIcon;
        [SerializeField] private Image _lv4SkillIcon;
        [SerializeField] private Image _lv5SkillIcon;

        [Header("Bar Fills")] 
        [SerializeField] private Image _lv1BarFill;
        [SerializeField] private Image _lv2BarFill;
        [SerializeField] private Image _lv3BarFill;
        [SerializeField] private Image _lv4BarFill;

        private JobState _job;

        public void Init(JobState job)
        {
            _job = job;
            Refresh();
        }

        private void Refresh()
        {
            string jobTitle = $"{_job.LevelTitle} {_job.JobData.JobName}";
            if (_job.IsCurrentJob)
            {
                _panelBG.sprite = _currentBGSpr;
            }
            else
            {
                _panelBG.sprite = _retiredBGSpr;
                jobTitle += " (Retired)";
            }

            _jobNameText.text = jobTitle;
            if (_job.JobData.HasSkills)
            {
                _skillsLayout.SetActive(true);
                _noSkillsMsg.SetActive(false);
                RefreshSkillDisplay();
            }
            else
            {
                _skillsLayout.SetActive(false);
                _noSkillsMsg.SetActive(true);
            }
        }

        private void RefreshSkillDisplay()
        {
            _lv1SkillIcon.sprite = _job.JobData.Lv1Skill.SkillIcon;
            _lv2SkillIcon.sprite = _job.JobData.Lv2Skill.SkillIcon;
            _lv3SkillIcon.sprite = _job.JobData.Lv3Skill.SkillIcon;
            _lv4SkillIcon.sprite = _job.JobData.Lv4Skill.SkillIcon;
            _lv5SkillIcon.sprite = _job.JobData.Lv5Skill.SkillIcon;
            
            int jobLv = _job.CurrentLevel;
            _lv5SkillIcon.material = null;
            _lv4SkillIcon.material = null;
            _lv3SkillIcon.material = null;
            _lv4SkillIcon.material = null;
            
            if (jobLv == 5)
            {
                _lv1SkillIcon.material = null;
                _lv2SkillIcon.material = null;
                _lv3SkillIcon.material = null;
                _lv4SkillIcon.material = null;
                _lv5SkillIcon.material = null;
                
                _lv1BarFill.fillAmount = 1f;
                _lv2BarFill.fillAmount = 1f;
                _lv3BarFill.fillAmount = 1f;
                _lv4BarFill.fillAmount = 1f;
            }

            if (jobLv == 4)
            {
                _lv1SkillIcon.material = null;
                _lv2SkillIcon.material = null;
                _lv3SkillIcon.material = null;
                _lv4SkillIcon.material = null;
                _lv5SkillIcon.material = _greyScaleMat;
                
                _lv1BarFill.fillAmount = 1f;
                _lv2BarFill.fillAmount = 1f;
                _lv3BarFill.fillAmount = 1f;
                _lv4BarFill.fillAmount = _job.CurrentLevelProgress();
            }
            
            if (jobLv == 3)
            {
                _lv1SkillIcon.material = null;
                _lv2SkillIcon.material = null;
                _lv3SkillIcon.material = null;
                _lv4SkillIcon.material = _greyScaleMat;
                _lv5SkillIcon.material = _greyScaleMat;
                
                _lv1BarFill.fillAmount = 1f;
                _lv2BarFill.fillAmount = 1f;
                _lv3BarFill.fillAmount = _job.CurrentLevelProgress();
                _lv4BarFill.fillAmount = 0f;
            }
            
            if (jobLv == 2)
            {
                _lv1SkillIcon.material = null;
                _lv2SkillIcon.material = null;
                _lv3SkillIcon.material = _greyScaleMat;
                _lv4SkillIcon.material = _greyScaleMat;
                _lv5SkillIcon.material = _greyScaleMat;
                
                _lv1BarFill.fillAmount = 1f;
                _lv2BarFill.fillAmount = _job.CurrentLevelProgress();
                _lv3BarFill.fillAmount = 0f;
                _lv4BarFill.fillAmount = 0f;
            }

            if (jobLv == 1)
            {
                _lv1SkillIcon.material = null;
                _lv2SkillIcon.material = _greyScaleMat;
                _lv3SkillIcon.material = _greyScaleMat;
                _lv4SkillIcon.material = _greyScaleMat;
                _lv5SkillIcon.material = _greyScaleMat;
                
                _lv1BarFill.fillAmount = _job.CurrentLevelProgress();
                _lv2BarFill.fillAmount = 0f;
                _lv3BarFill.fillAmount = 0f;
                _lv4BarFill.fillAmount = 0f;
            }
        }
    }
}
