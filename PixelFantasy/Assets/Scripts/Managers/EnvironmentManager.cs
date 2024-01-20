using System;
using FunkyCode;
using UnityEngine;
using QFSW.QC;

namespace Managers
{
    public class EnvironmentManager : Singleton<EnvironmentManager>
    {
        [Range(0, 23.99f)] [SerializeField] private float _timeOfDay;
        [SerializeField] private AnimationCurve _lightingCurve;

        [SerializeField] private int _currentDay;
        [SerializeField] private int _currentHour;
        [SerializeField] private int _currentMin;
        [SerializeField] private bool _isPM;
        [SerializeField, Range(0f, 1f)] private float _maxDarkness;
        [Tooltip("The amount to real minutes in a day at normal speed")][SerializeField] private float _realMinsInADay;
        [SerializeField] private Material _environmentMaterial;

        private float _gameDayTimer;
        private int _cur24Hour;
        
        public GameTime GameTime => new GameTime( _currentDay, _currentHour, _currentMin, _isPM);

        protected override void Awake()
        {
            base.Awake();
            _gameDayTimer = _timeOfDay;
            _cur24Hour = (int)_gameDayTimer;
            CalculateTimeOfDay();
        }

        private void LateUpdate()
        {
            CalculateTimeOfDay();
            CalculateLighting();
        }

        [Command("set_time")]
        private void CMD_SetGameTime(int hour24, int min)
        {
            hour24 = Mathf.Clamp(hour24, 0, 23);
            min = Mathf.Clamp(min, 0, 59);
            float fractionalMin = min / 60f;
            float value = hour24 + fractionalMin;
            _timeOfDay = value;
            
            _gameDayTimer = _timeOfDay;
            _cur24Hour = (int)_gameDayTimer;
            CalculateTimeOfDay();
        }

        private void CalculateLighting()
        {
            _environmentMaterial.SetFloat("_Lit", GlobalDarkness);
            
            Lighting2D.DarknessColor = new Color(Lighting2D.DarknessColor.r, Lighting2D.DarknessColor.g,
                Lighting2D.DarknessColor.b, GlobalDarkness);
        }

        private float GlobalDarkness
        {
            get
            {
                var dayPercent = _timeOfDay / 23.99f;
                var curveHeight = _lightingCurve.Evaluate(dayPercent);
                var darkness = _maxDarkness * curveHeight;
                return darkness;
            }
        }
        
        private void CalculateTimeOfDay()
        {
            _gameDayTimer += TimeManager.Instance.DeltaTime * (24f / 60f) / _realMinsInADay;
            if (_gameDayTimer >= 23.99f)
            {
                _gameDayTimer = 0f;
                _currentDay++;
            }

            var hour24 = (int)_gameDayTimer;
            if (_cur24Hour != hour24)
            {
                _cur24Hour = hour24;
                GameEvents.Trigger_HourTick(_cur24Hour);

                if (_cur24Hour == 0)
                {
                    Debug.Log("New Day");
                    GameEvents.Trigger_DayTick();
                }
            }
            
            _timeOfDay = _gameDayTimer;
            
            var mins100 = _timeOfDay - Math.Truncate(_timeOfDay);
            
            var curMin = (int)(mins100 * 60f);
            if (curMin != _currentMin)
            {
                GameEvents.Trigger_MinuteTick();
            }
            _currentMin = curMin;

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
        
        public float DoorLightInteriorAlpha => 1 - (GlobalDarkness / _maxDarkness);
    }

    public class GameTime
    {
        public int Day;
        public int Hour;
        public int Minute;
        public bool IsPM;

        public int GetCurrentHour24()
        {
            if ((Hour < 12 && !IsPM) || (Hour == 12 && IsPM))
            {
                return Hour;
            }

            if (Hour == 12 && !IsPM)
            {
                return 0;
            }

            return Hour + 12;
        }

        public GameTime(int day, int hour, int min, bool isPM)
        {
            Day = day;
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
