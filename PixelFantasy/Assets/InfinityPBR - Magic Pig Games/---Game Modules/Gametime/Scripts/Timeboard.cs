using System;
using UnityEngine;

/*
 * Timeboard combines the Gametime module, which keeps track of in-game time with many helper methods for dates and
 * time computation, with the Blackboard system. This way a blackboard exists specifically for Gametime content, allowing
 * objects which care about time to subscribe to it, without cluttering up the main Blackboard.
 *
 * Bring the Timeboard prefab into your scene to use.
 */

namespace InfinityPBR.Modules
{
    [RequireComponent(typeof(Blackboard))]
    [Serializable]
    public class Timeboard : MonoBehaviour, ISaveable
    {
        /*
         * These static reference are used to easily grab the objects. To the top of another script add:
         *
         * using static InfinityPBR.Modules.Timeboard;
         *
         * And then you can simply call:
         *
         * timeboard.Blackboard...
         * timeboard.gametime.Now() etc....
         */

        public static Timeboard timeboard;

        public Blackboard Blackboard { get; private set; }
        public Gametime gametime = new Gametime();
        public float FrameTime { get; set; }

        // Set these options true in the Inspector if you'd like events for them to be sent.
        [Header("Blackboard Events")] 
        public bool sendEventOnPauseLevelChange = true;
        public bool sendEventOnMinuteChange;
        public bool sendEventOnHourChange;
        public bool sendEventOnDayChange;
        public bool sendEventOnWeekChange;
        public bool sendEventOnMonthChange;
        public bool sendEventOnSeasonChange;
        public bool sendEventOnYearChange;
        
        // Private saves of data for events. Only saved if the corresponding bool values above are true.
        private int _pauseLevel;
        private int _minute;
        private int _hour;
        private int _day;
        private int _week;
        private string _dayOfWeek;
        private string _firstDayOfWeek;
        private int _month;
        private int _season;
        private int _year;
        
        private void Awake()
        {
            // Assign the timeboard to the static reference. There should only be one Timeboard in the scene!
            if (timeboard == null)
                timeboard = this; // Save the reference to the Blackboard
            else if (timeboard != this)
                Destroy(gameObject);
            
            Blackboard = GetComponent<Blackboard>(); // Cache this
            gametime.Awake();
            CacheValues();
        }

        private void Update()
        {
            FrameTime = Time.time; // Sets the Frametime to this -- all can then agree on the same time.
            gametime.Tick(); // Must call this or Gametime won't update!
            SendEvents();
        }

        // For each of the events we're sending, if the value has changed, send the event. Subscribe to the
        // Timeboard Blackboard to receive these events. Values are only tracked in this context if the events are
        // being sent. To get the value directly, call Timeboard.Gametime.[value]
        private void SendEvents()
        {
            if (sendEventOnPauseLevelChange && gametime.pauseLevel != _pauseLevel)
                Blackboard.AddEvent("Gametime", "Pause Level"
                    , gametime.pauseLevel.ToString()
                    , _pauseLevel = gametime.pauseLevel);
            
            if (sendEventOnMinuteChange && gametime.Minute() != _minute)
                Blackboard.AddEvent("Gametime", "Minute"
                    , ParseTwoDigitNumber(gametime.Minute())
                    , _minute = gametime.Minute());
            
            if (sendEventOnHourChange && gametime.Hour() != _hour)
                Blackboard.AddEvent("Gametime", "Hour"
                    , ParseTwoDigitNumber(gametime.Hour())
                    , _hour = gametime.Hour());
            
            if (sendEventOnDayChange && gametime.Day() != _day)
                Blackboard.AddEvent("Gametime", "Day"
                    , gametime.DayName()
                    , _day = gametime.Day());
            
            if (sendEventOnWeekChange && gametime.Week() != _week)
            {
                if (gametime.DayName() == _firstDayOfWeek) // If the new day name is the first, send the event
                    Blackboard.AddEvent("Gametime", "Week"
                        , gametime.Week(-1, true).ToString()
                        , _week = gametime.Week());
            }
            
            if (sendEventOnMonthChange && gametime.Month() != _month)
                Blackboard.AddEvent("Gametime", "Month"
                    , gametime.MonthName()
                    , _month = gametime.Month());
            
            // The "status" value will be the name of the season. The "obj" is the int index of the season.
            if (sendEventOnSeasonChange && gametime.Season() != _season)
                Blackboard.AddEvent("Gametime", "Season"
                    , gametime.SeasonName()
                    , _season = gametime.Season());
            
            if (sendEventOnYearChange && gametime.Year() != _year)
                Blackboard.AddEvent("Gametime", "Year"
                    , gametime.Year().ToString()
                    , _year = gametime.Year());
        }

        // Will return two-digit values for single-digit results, i.e. "08" instead of "8".
        public string ParseTwoDigitNumber(int value) => $"{(value < 10 ? "0" : "")}{value}";

        // We cache the values on awake, so that they do not send Events simply because they're being saved for
        // the first time.
        private void CacheValues()
        {
            _minute = gametime.Minute();
            _hour = gametime.Hour();
            _day = gametime.Day();
            _week = gametime.Week();
            _dayOfWeek = gametime.DayName();
            _firstDayOfWeek = gametime.dayNames[0];
            _month = gametime.Month();
            _season = gametime.Season();
            _year = gametime.Year();
        }
        
        // Must be called or it will not run!
        private void OnValidate() => gametime.OnValidate();
        public object SaveState()
        {
            var saveData = new TimeboardSaveData
            {
                gametime = gametime
            };

            return saveData;
        }

        public void LoadState(string jsonEncodedState)
        {
            var data = JsonUtility.FromJson<TimeboardSaveData>(jsonEncodedState);

            gametime = data.gametime;
        }

        public string SaveableObjectId() => "Timeboard";

        public void PreSaveActions()
        {
            throw new NotImplementedException();
        }

        public void PostSaveActions()
        {
            throw new NotImplementedException();
        }

        public void PreLoadActions()
        {
            throw new NotImplementedException();
        }

        public void PostLoadActions()
        {
            throw new NotImplementedException();
        }
        
        [Serializable]
        private struct TimeboardSaveData
        {
            public Gametime gametime;
        }
    }
}
