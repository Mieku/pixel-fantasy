using System.Collections.Generic;
using System.Linq;
using Characters;
using Managers;
using ScriptableObjects;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Popups.Change_Job_Popup
{
    public class ChangeJobPopup : Popup<ChangeJobPopup>
    {
        [Header("Side Panel")] 
        [SerializeField] private TextMeshProUGUI _kinlingName;
        [SerializeField] private TextMeshProUGUI _kinlingJobName;
        
        [Header("Selected Job Details")]
        [SerializeField] private TextMeshProUGUI _selectedJobName;
        [SerializeField] private TextMeshProUGUI _selectedJobDetails;
        [SerializeField] private GameObject _skillsHandle;
        [SerializeField] private Image _skill1Icon;
        [SerializeField] private Image _skill2Icon;
        [SerializeField] private Image _skill3Icon;
        [SerializeField] private Image _skill4Icon;
        [SerializeField] private Image _skill5Icon;

        [Header("Requirements")] 
        [SerializeField] private GameObject _toolRequirementHandle;
        [SerializeField] private GameObject _jobRequirementHandle;
        [SerializeField] private GameObject _noRequirementHandle;
        [SerializeField] private TextMeshProUGUI _toolRequirementText;
        [SerializeField] private Image _toolRequirementIcon;
        [SerializeField] private TextMeshProUGUI _jobRequirementText;
        [SerializeField] private Color _availableGreen;
        [SerializeField] private Color _unavailableRed;

        [Header("Job Options")] 
        [SerializeField] private Transform _jobOptionParent;
        
        private Unit _unit;
        private JobData _curJob;
        private ChangeJobOption _curSelectedOption;
        private List<ChangeJobOption> _allOptions = new List<ChangeJobOption>();

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
            _unit = unit;
            RefreshJobs();
            SelectCurrentJob();
        }

        private void SelectCurrentJob()
        {
            var job = _unit.GetUnitState().CurrentJob.JobData;
            foreach (var option in _allOptions)
            {
                if (option.Job == job)
                {
                    OnJobSelected(job, option);
                    return;
                }
            }
        }

        private void RefreshJobs()
        {
            _kinlingName.text = _unit.GetUnitState().FullName;
            _kinlingJobName.text = _unit.GetUnitState().CurrentJob.JobNameWithTitle;
            
            _allOptions.Clear();
            _allOptions = _jobOptionParent.GetComponentsInChildren<ChangeJobOption>().ToList();
            foreach (var option in _allOptions)
            {
                var job = option.Job;
                if (job != null)
                {
                    if (_unit.GetUnitState().CurrentJob.JobData == job)
                    {
                        option.Init(ChangeJobOption.JobOptionState.Current, OnJobSelected);
                    }
                    else if (IsToolAvailable(job) && IsJobRequirementFulfilled(job))
                    {
                        option.Init(ChangeJobOption.JobOptionState.Available, OnJobSelected);
                    }
                    else
                    {
                        option.Init(ChangeJobOption.JobOptionState.Unavailable, OnJobSelected);
                    }
                }
                else
                {
                    option.Init(ChangeJobOption.JobOptionState.Unavailable, OnJobSelected);
                }
            }
        }

        private bool IsToolAvailable(JobData job)
        {
            var reqTool = job.RequiredTool;
            if (reqTool == null) return true;

            if (InventoryManager.Instance.IsItemInStorage(reqTool))
            {
                return true;
            }
            
            return false;
        }

        private bool IsJobRequirementFulfilled(JobData job)
        {
            var reqJob = job.PrereqJob;
            if (reqJob == null) return true;
            
            var reqLv = job.PrereqJobLv;
            var jobHistory = _unit.GetUnitState().JobHistory;
            foreach (var prevJob in jobHistory)
            {
                if (prevJob.JobData == job && prevJob.CurrentLevel >= reqLv)
                {
                    return true;
                }
            }
            
            
            return false;
        }

        public void ChangeJobPressed()
        {
            _unit.GetUnitState().ChangeJob(_curJob);
            RefreshJobs();
        }
        
        public void OnJobSelected(JobData jobData, ChangeJobOption optionPressed)
        {
            // Update Selector
            if (_curSelectedOption != null)
            {
                _curSelectedOption.ToggleSelected(false);
            }
            _curSelectedOption = optionPressed;
            _curSelectedOption.ToggleSelected(true);
            _curJob = jobData;

            // Update the Requirements
            RefreshRequirements(jobData);
            
            // Update the Selected Job Details
            RefreshSelectedJobDetails(jobData);
        }

        private void RefreshRequirements(JobData job)
        {
            if (job.PrereqJob != null)
            {
                _jobRequirementHandle.SetActive(true);
                _jobRequirementText.text = job.PrereqJob.GetLevelName(job.PrereqJobLv) + job.PrereqJob.JobName;
                if (IsJobRequirementFulfilled(job.PrereqJob))
                {
                    _jobRequirementText.color = _availableGreen;
                }
                else
                {
                    _jobRequirementText.color = _unavailableRed;
                }
            }
            else
            {
                _jobRequirementHandle.SetActive(false);
            }

            if (job.RequiredTool != null)
            {
                _toolRequirementHandle.SetActive(true);
                _toolRequirementIcon.sprite = job.RequiredTool.ItemSprite;
                _toolRequirementText.text = job.RequiredTool.ItemName;
                if (IsToolAvailable(job))
                {
                    _toolRequirementText.color = _availableGreen;
                }
                else
                {
                    _toolRequirementText.color = _unavailableRed;
                }
            }
            else
            {
                _toolRequirementHandle.SetActive(false);
            }

            if (job.RequiredTool == null && job.PrereqJob == null)
            {
                _noRequirementHandle.SetActive(true);
            }
            else
            {
                _noRequirementHandle.SetActive(false);
            }
        }

        private void RefreshSelectedJobDetails(JobData job)
        {
            _selectedJobName.text = job.JobName;
            _selectedJobDetails.text = job.JobDescription;

            if (job.HasSkills)
            {
                _skillsHandle.SetActive(true);
                _skill1Icon.sprite = job.Lv1Skill.SkillIcon;
                _skill2Icon.sprite = job.Lv2Skill.SkillIcon;
                _skill3Icon.sprite = job.Lv3Skill.SkillIcon;
                _skill4Icon.sprite = job.Lv4Skill.SkillIcon;
                _skill5Icon.sprite = job.Lv5Skill.SkillIcon;
            }
            else
            {
                _skillsHandle.SetActive(false);
            }
        }
    }
}
