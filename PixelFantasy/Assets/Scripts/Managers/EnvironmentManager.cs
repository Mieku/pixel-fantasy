using System;
using FunkyCode;
using UnityEngine;

namespace Managers
{
    public class EnvironmentManager : Singleton<EnvironmentManager>
    {
        [Range(0, 23.99f)] [SerializeField] private float _timeOfDay;
        [SerializeField] private AnimationCurve _lightingCurve;

        [SerializeField] private int _currentHour;
        [SerializeField] private int _currentMin;
        [SerializeField] private bool _isPM;
        [Tooltip("The amount to real minutes in a day at normal speed")][SerializeField] private float _realMinsInADay;

        private float _gameDayTimer;
        
        public GameTime GameTime => new GameTime(_currentHour, _currentMin, _isPM);

        private void Start()
        {
            _gameDayTimer = _timeOfDay;
        }

        private void LateUpdate()
        {
            CalculateTimeOfDay();
            CalculateLighting();
        }

        private void CalculateLighting()
        {
            var dayPercent = _timeOfDay / 23.99f;
            var curveHeight = _lightingCurve.Evaluate(dayPercent);
            Lighting2D.DarknessColor = new Color(Lighting2D.DarknessColor.r, Lighting2D.DarknessColor.g,
                Lighting2D.DarknessColor.b, curveHeight);
        }
        
        private void CalculateTimeOfDay()
        {
            _gameDayTimer += TimeManager.Instance.DeltaTime * (24f / 60f) / _realMinsInADay;
            if (_gameDayTimer >= 23.99f)
            {
                _gameDayTimer = 0f;
            }
            
            _timeOfDay = _gameDayTimer;
            
            var mins100 = _timeOfDay - Math.Truncate(_timeOfDay);
            _currentMin = (int)(mins100 * 60f);

            if (_timeOfDay < 1f)
            {
                _currentHour = 12;
                _isPM = false;
            }
            else if (_timeOfDay < 12f)
            {
                _currentHour = (int)_timeOfDay;
                _isPM = false;
            }
            else if(_timeOfDay < 13f)
            {
                _currentHour = (int)_timeOfDay;
                _isPM = true;
            }
            else
            {
                _currentHour = (int)(_timeOfDay - 12f);
                _isPM = true;
            }
        }
    }

    public class GameTime
    {
        public int Hour;
        public int Minute;
        public bool IsPM;

        public GameTime(int hour, int min, bool isPM)
        {
            Hour = hour;
            Minute = min;
            IsPM = isPM;
        }

        public string Readable(bool showMin = true)
        {
            string amPm = "AM";
            if (IsPM)
            {
                amPm = "PM";
            }

            if (showMin)
            {
                string displayMin = "" + Minute;
                if (Minute < 10)
                {
                    displayMin = "0" + Minute;
                }

                return $"{Hour}:{displayMin}{amPm}";
            }
            else
            {
                return $"{Hour}{amPm}";
            }
        }
    }
}
