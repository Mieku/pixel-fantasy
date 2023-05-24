using System;
using System.Collections.Generic;
using Characters;
using UnityEngine;

namespace Popups.Kinling_Info_Popup
{
    public class KinlingScheduleDisplay : MonoBehaviour
    {
        [SerializeField] private KinlingScheduleOption _sleepOption;
        [SerializeField] private KinlingScheduleOption _workOption;
        [SerializeField] private KinlingScheduleOption _recreationOption;
        [SerializeField] private Color _sleepColour;
        [SerializeField] private Color _workColour;
        [SerializeField] private Color _recreationColour;

        [SerializeField] private List<KinlingScheduleHourDisplay> _hours;

        private KinlingScheduleOption _currentSelected;
        private Unit _unit;

        public void Refresh(Unit unit)
        {
            OptionSelected(_sleepOption);
            _unit = unit;
            RefreshDisplayedHours();
        }

        public void OptionSelected(KinlingScheduleOption option)
        {
            if (_currentSelected != null)
            {
                _currentSelected.SetSelected(false);
            }

            _currentSelected = option;
            _currentSelected.SetSelected(true);
        }

        public void OnHourClicked(int hour, KinlingScheduleHourDisplay hourDisplay)
        {
            hourDisplay.SetColour(GetCurOptionColour());
            _unit.GetUnitState().Schedule.SetHour(hour, _currentSelected.Option);
            RefreshDisplayedHours();
        }

        private void RefreshDisplayedHours()
        {
            var schedule = _unit.GetUnitState().Schedule;
            foreach (var hour in _hours)
            {
                var option = schedule.GetHour(hour.Hour);
                hour.SetColour(GetOptionColour(option));
            }
        }

        private Color GetCurOptionColour()
        {
            return GetOptionColour(_currentSelected.Option);
        }

        private Color GetOptionColour(ScheduleOption option)
        {
            switch (option)
            {
                case ScheduleOption.Sleep:
                    return _sleepColour;
                case ScheduleOption.Work:
                    return _workColour;
                case ScheduleOption.Recreation:
                    return _recreationColour;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}
