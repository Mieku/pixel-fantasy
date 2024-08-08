using System;
using Characters;
using TMPro;
using UnityEngine;

namespace Systems.Details.Controls_Details.Scripts
{
    public class ScheduleDisplay : MonoBehaviour
    {
        [SerializeField] private HourDisplay[] _hourDisplays;
        [SerializeField] private TextMeshProUGUI _kinlingNickname;
        [SerializeField] private GameObject _pasteBtnHandle;

        private KinlingData _kinlingData;
        private ScheduleMenu _scheduleMenu;
        private ScheduleData _availableCopiedScheduleData;

        public void Init(KinlingData kinlingData, ScheduleMenu scheduleMenu)
        {
            _scheduleMenu = scheduleMenu;
            _kinlingData = kinlingData;
            _kinlingNickname.text = _kinlingData.Nickname;
            RefreshSchedule();

            _scheduleMenu.OnCurrentSelectedOptionChanged += OnPaintOptionSelected;
            _scheduleMenu.OnScheduleDataCopied += OnScheduleDataCopied;
        }

        private void OnDestroy()
        {
            _scheduleMenu.OnCurrentSelectedOptionChanged -= OnPaintOptionSelected;
            _scheduleMenu.OnScheduleDataCopied -= OnScheduleDataCopied;
        }

        public void RefreshSchedule()
        {
            for (int hour = 0; hour < 24; hour++)
            {
                var schedule = _kinlingData.Schedule.GetHour(hour);
                _hourDisplays[hour].Init(hour, schedule, this);
            }
        }

        public void AssignScheduleOption(ScheduleOption scheduleOption, int hour)
        {
            _kinlingData.Schedule.SetHour(hour, scheduleOption);
        }
        
        private void OnPaintOptionSelected(ScheduleOption? selectedOption)
        {
            foreach (var hourDisplay in _hourDisplays)
            {
                hourDisplay.OnPaintOptionSelected(selectedOption);
            }
        }

        private void OnScheduleDataCopied(ScheduleData copiedData)
        {
            _availableCopiedScheduleData = copiedData;
            if (copiedData == null)
            {
                _pasteBtnHandle.SetActive(false);
            }
            else
            {
                _pasteBtnHandle.SetActive(true);
            }
        }

        public void OnCopyPressed()
        {
            _scheduleMenu.OnScheduleCopied(_kinlingData.Schedule);
        }

        public void OnPastePressed()
        {
            if (_availableCopiedScheduleData == null) return;
            
            for (int hour = 0; hour < 24; hour++)
            {
                var schedule = _availableCopiedScheduleData.GetHour(hour);
                AssignScheduleOption(schedule, hour);
            }
            
            RefreshSchedule();
        }
    }
}
