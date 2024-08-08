using System;
using Characters;
using ScriptableObjects;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Systems.Details.Controls_Details.Scripts
{
    public class HourDisplay : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler
    {
        [SerializeField] private Image _colourDisplay;
        [SerializeField] private Image _frame;

        [SerializeField] private Color _defaultFrameColour;
        [SerializeField] private Color _highlightedFrameColour;

        private int _assignedHour;
        private ScheduleDisplay _scheduleDisplay;
        private ScheduleOption _assignedScheduleOption;
        private ScheduleOption? _currentSelectedOption;

        private void Awake()
        {
            _frame.color = _defaultFrameColour;
        }

        public void Init(int hour, ScheduleOption scheduleOption, ScheduleDisplay scheduleDisplay)
        {
            _scheduleDisplay = scheduleDisplay;
            _assignedHour = hour;
            _assignedScheduleOption = scheduleOption;
            _colourDisplay.color = GetOptionColour(scheduleOption);
        }

        private Color GetOptionColour(ScheduleOption scheduleOption)
        {
            switch (scheduleOption)
            {
                case ScheduleOption.Sleep:
                    return GameSettings.Instance.SleepScheduleColour;
                case ScheduleOption.Work:
                    return GameSettings.Instance.WorkScheduleColour;
                case ScheduleOption.Recreation:
                    return GameSettings.Instance.RecreationScheduleColour;
                case ScheduleOption.Anything:
                    return GameSettings.Instance.AnythingScheduleColour;
                default:
                    throw new ArgumentOutOfRangeException(nameof(scheduleOption), scheduleOption, null);
            }
        }

        public void OnPaintOptionSelected(ScheduleOption? selectedOption)
        {
            _currentSelectedOption = selectedOption;
        }

        private void AssignScheduleOption(ScheduleOption scheduleOption)
        {
            _assignedScheduleOption = scheduleOption;
            _colourDisplay.color = GetOptionColour(scheduleOption);
            
            _scheduleDisplay.AssignScheduleOption(scheduleOption, _assignedHour);
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (eventData.button == PointerEventData.InputButton.Left && Input.GetMouseButton(0))
            {
                if (_currentSelectedOption != null && _assignedScheduleOption != _currentSelectedOption)
                {
                    AssignScheduleOption((ScheduleOption)_currentSelectedOption);
                }
            }
            else
            {
                _frame.color = _highlightedFrameColour;
            }
        }

        private void OnLeftClick()
        {
            if (_currentSelectedOption != null && _assignedScheduleOption != _currentSelectedOption)
            {
                AssignScheduleOption((ScheduleOption)_currentSelectedOption);
            }
            else if(_currentSelectedOption == null)
            {
                switch (_assignedScheduleOption)
                {
                    case ScheduleOption.Anything:
                        AssignScheduleOption(ScheduleOption.Sleep);
                        break;
                    case ScheduleOption.Sleep:
                        AssignScheduleOption(ScheduleOption.Work);
                        break;
                    case ScheduleOption.Work:
                        AssignScheduleOption(ScheduleOption.Recreation);
                        break;
                    case ScheduleOption.Recreation:
                        AssignScheduleOption(ScheduleOption.Anything);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }

        private void OnRightClick()
        {
            if(_currentSelectedOption == null)
            {
                switch (_assignedScheduleOption)
                {
                    case ScheduleOption.Anything:
                        AssignScheduleOption(ScheduleOption.Recreation);
                        break;
                    case ScheduleOption.Sleep:
                        AssignScheduleOption(ScheduleOption.Anything);
                        break;
                    case ScheduleOption.Work:
                        AssignScheduleOption(ScheduleOption.Sleep);
                        break;
                    case ScheduleOption.Recreation:
                        AssignScheduleOption(ScheduleOption.Work);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            _frame.color = _defaultFrameColour;
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (eventData.button == PointerEventData.InputButton.Left && Input.GetMouseButtonDown(0))
            {
                OnLeftClick();
            } 
            else if (eventData.button == PointerEventData.InputButton.Right)
            {
                OnRightClick();    
            }
        }
    }
}
