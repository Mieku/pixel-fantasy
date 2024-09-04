using System;
using Managers;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace HUD
{
    public class ClockHUD : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _timeText;
        [SerializeField] private TextMeshProUGUI _dateText;
        [SerializeField] private Image _sunDialArrow;
        [SerializeField] private Sprite _arrowDown, _arrowUp, _arrowRight, _arrowLeft;
        [SerializeField] private Sprite _arrowBL, _arrowBR, _arrowTL, _arrowTR;

        private void Awake()
        {
            GameEvents.DayTick += OnNewDay;
            GameEvents.HourTick += OnNewHour;
            GameEvents.MinuteTick += OnNewMinute;
            GameEvents.OnGameLoadComplete += OnGameLoaded;
        }

        private void OnDestroy()
        {
            GameEvents.DayTick -= OnNewDay;
            GameEvents.HourTick -= OnNewHour;
            GameEvents.MinuteTick -= OnNewMinute;
            GameEvents.OnGameLoadComplete -= OnGameLoaded;
        }

        private void OnGameLoaded()
        {
            OnNewDay();
        }

        private void OnNewDay()
        {
            // # Month
            string date = EnvironmentManager.Instance.GetReadableDate();
            _dateText.text = date;
        }

        private void OnNewHour(int hour)
        {
            if (hour >= 23 || hour == 0 || hour == 1)
            {
                // Down
                _sunDialArrow.sprite = _arrowDown;
            }
            else if (hour >= 2 && hour <= 4)
            {
                // BL
                _sunDialArrow.sprite = _arrowBL;
            }
            else if (hour >= 5 && hour <= 7)
            {
                // Left
                _sunDialArrow.sprite = _arrowLeft;
            }
            else if (hour >= 8 && hour <= 10)
            {
                // TL
                _sunDialArrow.sprite = _arrowTL;
            }
            else if (hour >= 11 && hour <= 13)
            {
                // Up
                _sunDialArrow.sprite = _arrowUp;
            }
            else if (hour >= 14 && hour <= 16)
            {
                // TR
                _sunDialArrow.sprite = _arrowTR;
            }
            else if (hour >= 17 && hour <= 19)
            {
                // Right
                _sunDialArrow.sprite = _arrowRight;
            }
            else if (hour >= 20 && hour <= 22)
            {
                // BR
                _sunDialArrow.sprite = _arrowBR;
            }
        }


        private void OnNewMinute()
        {
            var time = EnvironmentManager.Instance.GameTime.Readable();
            _timeText.text = time;
        }
    }
}
