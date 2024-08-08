using System;
using System.Collections.Generic;
using Characters;
using Managers;
using UnityEngine;
using UnityEngine.Serialization;

namespace Systems.Details.Controls_Details.Scripts
{
    public class ScheduleMenu : ControlsMenu
    {
        [SerializeField] private PaintScheduleBtn _anythingPaintBtn;
        [SerializeField] private PaintScheduleBtn _sleepPaintBtn;
        [SerializeField] private PaintScheduleBtn _workPaintBtn;
        [SerializeField] private PaintScheduleBtn _recreationPaintBtn;

        [SerializeField] private ScheduleDisplay _scheduleDisplayPrefab;

        private List<ScheduleDisplay> _displayedSchedules = new List<ScheduleDisplay>();
        private ScheduleOption? _currentSelectedScheduleOption;
        private ScheduleData _copiedScheduleData = null;

        public Action<ScheduleOption?> OnCurrentSelectedOptionChanged;
        public Action<ScheduleData> OnScheduleDataCopied;

        private void Awake()
        {
            _scheduleDisplayPrefab.gameObject.SetActive(false);
        }

        public override void Show()
        {
            base.Show();
            _currentSelectedScheduleOption = null;
            
            ClearPaintOptionHighlights();
            ClearDisplayedSchedules();
            DisplayKinlingSchedules();

            if (_copiedScheduleData != null)
            {
                OnScheduleDataCopied?.Invoke(_copiedScheduleData);
            }
        }

        public override void Hide()
        {
            base.Hide();
            _currentSelectedScheduleOption = null;
            
            ClearDisplayedSchedules();
        }

        private void Update()
        {
            if (Input.GetMouseButtonDown(1) && _currentSelectedScheduleOption != null) 
            {
                ClearPaintOptionHighlights();
                _currentSelectedScheduleOption = null;
                OnCurrentSelectedOptionChanged?.Invoke(null);
            }
        }

        public void OnPaintSchedulePressed(ScheduleOption selectedOption)
        {
            HighlightPaintOption(selectedOption);
            _currentSelectedScheduleOption = selectedOption;
            
            OnCurrentSelectedOptionChanged?.Invoke(selectedOption);
        }

        private void HighlightPaintOption(ScheduleOption scheduleOption)
        {
            ClearPaintOptionHighlights();
            
            switch (scheduleOption)
            {
                case ScheduleOption.Sleep:
                    _sleepPaintBtn.SetHighlight(true);
                    break;
                case ScheduleOption.Work:
                    _workPaintBtn.SetHighlight(true);
                    break;
                case ScheduleOption.Recreation:
                    _recreationPaintBtn.SetHighlight(true);
                    break;
                case ScheduleOption.Anything:
                    _anythingPaintBtn.SetHighlight(true);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(scheduleOption), scheduleOption, null);
            }
        }

        private void ClearPaintOptionHighlights()
        {
            _anythingPaintBtn.SetHighlight(false);
            _sleepPaintBtn.SetHighlight(false);
            _workPaintBtn.SetHighlight(false);
            _recreationPaintBtn.SetHighlight(false);
        }

        private void DisplayKinlingSchedules()
        {
            var kinlings = KinlingsDatabase.Instance.GetKinlingsDataList();
            foreach (var kinling in kinlings)
            {
                var display = Instantiate(_scheduleDisplayPrefab, transform);
                display.transform.SetSiblingIndex(_scheduleDisplayPrefab.transform.GetSiblingIndex());
                display.gameObject.SetActive(true);
                display.Init(kinling, this);
                _displayedSchedules.Add(display);
            }
        }

        private void ClearDisplayedSchedules()
        {
            foreach (var display in _displayedSchedules)
            {
                Destroy(display.gameObject);
            }
            _displayedSchedules.Clear();
        }

        public void OnScheduleCopied(ScheduleData copiedScheduleData)
        {
            _copiedScheduleData = new ScheduleData();
            for (int hour = 0; hour < 24; hour++)
            {
                var schedule = copiedScheduleData.GetHour(hour);
                _copiedScheduleData.SetHour(hour, schedule);
            }
            
            OnScheduleDataCopied?.Invoke(_copiedScheduleData);
        }
    }
}
