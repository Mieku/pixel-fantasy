using System;
using System.Collections.Generic;
using Managers;
using TaskSystem;
using UnityEngine;

namespace Systems.Details.Controls_Details.Scripts
{
    public class JobPrioritiesMenu : ControlsMenu
    {
        [SerializeField] private JobPrioritiesDisplay _prioritiesDisplayPrefab;
        [SerializeField] private Transform _prioritiesLayout;

        private List<JobPrioritiesDisplay> _displayedPriorities = new List<JobPrioritiesDisplay>();

        private void Awake()
        {
            _prioritiesDisplayPrefab.gameObject.SetActive(false);
        }

        public override void Show()
        {
            base.Show();
            ClearDisplayedPriorities();
            
            var kinlings = KinlingsDatabase.Instance.GetKinlingsDataList();
            foreach (var kinling in kinlings)
            {
                var display = Instantiate(_prioritiesDisplayPrefab, transform);
                display.transform.SetSiblingIndex(_prioritiesDisplayPrefab.transform.GetSiblingIndex());
                display.gameObject.SetActive(true);
                display.Init(kinling);
                _displayedPriorities.Add(display);
            }
        }

        public override void Hide()
        {
            base.Hide();
            
            ClearDisplayedPriorities();
        }

        public void HighlightTaskType(ETaskType taskType, bool shouldHighlight)
        {
            foreach (var displayedPriority in _displayedPriorities)
            {
                displayedPriority.HighlightTaskType(taskType, shouldHighlight);
            }
        }

        private void ClearDisplayedPriorities()
        {
            foreach (var priority in _displayedPriorities)
            {
                Destroy(priority.gameObject);
            }
            _displayedPriorities.Clear();
        }
    }
}
