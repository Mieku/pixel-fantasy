using System.Collections.Generic;
using Characters;
using Popups.Change_Job_Popup;
using UnityEngine;

namespace Popups.Kinling_Info_Popup
{
    public class KinlingInfoJobContent : MonoBehaviour
    {
        [SerializeField] private Transform _prevJobSkillsParent;
        [SerializeField] private JobSkillsPanel _curJobPanel;
        
        private Unit _unit;
        private List<JobSkillsPanel> _displayedRetiredJobs = new List<JobSkillsPanel>();

        public void Show(Unit unit)
        {
            _unit = unit;
            
            gameObject.SetActive(true);
            RefreshJobs();
        }

        public void Close()
        {
            gameObject.SetActive(false);
        }

        private void RefreshJobs()
        {
            _curJobPanel.Init(_unit.GetUnitState().CurrentJob);

            foreach (var displayedRetired in _displayedRetiredJobs)
            {
                Destroy(displayedRetired.gameObject);
            }
            _displayedRetiredJobs.Clear();

            foreach (var job in _unit.GetUnitState().JobHistory)
            {
                if (!job.IsCurrentJob)
                {
                    var jobPanel = Instantiate(_curJobPanel, _prevJobSkillsParent);
                    jobPanel.Init(job);
                    jobPanel.transform.localScale = new Vector3(0.9f, 0.9f, 0.9f);
                    _displayedRetiredJobs.Add(jobPanel);
                }
            }
        }

        public void ChangeJobPressed()
        {
            ChangeJobPopup.Show(_unit);
        }
    }
}
