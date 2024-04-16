using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;

/*
 * INFINITY PBR - https://infinitypbr.com
 * Join the Discord for support & discussion with the community: https://discord.com/invite/cmZY2tH
 * Scripting documentation: https://infinitypbr.gitbook.io/infinity-pbr/
 * Youtube videos, tutorials, demos, and integrations: https://www.youtube.com/channel/UCzATh2-NC_xZSGnhZF-cFaw
 * All assets on the Asset Store: https://assetstore.unity.com/publishers/4645?aid=1100lxWw&pubref=p88
 */

/*
 * LEGEND OF THE STONES
 * 0 = unpaused
 * 1 = turn based combat
 * 2 = spell or potion on deck -- like turn based, but no time moves and camera can't move
 * 3 = Menu pause
 * 4 = Full pause / Save/Load menu is up
 */

/*
 * This is a simple module intended to help keep track of time in the game, plus pausing the game.
 *
 * Reference this class from your main game data. It is serializable, so should be saved with the rest of your
 * game data!
 *
 * If you are integrating with a weather or day/night system, this should integrated fairly easily, though depending on
 * the other system you are using, you may need to write some code to translate the data into and out of the other
 * system.
 *
 * IMPORTANT: You must call Tick() on this script during your Update() loop to advance time! Pausing the
 * increase of time is handled by pauseLevel here, however.
 *
 * Similarly, OnValidate will only run if called from a MonoBehaviour. Add OnValidate() to that script, and then call
 * this methods OnValidate() from there.
 */

namespace InfinityPBR.Modules
{
    [Serializable]
    public class Gametime
    {
        public enum DatePart
        {
            Second,
            Minute,
            Hour,
            Day,
            Week,
            Month,
            Season,
            Year
        }

        // NOTE: This module uses "pauseLevel" to provide various levels of game pause. 0 means unpaused, and the numbers
        // increase from there. In some games, 1 = paused, and that's it. In other games 1 = some things are paused, while
        // 2 = full pause of the game.  This way, you can allow some activity to continue, like animations and audio,
        // when partially paused while keeping other activity paused.

        [Header("Options (-1 = no effect)")] 
        // -1 means no effect
        public int pauseLevelOnAwake = -1; // If true, when this script is awakened, game will pause
        public int pauseLevelWhenSceneChanges = -1; // If true, game will pause when we change scenes.
        public int pauseLevelWhenNewSceneLoaded = -1; // If true, game will unpause when a new scene is loaded.
        public int stopTimeOnPauseLevel = 1; // Time will not advance if pauseLevel >= this value.

        [Header("Time Options")]
        public bool useAmPm = true;
        public bool monthDayYear = true; // If false, will do Day/Month/Year
        [FormerlySerializedAs("secondsPerGameMinute")] [Range(0.5f, 100)] public float realWorldSecondsPerGameMinute = 3.0f; // Real-world seconds per in-game minute
        [Range(1, 240)] public int secondsPerMinute = 60; // In-game seconds per in-game minute
        [Range(1, 240)] public int minutesPerHour = 60; // In-game minutes per in-game hour
        [Range(1, 240)] public int hoursPerDay = 24; // In-game hours per in-game day
        [Range(1, 240)] public int daysPerMonth = 30; // In-game days per in-game month
        
        [Header("Starting Time")]
        public int startingYear = 1288;
        public int startingMinute = 30;
        public int startingHour = 9;
        public int startingDay = 15;
        public int startingMonth = 5;

        [Header("Calendar Names")]
        public List<string> dayNames = new List<string>();
        public List<string> monthNames = new List<string>();
        public List<string> seasonNames = new List<string>();

        [Header("Seasonality")] 
        public int firstSeasonDaysFromNewYear; // 0 = first season starts on New Year. This value should be in the first portion of the year.
       
        // Can add [HideInInspector] to these if you'd like.
        [Header("Managed by class")] 
        public int pauseLevel;
        //public double timeCounter = 0.0; // This is the main source of truth for the date/time.
        [SerializeField] private float _subCounter; // Can be used during runtime.
        [SerializeField] private int _dayOfWeek;
        public int Seconds(int gameTime = -1) => GetSeconds(gameTime);
        public int Minute(int gameTime = -1) => GetMinute(gameTime);
        public int Hour(int gameTime = -1) => GetHour(gameTime);
        public int Day(int gameTime = -1, bool shiftForNumericalDisplay = false) => GetDay(gameTime, shiftForNumericalDisplay);
        public int Week(int gameTime = -1, bool shiftForNumericalDisplay = false) => GetWeek(gameTime, shiftForNumericalDisplay);
        public int Month(int gameTime = -1, bool shiftForNumericalDisplay = false) => GetMonth(gameTime, shiftForNumericalDisplay);
        public int Season(int gameTime = -1) => GetSeason(gameTime);
        public int Year(int gameTime = -1) => GetYear(gameTime);
        
        public string DayName(int gameTime = -1) => DayOfWeekName(gameTime);
        public string MonthName(int gameTime = -1) => monthNames[GetMonth(gameTime)];
        public string SeasonName(int gameTime = -1) => seasonNames[Season(gameTime)];

        public int GameTime => _gameTime; // Returns an int
        
        public int DaysPerWeek => dayNames.Count;
        public int NumberOfMonths => monthNames.Count;
        public int NumberOfSeasons => seasonNames.Count;
        public int DaysPerMonth => daysPerMonth; // really just the same, but folks may think of it this way.
        public int SecondsPerMinute => secondsPerMinute;
        public int MinutesPerHour => minutesPerHour;
        public int HoursPerDay => hoursPerDay;
        public int DaysPerYear => daysPerMonth * NumberOfMonths;
        public int DaysPerSeason => Mathf.FloorToInt(DaysPerYear / NumberOfSeasons); // Note: There could be different number of days per season if the year is not divisible!
        public int WeeksPerYear => Mathf.CeilToInt(DaysPerYear / DaysPerWeek); // Note: There could be a non-even number of weeks per year!
        public float DaysPerSeasonFloat => DaysPerYear / NumberOfSeasons;
        public float WeeksPerYearFloat => DaysPerYear / DaysPerWeek;
        
        //"Seconds Per" Properties use the secondsPerGameMinute float, which corresponds to real-world time.
        public float RealWorldSecondsPerGameMinute => realWorldSecondsPerGameMinute;
        public float SecondsPerGameHour => realWorldSecondsPerGameMinute * minutesPerHour;
        public float SecondsPerGameDay => SecondsPerGameHour * hoursPerDay;
        public float SecondsPerGameWeek => SecondsPerGameDay * dayNames.Count;
        public float SecondsPerGameMonth => SecondsPerGameDay * daysPerMonth;
        public float SecondsPerGameYear => SecondsPerGameMonth * monthNames.Count;
        public float SecondsPerGameSeason => SecondsPerGameYear / seasonNames.Count;
        
        // "GameTime Per" Properties do not include secondsPerGameMinute, and "1" = "1 in game minute".
        public int GameTimePerHour => minutesPerHour; // minutesPerHour is the same as GameTimePerHour!
        public int GameTimePerDay => GameTimePerHour * hoursPerDay;
        public int GameTimePerWeek => GameTimePerDay * DaysPerWeek;
        public int GameTimePerMonth => daysPerMonth * GameTimePerDay;
        public int GameTimePerYear => DaysPerYear * GameTimePerDay;
        public int GameTimePerSeason => DaysPerSeason * GameTimePerDay;

        
        // PRIVATES
        [SerializeField] private int _gameTime;
        [SerializeField] private int lastPauseLevel;
        [SerializeField] private bool _isSetup;
        public bool IsSetup => _isSetup;
        
        
        
        /// <summary>
        /// Returns the float value of the current in-game time, including subtime by default.
        /// </summary>
        /// <param name="includeSubtime"></param>
        /// <returns></returns>
        public float Now(bool includeSubtime = true) => _gameTime + (includeSubtime ? _subCounter / realWorldSecondsPerGameMinute : 0);
        
        
        
        
        
        
        
        
        
        
        
        
        

        
        
        
        /// <summary>
        /// Returns the gametime based on Now() plus the additional real-world timeToAdd (in seconds)
        /// </summary>
        /// <param name="realWorldMinutes"></param>
        /// <returns></returns>
        public float LaterRealTime(float realWorldMinutes)
        {
            var minutes = Mathf.Floor(realWorldMinutes);
            var seconds = (realWorldMinutes - minutes) / realWorldSecondsPerGameMinute;
            return Now() + minutes + seconds;
        }

        /// <summary>
        /// Returns the gametime based on Now() minus the additional real-world timeToSubtract (in seconds)
        /// </summary>
        /// <param name="realWorldMinutes"></param>
        /// <returns></returns>
        public float EarlierRealTime(float realWorldMinutes)
        {
            var minutes = Mathf.Floor(realWorldMinutes);
            var seconds = (realWorldMinutes - minutes) / realWorldSecondsPerGameMinute;
            return Now() - minutes - seconds;
        }
        
        /// <summary>
        /// Returns the gametime based on Now() plus the values in the provided TimeSpan, which are all
        /// in-game time values.
        /// </summary>
        /// <param name="span"></param>
        /// <returns></returns>
        public float Later(TimeSpan span)
        {
            var totalSeconds = 0f;

            totalSeconds += span.Seconds;
            totalSeconds += span.Minutes * RealWorldSecondsPerGameMinute;
            totalSeconds += span.Hours * SecondsPerGameHour;
            totalSeconds += span.Days * SecondsPerGameDay;
            totalSeconds += span.Months * SecondsPerGameMonth;
            totalSeconds += span.Years * SecondsPerGameYear;

            // Convert game seconds back into game time
            var gameTimeToAdd = totalSeconds / RealWorldSecondsPerGameMinute;

            return Now() + gameTimeToAdd;
        }

        /// <summary>
        /// Returns the gametime based on Now() minus the values in the provided TimeSpan, which are all
        /// in-game time values.
        /// </summary>
        /// <param name="span"></param>
        /// <returns></returns>
        public float Earlier(TimeSpan span)
        {
            var totalSeconds = 0f;

            totalSeconds += span.Seconds;
            totalSeconds += span.Minutes * RealWorldSecondsPerGameMinute;
            totalSeconds += span.Hours * SecondsPerGameHour;
            totalSeconds += span.Days * SecondsPerGameDay;
            totalSeconds += span.Months * SecondsPerGameMonth;
            totalSeconds += span.Years * SecondsPerGameYear;

            // Convert game seconds back into game time
            var gameTimeToAdd = totalSeconds / RealWorldSecondsPerGameMinute;

            return Now() - gameTimeToAdd;
        }
        
        /// <summary>
        /// Returns the gametime based on Now() plus the values in the provided in-game time values.
        /// </summary>
        /// <param name="seconds"></param>
        /// <param name="minutes"></param>
        /// <param name="hours"></param>
        /// <param name="days"></param>
        /// <param name="weeks"></param>
        /// <param name="months"></param>
        /// <param name="seasons"></param>
        /// <param name="years"></param>
        /// <returns></returns>
        public float Later(float seconds = 0f, float minutes = 0f, float hours = 0f, float days = 0f, float weeks = 0f, float months = 0f, float seasons = 0f, float years = 0f)
        {
            // Convert weeks and seasons to days
            days += weeks * DaysPerWeek;
            days += seasons * DaysPerSeasonFloat;
    
            var span = new TimeSpan
            {
                Seconds = seconds,
                Minutes = minutes,
                Hours = hours,
                Days = days,
                Months = months,
                Years = years
            };

            return Later(span);
        }

        /// <summary>
        /// Returns the gametime based on Now() minus the values in the provided in-game time values.
        /// </summary>
        /// <param name="seconds"></param>
        /// <param name="minutes"></param>
        /// <param name="hours"></param>
        /// <param name="days"></param>
        /// <param name="weeks"></param>
        /// <param name="months"></param>
        /// <param name="seasons"></param>
        /// <param name="years"></param>
        /// <returns></returns>
        public float Earlier(float seconds = 0f, float minutes = 0f, float hours = 0f, float days = 0f, float weeks = 0f, float months = 0f, float seasons = 0f, float years = 0f)
        {
            // Convert weeks and seasons to days
            days += weeks * DaysPerWeek;
            days += seasons * DaysPerSeasonFloat;
    
            var span = new TimeSpan
            {
                Seconds = seconds,
                Minutes = minutes,
                Hours = hours,
                Days = days,
                Months = months,
                Years = years
            };

            return Earlier(span);
        }
        
        public float WeekStartGameTime(float gameTime = -1)
        {
            int currentGameTime = gameTime < 0 ? Mathf.FloorToInt(Now(false)) : Mathf.FloorToInt(gameTime);
            int daysIntoWeek = currentGameTime % GameTimePerWeek;
            return currentGameTime - daysIntoWeek;
        }

        public float MonthStartGameTime(float gameTime = -1)
        {
            int currentGameTime = gameTime < 0 ? Mathf.FloorToInt(Now(false)) : Mathf.FloorToInt(gameTime);
            int daysIntoMonth = currentGameTime % GameTimePerMonth;
            return currentGameTime - daysIntoMonth;
        }

        public float NextWeekGameTime(float gameTime = -1)
        {
            int currentGameTime = gameTime < 0 ? Mathf.FloorToInt(Now(false)) : Mathf.FloorToInt(gameTime);
            int daysUntilNextWeek = GameTimePerWeek - (currentGameTime % GameTimePerWeek);
            return currentGameTime + daysUntilNextWeek;
        }

        public float NextMonthGameTime(float gameTime = -1)
        {
            int currentGameTime = gameTime < 0 ? Mathf.FloorToInt(Now(false)) : Mathf.FloorToInt(gameTime);
            int daysUntilNextMonth = GameTimePerMonth - (currentGameTime % GameTimePerMonth);
            return currentGameTime + daysUntilNextMonth;
        }


        /// <summary>
        /// Returns the float gameTime of the start of the season in the provided year. If year is -1, the current
        /// year is used.
        /// </summary>
        /// <param name="season"></param>
        /// <param name="year"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public float SeasonGameTime(int season, int year = -1)
        {
            // Check seasonIndex is valid
            if (season < 0 || season >= seasonNames.Count)
                throw new ArgumentOutOfRangeException($"Invalid season index. {season} was provided and there are {seasonNames.Count} seasons.");

            return GameTimeNewYear(year < 0 ? Year() : year) // Start with gametime of the year
                  + (firstSeasonDaysFromNewYear // Add in the days until start of first season
                     + (season * DaysPerSeason))  // Add in the days per season * the index. If index = 0, will be 0
                  * GameTimePerDay; // Multiply by gametime per day.
        }

        /// <summary>
        /// Add time to GameTime. Note minutes = timeToAdd / secondsPerGameMinute
        /// </summary>
        /// <param name="realWorldSeconds">This is similar to real-world time / deltaTime</param>
        public void AddRealWorldSeconds(float realWorldSeconds)
        {
            _subCounter += realWorldSeconds;

            // If the subCounter is over the limit, subtract the secondPerGameMinute and add one more minute
            if (_subCounter < realWorldSecondsPerGameMinute) return;
            var remainder = _subCounter % realWorldSecondsPerGameMinute;
            AddMinutes((_subCounter - remainder) / realWorldSecondsPerGameMinute);
            _subCounter = remainder;
        }
        
        public void AddRealWorldMinutes(int realWorldMinutes) => AddRealWorldSeconds(realWorldMinutes * 60);
        public void AddRealWorldHours(int realWorldHours) => AddRealWorldSeconds(realWorldHours * 60 * 60);

        
        



        public TimeSpan TimeSpanBetween(float startGameTime, float endGameTime) 
        {
            float elapsedTime = endGameTime - startGameTime;
            return GameTimeToTimeSpan(elapsedTime);
        }

        public TimeSpan TimeSpanSince(float otherGameTime, float gameTime = -1) 
        {
            gameTime = gameTime < 0 ? Now(false) : gameTime;
            return TimeSpanBetween(otherGameTime, gameTime);
        }

        public TimeSpan TimeSpanUntil(float otherGameTime, float gameTime = -1) 
        {
            gameTime = gameTime < 0 ? Now(false) : gameTime;
            return TimeSpanBetween(gameTime, otherGameTime);
        }

        // ADD METHODS
        public void AddMinutes(float minutesToAdd) => AddGameTime(minutesToAdd);
        public void AddHours(float hoursToAdd) => AddGameTime(hoursToAdd * MinutesPerHour);
        public void AddDays(float daysToAdd) => AddGameTime(daysToAdd * GameTimePerDay);
        public void AddWeeks(float weeksToAdd) => AddGameTime(weeksToAdd * GameTimePerWeek);
        public void AddMonths(float monthsToAdd) => AddGameTime(monthsToAdd * GameTimePerMonth);
        public void AddYears(float yearsToAdd) => AddGameTime(yearsToAdd * GameTimePerYear);
        public void AddSeasons(float seasonsToAdd) => AddGameTime(seasonsToAdd * GameTimePerSeason);


        public void AddGameTime(float gameTimeToAdd)
        {
            var wholeNumber = Mathf.FloorToInt(gameTimeToAdd);
            var remainder = gameTimeToAdd - wholeNumber;

            _gameTime += wholeNumber;
            _subCounter += remainder * realWorldSecondsPerGameMinute;
        }


        /// <summary>
        /// This is the main method for displaying the date based on the GameTime. There are multiple options on how to display the information.
        /// </summary>
        /// <param name="gameTime">If not provided, will use the current GameTime value.</param>
        /// <param name="numericalDisplay"></param>
        /// <param name="includeTime"></param>
        /// <param name="includeDayOfWeek"></param>
        /// <param name="dateSeparator"></param>
        /// <returns></returns>
        public string FullDate(float gameTime = -1, bool numericalDisplay = true, bool includeTime = true, bool includeDayOfWeek = true, string dateSeparator = "/")
        {
            // Cache values
            var gameMinute = GetMinute(gameTime);
            var gameHour = GetHour(gameTime);
            var gameDay = GetDay(gameTime, true);
            var gameMonth = GetMonth(gameTime, numericalDisplay);
            var gameYear = GetYear(gameTime);
            
            // Set other values
            var leadingZero = gameMinute < 10 ? "0" : "";
            var ampm = useAmPm ? gameHour > (hoursPerDay / 2 - 1) ? "pm" : "am" : null;
            var displayHour = gameHour;
            if (useAmPm && gameHour == 0) displayHour = hoursPerDay / 2;
            if (useAmPm && gameHour > hoursPerDay / 2) displayHour = gameHour - (hoursPerDay / 2);

            // Day of week
            var value = includeDayOfWeek ? $"{DayOfWeekName(gameTime)}" : "";
            
            // Date
            if (numericalDisplay && monthDayYear)
                value = $"{value} {gameMonth}{dateSeparator}{gameDay}{dateSeparator}{gameYear}";

            if (numericalDisplay && !monthDayYear)
                value = $"{value} {gameDay}{dateSeparator}{gameMonth}{dateSeparator}{gameYear}";
            
            if (!numericalDisplay && monthDayYear)
                value = $"{value} {monthNames[gameMonth]} {gameDay}, {gameYear}";
            
            if (!numericalDisplay && !monthDayYear)
                value = $"{value} {gameDay} {monthNames[gameMonth]}, {gameYear}";
            
            // Time
            if (includeTime)
                value = $"{value} {displayHour}:{leadingZero}{gameMinute}{ampm}";

            return value;
        }





        /*
        * ============================================================================
        * REQUIRED! Call this from your Update() loop
        * ============================================================================
        */
        
        public void Tick(float timeMod = 1f)
        {
            CheckSetup();
            
            if (pauseLevel >= stopTimeOnPauseLevel) return;
            
            AddRealWorldSeconds(Time.deltaTime * timeMod);
        }

        /*
         * ============================================================================
         * ON VALIDATION METHODS -- Call "OnValidate" from your other method!
         * ============================================================================
         */

        public void OnValidate()
        {
            // Ensure we have good default values
            CheckForZeroes();
            CheckHoursPerDay();
            
            // Ensure first season is in the first part of the year (based on number of seasons)
            firstSeasonDaysFromNewYear = Mathf.Clamp(firstSeasonDaysFromNewYear, 0, DaysPerSeason - 1);
            
            /*
            // Ensure the solstice starts at the right portion of the year
            var halfYearDays = Mathf.FloorToInt(GameTimePerYear / (2 * GameTimePerDay));
            solsticeDaysFromNewYear = Modulo(solsticeDaysFromNewYear, halfYearDays * 2);
            if (solsticeDaysFromNewYear > halfYearDays)
            {
                solsticeDaysFromNewYear -= halfYearDays * 2;
            }
            
            // Ensure sunrise is in first half of the day and sunset in the second half
            summerSolsticeSunrise = Mathf.Clamp(summerSolsticeSunrise, 0, 0.4999f);
            summerSolsticeSunset = Mathf.Clamp(summerSolsticeSunset, 0.5f, 1);
            
            // Convert fractions of the day to game time
            _summerSolsticeSunriseGameTime = (int)(summerSolsticeSunrise * GameTimePerDay);
            _summerSolsticeSunsetGameTime = (int)(summerSolsticeSunset * GameTimePerDay);
            _winterSolsticeSunriseGameTime = (int)(winterSolsticeSunrise * GameTimePerDay);
            _winterSolsticeSunsetGameTime = (int)(winterSolsticeSunset * GameTimePerDay);

            // Convert fractions of the day to hours and minutes
            _summerSolsticeSunriseHour = (int)(summerSolsticeSunrise * HoursPerDay);
            _summerSolsticeSunsetHour = (int)(summerSolsticeSunset * HoursPerDay);
            _winterSolsticeSunriseHour = (int)(winterSolsticeSunrise * HoursPerDay);
            _winterSolsticeSunsetHour = (int)(winterSolsticeSunset * HoursPerDay);

            _summerSolsticeSunriseMinute = (int)((summerSolsticeSunrise * HoursPerDay - _summerSolsticeSunriseHour) * MinutesPerHour);
            _summerSolsticeSunsetMinute = (int)((summerSolsticeSunset * HoursPerDay - _summerSolsticeSunsetHour) * MinutesPerHour);
            _winterSolsticeSunriseMinute = (int)((winterSolsticeSunrise * HoursPerDay - _winterSolsticeSunriseHour) * MinutesPerHour);
            _winterSolsticeSunsetMinute = (int)((winterSolsticeSunset * HoursPerDay - _winterSolsticeSunsetHour) * MinutesPerHour);
            
            // Update day length curve
            UpdateDayLengthCurve();
            */
            
            /*
            // Ensure dayLengthCurve ends where it starts
            if (dayLengthCurve.keys.Length > 0)
            {
                var firstKey = dayLengthCurve.keys[0];
                var lastKey = dayLengthCurve.keys[dayLengthCurve.keys.Length - 1];
                if (firstKey.value != lastKey.value)
                {
                    lastKey.value = firstKey.value;
                    dayLengthCurve.MoveKey(dayLengthCurve.keys.Length - 1, lastKey);
                }
            }
            
            dayLengthCurve.preWrapMode = WrapMode.Loop;
            dayLengthCurve.postWrapMode = WrapMode.Loop;
            */

            // Ensure we are starting on month / day 1 at a minimum
            if (startingMonth <= 0) startingMonth = 1;
            if (startingDay <= 0) startingDay = 1;
            
            // The initial value of _gameTime is based on the starting month, day, hour, and minute, with New Year
            // on the startingYear = 0.
            _gameTime = 0;

            // Add game time for the starting month, day, hour, and minute
            _gameTime += (startingMonth - 1) * GameTimePerMonth;
            _gameTime += (startingDay - 1) * GameTimePerDay;
            _gameTime += startingHour * GameTimePerHour;
            _gameTime += startingMinute;
        }
        
        /*
        void UpdateDayLengthCurve()
        {
            // Create the day length curve
            float summerSolsticeDayLength = summerSolsticeSunset - summerSolsticeSunrise;
            float winterSolsticeDayLength = winterSolsticeSunset - winterSolsticeSunrise;
            dayLengthCurve = new AnimationCurve(new Keyframe(0f, summerSolsticeDayLength), new Keyframe(1f, winterSolsticeDayLength));

            // Set the tangents to ensure a smooth transition
            dayLengthCurve.keys[0].outTangent = 0;
            dayLengthCurve.keys[1].inTangent = 0;
        }
        */


        public int Modulo(int a, int b)
        {
            int r = a % b;
            return r < 0 ? r + b : r;
        }
        
        public float Modulo(float a, float b)
        {
            float m = a % b;
            return m < 0 ? m + b : m;
        }

        private void CheckSetup()
        {
            if (_isSetup) return;

            if (startingDay >= daysPerMonth) startingDay = 0;
            if (startingMinute >= minutesPerHour) startingMinute = 0;
            if (startingHour >= hoursPerDay) startingHour = 0;
            if (startingMonth >= monthNames.Count) startingMonth = 0;

            var startingGametime = startingMinute;
            startingGametime += startingHour * minutesPerHour;
            startingGametime += startingDay * (hoursPerDay * minutesPerHour);
            startingGametime += startingMonth * (daysPerMonth * hoursPerDay * minutesPerHour);
            _gameTime = startingGametime;
            
            _isSetup = true;
        }

        // This will ensure that none of the required string lists are empty
        private void CheckForZeroes()
        {
            if (realWorldSecondsPerGameMinute < 1) realWorldSecondsPerGameMinute = 3;

            if (dayNames.Count == 0)
            {
                dayNames.Add("Sunday");
                dayNames.Add("Monday");
                dayNames.Add("Tuesday");
                dayNames.Add("Wednesday");
                dayNames.Add("Thursday");
                dayNames.Add("Friday");
                dayNames.Add("Saturday");
            }
            if (monthNames.Count == 0)
            {
                monthNames.Add("January");
                monthNames.Add("February");
                monthNames.Add("March");
                monthNames.Add("April");
                monthNames.Add("May");
                monthNames.Add("June");
                monthNames.Add("July");
                monthNames.Add("August");
                monthNames.Add("September");
                monthNames.Add("October");
                monthNames.Add("November");
                monthNames.Add("December");
            }
            if (seasonNames.Count == 0)
            {
                seasonNames.Add("Winter");
                seasonNames.Add("Spring");
                seasonNames.Add("Summer");
                seasonNames.Add("Fall");
            }
        }
        
        private void CheckHoursPerDay()
        {
            if (!useAmPm) return;
            if (hoursPerDay % 2 == 0 && hoursPerDay > 0) return;
            
            hoursPerDay += 1;
            Debug.Log("<color=yellow>When useAmPm = true, hoursPerDay must be even.");
        }
        
        /*
         * ============================================================================
         * STARTING METHODS -- Call "Awake" from your other method!
         * ============================================================================
         */
        
        // NOTE -- this should be called from the parent monobehavior that this is attached to! Awake is not called
        // directly, instead you need to call it manually.
        public void Awake()
        {
            SceneManager.sceneLoaded += SceneLoaded;
            SceneManager.activeSceneChanged += ChangedActiveScene;
            pauseLevel = pauseLevelOnAwake >= 0 ? pauseLevelOnAwake : pauseLevel;
        }
        
        private void SceneLoaded(Scene scene, LoadSceneMode mode) => pauseLevel = pauseLevelWhenNewSceneLoaded >= 0 ? pauseLevelWhenNewSceneLoaded : pauseLevel;
        private void ChangedActiveScene(Scene current, Scene next) => pauseLevel = pauseLevelWhenSceneChanges >= 0 ? pauseLevelWhenSceneChanges : pauseLevel;
        
        /*
         * ============================================================================
         * METHODS
         * ============================================================================
         */
        
        private int GetMinute(float gameTime = -1)
        {
            gameTime = gameTime < 0 ? Now() : gameTime;
            var value = Mathf.FloorToInt(gameTime % GameTimePerHour);
            if (value < 0) value = GameTimePerHour + value;
            return value;
        }

        private int GetHour(float gameTime = -1)
        {
            gameTime = gameTime < 0 ? Now() : gameTime;
            var value = Mathf.FloorToInt(gameTime % GameTimePerDay / GameTimePerHour);
            if (value < 0) value = hoursPerDay + value;
            return value;
        }

        private int GetDay(float gameTime = -1, bool shiftForNumericalDisplay = false)
        {
            gameTime = gameTime < 0 ? Now() : gameTime;
            var value = Mathf.FloorToInt(gameTime % GameTimePerMonth / GameTimePerDay);
            if (value < 0) value = daysPerMonth + value;
            return value + (shiftForNumericalDisplay ? 1 : 0);
        }
        
        private int GetWeek(float gameTime = -1, bool shiftForNumericalDisplay = false)
        {
            gameTime = gameTime < 0 ? Now() : gameTime;
            var value = Mathf.FloorToInt(gameTime % GameTimePerYear / GameTimePerWeek);
            if (value < 0) value = WeeksPerYear + value; // weeksPerYear should be a constant holding the number of weeks in a year
            return value + (shiftForNumericalDisplay ? 1 : 0);
        }

        private int GetSeconds(float gameTime = -1)
        {
            gameTime = gameTime < 0 ? Now() : gameTime;
            // Subtract whole minutes from the current gameTime to get the remaining seconds
            var remainingMinutes = gameTime - Mathf.Floor(gameTime);
            // Convert remaining minutes to seconds
            var seconds = remainingMinutes * SecondsPerMinute;
            return Mathf.FloorToInt(seconds);
        }


        private int GetMonth(float gameTime = -1, bool shiftForNumericalDisplay = false)
        {
            if (Math.Abs(gameTime - (-1)) < 0.01)
                gameTime = Now(false);
            var value = Mathf.FloorToInt(gameTime % GameTimePerYear / GameTimePerMonth);
            if (value < 0) value = monthNames.Count + value;
            return value + (shiftForNumericalDisplay ? 1 : 0);
        }

        private int GetYear(float gameTime = -1, bool addStartingYear = true)
        {
            if (Math.Abs(gameTime - (-1)) < 0.01)
                gameTime = Now(false);
            var value = Mathf.FloorToInt(gameTime / GameTimePerYear) + (addStartingYear ? startingYear : 0);
            return value;
        }
        
        private string DayOfWeekName(float gameTime = -1)
        {
            if (gameTime == -1)
                gameTime = GameTime;
            
            var totalDays = Mathf.Floor(gameTime / GameTimePerDay);
            var dayShift = totalDays > 0 ? totalDays % dayNames.Count : 0;
            var newDayIndex = 0 + dayShift;
            if (newDayIndex < 0) newDayIndex += dayNames.Count;
            if (newDayIndex >= dayNames.Count)
            {
                Debug.LogWarning($"newDayIndex is {newDayIndex} but there are only {dayNames.Count} day names.");
                return default;
            }
            return dayNames[(int) newDayIndex];
        }
        
        private string GetMonthName(int gameTime = -1)
        {
            if (gameTime == -1)
                gameTime = GameTime;

            return monthNames[GetMonth(gameTime)];
        }
        
        private string GetSeasonName(int gameTime = -1)
        {
            if (gameTime == -1)
                gameTime = GameTime;
            
            var totalDays = Mathf.Floor((float)gameTime / GameTimePerDay);
            var dayShift = totalDays % dayNames.Count;
            var newDayIndex = 0 + dayShift;
            if (newDayIndex < 0) newDayIndex += dayNames.Count;
            return dayNames[(int) newDayIndex];
        }
        
        /// <summary>
        /// This will return the gameTime value for New Year of the provided year. If no year is provided, it will
        /// use the current Year.
        /// </summary>
        /// <param name="year"></param>
        /// <returns></returns>
        public float GameTimeNewYear(int year = -1)
        {
            if (year == -1)
                year = Year();

            int yearsSinceStart = year - startingYear;
            float gameTimeAtStartOfYear = yearsSinceStart * GameTimePerYear;

            return gameTimeAtStartOfYear;
        }

        public float LastNewYear => GameTimeNewYear(); // Current years new year
        public float NextNewYear => GameTimeNewYear(Year() + 1); // Next new year
        public float GameTimeUntilNextNewYear => NextNewYear - GameTime; // Gametime until the next new year
        public float GameTimeSinceNewYear => GameTime - LastNewYear; // Gametime since the last new year

        /// <summary>
        /// Provided a endGameTime, we will return a string displaying how much time is left before timeCounter
        /// reaches that value -- human readable.
        /// </summary>
        /// <param name="endGameTime"></param>
        /// <param name="numerical">Will display as "X Days HH:MM" if true</param>
        /// <returns></returns>
        public string TimeLeft(int endGameTime, bool numerical = false, bool roundUp = false, bool returnWeeks = false, bool returnSeasons = false)
        {
            var timeDif = endGameTime - Now();
            if (timeDif < 0)
                timeDif = -timeDif;

            int seasons = 0, months = 0, weeks = 0, days = 0, hours = 0, minutes = 0;

            if (returnSeasons)
            {
                seasons = (int)Mathf.Floor(timeDif / GameTimePerSeason);
                timeDif -= seasons * GameTimePerSeason;
            }

            months = (int)Mathf.Floor(timeDif / GameTimePerMonth);
            timeDif -= months * GameTimePerMonth;

            if (returnWeeks)
            {
                weeks = (int)Mathf.Floor(timeDif / GameTimePerWeek);
                timeDif -= weeks * GameTimePerWeek;
            }

            days = (int)Mathf.Floor(timeDif / GameTimePerDay);
            timeDif -= days * GameTimePerDay;

            hours = (int)Mathf.Floor(timeDif / GameTimePerHour);
            timeDif -= hours * GameTimePerHour;

            minutes = (int)Mathf.Floor(timeDif);

            var seasonString = returnSeasons && seasons > 0 ? seasons + " Seasons, " : "";
            var monthString = months > 0 ? months + " Months, " : "";
            var weekString = returnWeeks && weeks > 0 ? weeks + " Weeks, " : "";
            var dayString = days > 0 ? days + " Days, " : "";
            var hourString = hours > 0 ? hours + " Hours, " : "";
            var minuteString = minutes + " Minutes";

            return seasonString + monthString + weekString + dayString + hourString + minuteString;
        }

        public string TimeLeft(TimeSpan timespan, bool numerical = false, bool roundUp = false, bool returnWeeks = false, bool returnSeasons = false) 
            => TimeLeft((int)(Now() + TimespanToGameTime(timespan)), numerical, roundUp, returnWeeks, returnSeasons);


        public TimeSpan RealSecondsToTimeSpan(float realSeconds)
        {
            float gameTime = RealSecondsToGameTime(realSeconds); // Convert real-world seconds to in-game minutes

            float years = GetYear(gameTime);
            gameTime -= years * GameTimePerYear;

            float months = GetMonth(gameTime);
            gameTime -= months * GameTimePerMonth;

            float days = GetDay(gameTime);
            gameTime -= days * GameTimePerDay;

            float hours = GetHour(gameTime);
            gameTime -= hours * GameTimePerHour;

            float minutes = GetMinute(gameTime);
            gameTime -= minutes;

            float seconds = gameTime * SecondsPerMinute; // Convert remaining minutes to seconds

            return new TimeSpan(seconds, minutes, hours, days, months, years);
        }
        public float RealSecondsToGameTime(float realSeconds) => realSeconds / RealWorldSecondsPerGameMinute;
        public float RealSecondsToDatePart(DatePart datePart, float realSeconds) => GameTimeToDatePart(datePart, RealSecondsToGameTime(realSeconds));
        
        
        private TimeSpan GameTimeToTimeSpan(float gameTime) 
        {
            var totalSeconds = gameTime * RealWorldSecondsPerGameMinute;
            var totalMinutes = (int)(totalSeconds / RealWorldSecondsPerGameMinute);
            totalSeconds -= totalMinutes * RealWorldSecondsPerGameMinute;
            var totalHours = totalMinutes / MinutesPerHour;
            totalMinutes -= totalHours * MinutesPerHour;
            var totalDays = totalHours / HoursPerDay;
            totalHours -= totalDays * HoursPerDay;
            var totalMonths = totalDays / daysPerMonth;
            totalDays -= totalMonths * daysPerMonth;
            var totalYears = totalMonths / NumberOfMonths;
            totalMonths -= totalYears * NumberOfMonths;

            return new TimeSpan(totalSeconds, totalMinutes, totalHours, totalDays, totalMonths, totalYears);
        }
        public float GameTimeToRealSeconds(float gameTime) => gameTime * RealWorldSecondsPerGameMinute;
        public float GameTimeToDatePart(DatePart datePart, float gameTime) => gameTime / GetComparisonValue(datePart);
        
        
        public float TimespanToGameTime(TimeSpan timeSpan)
        {
            float gameTime = 0;

            gameTime += timeSpan.Seconds / RealWorldSecondsPerGameMinute;
            gameTime += timeSpan.Minutes;
            gameTime += timeSpan.Hours * MinutesPerHour;
            gameTime += timeSpan.Days * GameTimePerDay;
            gameTime += timeSpan.Months * GameTimePerMonth;
            gameTime += timeSpan.Years * GameTimePerYear;

            return gameTime;
        }
        public float TimeSpanToRealSeconds(TimeSpan timeSpan) => GameTimeToRealSeconds(TimespanToGameTime(timeSpan));
        public float TimeSpanToDatePart(DatePart datePart, TimeSpan timeSpan) => RealSecondsToDatePart(datePart, TimeSpanToRealSeconds(timeSpan));
        
        
        public float DatePartToGameTime(DatePart datePart, float value) => GetComparisonValue(datePart) * value;
        public float DatePartToRealSeconds(DatePart datePart, float value) => GameTimeToRealSeconds(DatePartToGameTime(datePart, value));
        public TimeSpan DatePartToTimeSpan(DatePart datePart, float value) => RealSecondsToTimeSpan(DatePartToRealSeconds(datePart, value));
        


        /// <summary>
        /// Returns an approximate time left until endGameTime, with each time unit being optional. decimals will affect
        /// only the last value (minutes, hours, days, etc), and ignoreNoValue will ignore any time unit that is 0.
        /// </summary>
        /// <param name="endGameTime"></param>
        /// <param name="minutes"></param>
        /// <param name="hours"></param>
        /// <param name="days"></param>
        /// <param name="weeks"></param>
        /// <param name="months"></param>
        /// <param name="seasons"></param>
        /// <param name="years"></param>
        /// <param name="decimals"></param>
        /// <param name="ignoreNoValue"></param>
        /// <returns></returns>
        public string ApproximateTime(int endGameTime, bool minutes = false, bool hours = false, bool days = true,
            bool weeks = false, bool months = false, bool seasons = false, bool years = false, float decimals = 1,
            bool ignoreNoValue = true)
        {
            float timeDif = endGameTime - Now();
            if (timeDif < 0)
                timeDif = -timeDif;

            string result = "";
            float scaleFactor = Mathf.Pow(10, decimals);
            bool isLastUnit = false;

            if (years && timeDif >= GameTimePerYear)
            {
                isLastUnit = !(seasons || months || weeks || days || hours || minutes);
                var floatValue = Mathf.Round((timeDif / GameTimePerYear) * (isLastUnit ? scaleFactor : 1)) /
                                 (isLastUnit ? scaleFactor : 1);
                timeDif -= Mathf.Floor(floatValue) * GameTimePerYear;
                if (floatValue > 0 || !ignoreNoValue)
                    result += floatValue + " years, ";
            }

            if (seasons && timeDif >= GameTimePerSeason)
            {
                isLastUnit = !(months || weeks || days || hours || minutes);
                var floatValue = Mathf.Round((timeDif / GameTimePerSeason) * (isLastUnit ? scaleFactor : 1)) /
                                 (isLastUnit ? scaleFactor : 1);
                timeDif -= Mathf.Floor(floatValue) * GameTimePerSeason;
                if (floatValue > 0 || !ignoreNoValue)
                    result += floatValue + " seasons, ";
            }

            if (months && timeDif >= GameTimePerMonth)
            {
                isLastUnit = !(weeks || days || hours || minutes);
                var floatValue = Mathf.Round((timeDif / GameTimePerMonth) * (isLastUnit ? scaleFactor : 1)) /
                                 (isLastUnit ? scaleFactor : 1);
                timeDif -= Mathf.Floor(floatValue) * GameTimePerMonth;
                if (floatValue > 0 || !ignoreNoValue)
                    result += floatValue + " months, ";
            }

            if (weeks && timeDif >= GameTimePerWeek)
            {
                isLastUnit = !(days || hours || minutes);
                var floatValue = Mathf.Round((timeDif / GameTimePerWeek) * (isLastUnit ? scaleFactor : 1)) /
                                 (isLastUnit ? scaleFactor : 1);
                timeDif -= Mathf.Floor(floatValue) * GameTimePerWeek;
                if (floatValue > 0 || !ignoreNoValue)
                    result += floatValue + " weeks, ";
            }

            if (days && timeDif >= GameTimePerDay)
            {
                isLastUnit = !(hours || minutes);
                var floatValue = Mathf.Round((timeDif / GameTimePerDay) * (isLastUnit ? scaleFactor : 1)) /
                                 (isLastUnit ? scaleFactor : 1);
                timeDif -= Mathf.Floor(floatValue) * GameTimePerDay;
                if (floatValue > 0 || !ignoreNoValue)
                    result += floatValue + " days, ";
            }

            if (hours && timeDif >= GameTimePerHour)
            {
                isLastUnit = !(minutes);
                var floatValue = Mathf.Round((timeDif / GameTimePerHour) * (isLastUnit ? scaleFactor : 1)) /
                                 (isLastUnit ? scaleFactor : 1);
                timeDif -= Mathf.Floor(floatValue) * GameTimePerHour;
                if (floatValue > 0 || !ignoreNoValue)
                    result += floatValue + " hours, ";
            }

            if (minutes && timeDif >= 1)
            {
                isLastUnit = true;
                var floatValue = Mathf.Round((timeDif / 1) * (isLastUnit ? scaleFactor : 1)) /
                                 (isLastUnit ? scaleFactor : 1);
                if (floatValue > 0 || !ignoreNoValue)
                    result += floatValue + " minutes, ";
            }

            return result.TrimEnd(' ', ',');
        }
        
        private int GetSeason(float gameTime = -1)
        {
            var daysSinceNewYear = Mathf.FloorToInt(((gameTime < 0 ? Now() : gameTime) - GameTimeNewYear()) / GameTimePerDay);
            var currentSeasonIndex = (daysSinceNewYear - firstSeasonDaysFromNewYear) / DaysPerSeason;
            if (currentSeasonIndex < 0)
                currentSeasonIndex += seasonNames.Count;
            return currentSeasonIndex;
        }
        
        // Date Part
        
        /// <summary>
        /// Returns the value of the specified date part, based on the gameTime provided. If gameTime is not provided,
        /// Now() will be used.
        /// </summary>
        /// <param name="datePart"></param>
        /// <param name="gameTime"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public int GetDatePart(DatePart datePart, float gameTime = -1) =>
            datePart switch
            {
                DatePart.Second => GetSeconds(gameTime),
                DatePart.Minute => GetMinute(gameTime),
                DatePart.Hour => GetHour(gameTime),
                DatePart.Day => GetDay(gameTime),
                DatePart.Week => GetWeek(gameTime),
                DatePart.Month => GetMonth(gameTime),
                DatePart.Season => GetSeason(gameTime),
                DatePart.Year => GetYear(gameTime),
                _ => throw new ArgumentOutOfRangeException(nameof(datePart), datePart, null)
            };
        
        /// <summary>
        /// Provides the value of the specified date part since the start of the last comparison DatePart, based
        /// on the gameTime provided. If gameTime is not provided, Now() will be used.
        /// </summary>
        /// <param name="datePart"></param>
        /// <param name="comparison"></param>
        /// <param name="gameTime"></param>
        /// <returns></returns>
        public float DatePartInto(DatePart datePart, DatePart comparison, float gameTime = -1)
        {
            var currentGameTime = gameTime < 0 ? Now(false) : gameTime;
            var comparisonValue = GetComparisonValue(comparison);
            var datePartValue = GetComparisonValue(datePart);

            return (currentGameTime % comparisonValue) / datePartValue;
        }

        /// <summary>
        /// Provides the value of the specified date part until the start of the next comparison DatePart, based
        /// on the gameTime provided. If gameTime is not provided, Now() will be used.
        /// </summary>
        /// <param name="datePart"></param>
        /// <param name="comparison"></param>
        /// <param name="gameTime"></param>
        /// <returns></returns>
        public float DatePartUntil(DatePart datePart, DatePart comparison, float gameTime = -1)
        {
            var currentGameTime = gameTime < 0 ? Now(false) : gameTime;
            var comparisonValue = GetComparisonValue(comparison);
            var datePartValue = GetComparisonValue(datePart);

            
            var timeTillNextPart = comparisonValue - (currentGameTime % comparisonValue);
            return timeTillNextPart / datePartValue;
        }
        
        // Helper method
        private float GetComparisonValue(DatePart datePart) =>
            datePart switch
            {
                DatePart.Second => 1 / SecondsPerMinute,
                DatePart.Minute => 1,
                DatePart.Hour => GameTimePerHour,
                DatePart.Day => GameTimePerDay,
                DatePart.Week => GameTimePerWeek,
                DatePart.Month => GameTimePerMonth,
                DatePart.Season => GameTimePerSeason,
                DatePart.Year => GameTimePerYear,
                _ => throw new ArgumentOutOfRangeException(nameof(datePart), datePart, null)
            };

        // Helper Methods
        public float SecondsInto(DatePart comparison, float gameTime = -1) => DatePartInto(DatePart.Second, comparison, gameTime);
        public float MinutesInto(DatePart comparison, float gameTime = -1) => DatePartInto(DatePart.Minute, comparison, gameTime);
        public float HoursInto(DatePart comparison, float gameTime = -1) => DatePartInto(DatePart.Hour, comparison, gameTime);
        public float DaysInto(DatePart comparison, float gameTime = -1) => DatePartInto(DatePart.Day, comparison, gameTime);
        public float WeeksInto(DatePart comparison, float gameTime = -1) => DatePartInto(DatePart.Week, comparison, gameTime);
        public float MonthsInto(DatePart comparison, float gameTime = -1) => DatePartInto(DatePart.Month, comparison, gameTime);
        public float SeasonsInto(DatePart comparison, float gameTime = -1) => DatePartInto(DatePart.Season, comparison, gameTime);
        
        // Helper Methods
        public float SecondsUntil(DatePart comparison, float gameTime = -1) => DatePartUntil(DatePart.Second, comparison, gameTime);
        public float MinutesUntil(DatePart comparison, float gameTime = -1) => DatePartUntil(DatePart.Minute, comparison, gameTime);
        public float HoursUntil(DatePart comparison, float gameTime = -1) => DatePartUntil(DatePart.Hour, comparison, gameTime);
        public float DaysUntil(DatePart comparison, float gameTime = -1) => DatePartUntil(DatePart.Day, comparison, gameTime);
        public float WeeksUntil(DatePart comparison, float gameTime = -1) => DatePartUntil(DatePart.Week, comparison, gameTime);
        public float MonthsUntil(DatePart comparison, float gameTime = -1) => DatePartUntil(DatePart.Month, comparison, gameTime);
        public float SeasonsUntil(DatePart comparison, float gameTime = -1) => DatePartUntil(DatePart.Season, comparison, gameTime);

        // Helper Methods
        public float SecondsSinceNewYear(int gameTime = -1) => DatePartInto(DatePart.Second, DatePart.Year, gameTime);
        public float MinutesSinceNewYear(int gameTime = -1) => DatePartInto(DatePart.Minute, DatePart.Year, gameTime);
        public float HoursSinceNewYear(int gameTime = -1) => DatePartInto(DatePart.Hour, DatePart.Year, gameTime);
        public float DaysSinceNewYear(int gameTime = -1) => DatePartInto(DatePart.Day, DatePart.Year, gameTime);
        public float WeeksSinceNewYear(int gameTime = -1) => DatePartInto(DatePart.Week, DatePart.Year, gameTime);
        public float MonthsSinceNewYear(int gameTime = -1) => DatePartInto(DatePart.Month, DatePart.Year, gameTime);
        public float SeasonsSinceNewYear(int gameTime = -1) => DatePartInto(DatePart.Season, DatePart.Year, gameTime);
        
        public float SecondsUntilNewYear(int gameTime = -1) => DatePartUntil(DatePart.Second, DatePart.Year, gameTime);
        public float MinutesUntilNewYear(int gameTime = -1) => DatePartUntil(DatePart.Minute, DatePart.Year, gameTime);
        public float HoursUntilNewYear(int gameTime = -1) => DatePartUntil(DatePart.Hour, DatePart.Year, gameTime);
        public float DaysUntilNewYear(int gameTime = -1) => DatePartUntil(DatePart.Day, DatePart.Year, gameTime);
        public float WeeksUntilNewYear(int gameTime = -1) => DatePartUntil(DatePart.Week, DatePart.Year, gameTime);
        public float MonthsUntilNewYear(int gameTime = -1) => DatePartUntil(DatePart.Month, DatePart.Year, gameTime);
        public float SeasonsUntilNewYear(int gameTime = -1) => DatePartUntil(DatePart.Season, DatePart.Year, gameTime);
        
        
        

        public float GetStartOfDay(float gameTime = -1)
        {
            float dayFraction = (gameTime < 0 ? Now(false) : gameTime) % GameTimePerDay;
            return gameTime - dayFraction;
        }


        

        /// <summary>
        /// Returns a string representation of the time of day, without the date. Will be similar to "12:30pm", or if
        /// ignoreAmPm is true, it will be formatted similar to "3 hours, 13 minutes". If includeSeconds is true,
        /// seconds will be included.
        /// </summary>
        /// <param name="gameTime"></param>
        /// <param name="ignoreAmPm"></param>
        /// <param name="includeSeconds"></param>
        /// <returns></returns>
        public string TimeOnlyString(float gameTime, bool ignoreAmPm = false, bool includeSeconds = false)
        {
            var gameHour = GetHour(gameTime);
            var gameMinute = GetMinute(gameTime);
            var gameSecond = GetSeconds(gameTime);

            // Set other values
            var leadingZeroHour = gameHour < 10 && gameHour != 0 && !ignoreAmPm ? "0" : "";
            
            var leadingZeroMinute = gameMinute < 10 ? "0" : "";
            var leadingZeroSecond = gameSecond < 10 && includeSeconds ? "0" : "";
            var ampm = useAmPm && !ignoreAmPm ? gameHour >= (hoursPerDay / 2) ? "pm" : "am" : null;
            var displayHour = gameHour;
            
            if (useAmPm && !ignoreAmPm && gameHour == 0) displayHour = hoursPerDay / 2;
            if (useAmPm && !ignoreAmPm && gameHour > hoursPerDay / 2) displayHour = gameHour - (hoursPerDay / 2);
            
            if(ignoreAmPm){
                return includeSeconds ? $"{displayHour} hours, {gameMinute} minutes, {leadingZeroSecond}{gameSecond} seconds" : $"{displayHour} hours, {gameMinute} minutes";
            }

            return includeSeconds ? $"{leadingZeroHour}{displayHour}:{leadingZeroMinute}{gameMinute}:{leadingZeroSecond}{gameSecond}{ampm}" : $"{leadingZeroHour}{displayHour}:{leadingZeroMinute}{gameMinute}{ampm}";
        }





        



        // PAUSE LEVEL
        
        /// <summary>
        /// Sets the current pause level to the maximum of the current level and new level.
        /// </summary>
        /// <param name="newLevel"></param>
        /// <param name="saveLastPauseLevel"></param>
        public void SetPauseLevelMax(int newLevel, bool saveLastPauseLevel = true)
        {
            // Will only save if the level is different!
            if (saveLastPauseLevel && pauseLevel != newLevel)
                lastPauseLevel = pauseLevel;
            
            pauseLevel = Mathf.Max(pauseLevel, newLevel);
        }
        
        /// <summary>
        /// Sets the current pause level to the minimum of the current level and new level.
        /// </summary>
        /// <param name="newLevel"></param>
        public void SetPauseLevelMin(int newLevel) => pauseLevel = Mathf.Min(pauseLevel, newLevel);
        
        /// <summary>
        /// Sets the pauseLevel to the value of the lastPauseLevel
        /// </summary>
        public void ResetToLastPauseLevel() => pauseLevel = lastPauseLevel;
    }
}





/*

Whoah you're looking down here? Well, I want to update in the future with a more accurate model of the sun's position
...but out of scope for now!



 [Header("Seasonality")] 
        public int firstSeasonDaysFromNewYear = 0; // 0 = first season starts on New Year. This value should be in the first portion of the year.
        public bool northernHemisphere = true; // false for southern hemisphere
        public int solsticeDaysFromNewYear = 0; // 0 = new year is on the solstice, -18 = solstice is 18 days before new year, etc.
        [Range(0, 1)]
        public float summerSolsticeSunrise = 0.20f; // Represents 20% through the day
        [Range(0, 1)]
        public float summerSolsticeSunset = 0.80f; // Represents 80% through the day
        
        [Range(0, 1)]
        public float winterSolsticeSunrise = 0.30f; // Represents 30% through the day
        [Range(0, 1)]
        public float winterSolsticeSunset = 0.7f; // Represents 70% through the day

        [Header("Computed based on % above")]
        [SerializeField] private int _summerSolsticeSunriseGameTime;
        [SerializeField] private int _summerSolsticeSunsetGameTime;
        
        [SerializeField] private int _winterSolsticeSunriseGameTime;
        [SerializeField] private int _winterSolsticeSunsetGameTime;
        
        [SerializeField] private int _summerSolsticeSunriseHour;
        [SerializeField] private int _summerSolsticeSunriseMinute;
        
        [SerializeField] private int _summerSolsticeSunsetHour;
        [SerializeField] private int _summerSolsticeSunsetMinute;
        
        [SerializeField] private int _winterSolsticeSunriseHour;
        [SerializeField] private int _winterSolsticeSunriseMinute;
        
        [SerializeField] private int _winterSolsticeSunsetHour;
        [SerializeField] private int _winterSolsticeSunsetMinute;
        
        public AnimationCurve dayLengthCurve = new AnimationCurve(new Keyframe(0, 1), new Keyframe(0.5f, 0.5f), new Keyframe(1, 1)); // 1 = longest day, 0.5 = half longest day length



        
        
        /// <summary>
        /// Returns the gameTime of the Winter Solstice in the provided year. If year is -1, the current year is used.
        /// </summary>
        /// <param name="gameTime"></param>
        /// <returns></returns>
        public float WinterSolsticeGameTime(float gameTime = -1)
        {
            int year = gameTime == -1 ? GetYear(Now(false)) : GetYear(gameTime);
            float yearGameTime = GameTimeNewYear(year);

            return northernHemisphere
                ? Modulo(yearGameTime + solsticeDaysFromNewYear * GameTimePerDay, GameTimePerYear)
                : Modulo(yearGameTime + (solsticeDaysFromNewYear + (GameTimePerYear / (2 * GameTimePerDay))) * GameTimePerDay, GameTimePerYear);
        }

        /// <summary>
        /// Returns the gameTime of the Summer Solstice in the provided year. If year is -1, the current year is used.
        /// </summary>
        /// <param name="gameTime"></param>
        /// <returns></returns>
        public float SummerSolsticeGameTime(float gameTime = -1)
        {
            int year = gameTime == -1 ? GetYear(Now(false)) : GetYear(gameTime);
            float yearGameTime = GameTimeNewYear(year);

            return !northernHemisphere
                ? Modulo(yearGameTime + solsticeDaysFromNewYear * GameTimePerDay, GameTimePerYear)
                : Modulo(yearGameTime + (solsticeDaysFromNewYear + (GameTimePerYear / (2 * GameTimePerDay))) * GameTimePerDay, GameTimePerYear);
        }

        /// <summary>
        /// Returns the gameTime of the Spring Equinox in the provided year. If year is -1, the current year is used.
        /// </summary>
        /// <param name="gameTime"></param>
        /// <returns></returns>
        public float SpringEquinoxGameTime(float gameTime = -1)
        {
            return northernHemisphere
                ? Modulo((WinterSolsticeGameTime(gameTime) + (GameTimePerYear / 4)), GameTimePerYear)
                : Modulo((SummerSolsticeGameTime(gameTime) + (GameTimePerYear / 4)), GameTimePerYear);
        }

        /// <summary>
        /// Returns the gameTime of the Fall Equinox in the provided year. If year is -1, the current year is used.
        /// </summary>
        /// <param name="gameTime"></param>
        /// <returns></returns>
        public float FallEquinoxGameTime(float gameTime = -1)
        {
            return northernHemisphere
                ? Modulo((SummerSolsticeGameTime(gameTime) + (GameTimePerYear / 4)), GameTimePerYear)
                : Modulo((WinterSolsticeGameTime(gameTime) + (GameTimePerYear / 4)), GameTimePerYear);
        }


        /// <summary>
        /// Returns the gameTime of the next Winter Solstice from the provided gameTime. If year is -1, the Now() time is used.
        /// </summary>
        /// <param name="gameTime"></param>
        /// <returns></returns>
        public float NextWinterSolsticeGameTime(float gameTime = -1)
        {
            float yearGameTime = gameTime == -1 ? LastNewYear : GameTimeNewYear(GetYear(gameTime));
    
            return northernHemisphere
                ? Modulo(yearGameTime + solsticeDaysFromNewYear * GameTimePerDay, GameTimePerYear)
                : Modulo(yearGameTime + (solsticeDaysFromNewYear + (GameTimePerYear / (2 * GameTimePerDay))) * GameTimePerDay, GameTimePerYear);
        }

        /// <summary>
        /// Returns the gameTime of the next Summer Solstice from the provided gameTime. If year is -1, the Now() time is used.
        /// </summary>
        /// <param name="gameTime"></param>
        /// <returns></returns>
        public float NextSummerSolsticeGameTime(float gameTime = -1)
        {
            float yearGameTime = gameTime == -1 ? LastNewYear : GameTimeNewYear(GetYear(gameTime));

            return !northernHemisphere
                ? Modulo(yearGameTime + solsticeDaysFromNewYear * GameTimePerDay, GameTimePerYear)
                : Modulo(yearGameTime + (solsticeDaysFromNewYear + (GameTimePerYear / (2 * GameTimePerDay))) * GameTimePerDay, GameTimePerYear);
        }

        /// <summary>
        /// Returns the gameTime of the next Spring Equinox from the provided gameTime. If year is -1, the Now() time is used.
        /// </summary>
        /// <param name="gameTime"></param>
        /// <returns></returns>
        public float NextSpringEquinoxGameTime(float gameTime = -1)
        {
            return northernHemisphere
                ? Modulo((NextWinterSolsticeGameTime(gameTime) + (GameTimePerYear / 4)), GameTimePerYear)
                : Modulo((NextSummerSolsticeGameTime(gameTime) + (GameTimePerYear / 4)), GameTimePerYear);
        }

        /// <summary>
        /// Returns the gameTime of the next Fall Equinox from the provided gameTime. If year is -1, the Now() time is used.
        /// </summary>
        /// <param name="gameTime"></param>
        /// <returns></returns>
        public float NextFallEquinoxGameTime(float gameTime = -1)
        {
            return northernHemisphere
                ? Modulo((NextSummerSolsticeGameTime(gameTime) + (GameTimePerYear / 4)), GameTimePerYear)
                : Modulo((NextWinterSolsticeGameTime(gameTime) + (GameTimePerYear / 4)), GameTimePerYear);
        }



        /// <summary>
        /// Returns the FullDate() of the Winter Solstice in the provided year. If year is -1, the current year is used.
        /// </summary>
        /// <param name="gameTime"></param>
        /// <returns></returns>
        public string WinterSolsticeDate(float gameTime = -1) 
            => FullDate(Mathf.FloorToInt(WinterSolsticeGameTime(gameTime)));

        /// <summary>
        /// Returns the FullDate() of the Summer Solstice in the provided year. If year is -1, the current year is used.
        /// </summary>
        /// <param name="gameTime"></param>
        /// <returns></returns>
        public string SummerSolsticeDate(float gameTime = -1) 
            => FullDate(Mathf.FloorToInt(SummerSolsticeGameTime(gameTime)));

        /// <summary>
        /// Returns the FullDate() of the Spring Equinox in the provided year. If year is -1, the current year is used.
        /// </summary>
        /// <param name="gameTime"></param>
        /// <returns></returns>
        public string SpringEquinoxDate(float gameTime = -1) 
            => FullDate(Mathf.FloorToInt(SpringEquinoxGameTime(gameTime)));

        /// <summary>
        /// Returns the FullDate() of the Fall Equinox in the provided year. If year is -1, the current year is used.
        /// </summary>
        /// <param name="gameTime"></param>
        /// <returns></returns>
        public string FallEquinoxDate(float gameTime = -1) 
            => FullDate(Mathf.FloorToInt(FallEquinoxGameTime(gameTime)));

        /// <summary>
        /// Returns the FullDate() of the next Winter Solstice from the provided gameTime. If year is -1, the Now() time is used.
        /// </summary>
        /// <param name="gameTime"></param>
        /// <returns></returns>
        public string NextWinterSolsticeDate(float gameTime = -1) 
        {
            gameTime = gameTime < 0 ? Now(false) : gameTime;
            return FullDate(Mathf.FloorToInt(NextWinterSolsticeGameTime(gameTime + GameTimePerDay)));
        }

        /// <summary>
        /// Returns the FullDate() of the next Summer Solstice from the provided gameTime. If year is -1, the Now() time is used.
        /// </summary>
        /// <param name="gameTime"></param>
        /// <returns></returns>
        public string NextSummerSolsticeDate(float gameTime = -1) 
        {
            gameTime = gameTime < 0 ? Now(false) : gameTime;
            return FullDate(Mathf.FloorToInt(NextSummerSolsticeGameTime(gameTime + GameTimePerDay)));
        }

        /// <summary>
        /// Returns the FullDate() of the next Spring Equinox from the provided gameTime. If year is -1, the Now() time is used.
        /// </summary>
        /// <param name="gameTime"></param>
        /// <returns></returns>
        public string NextSpringEquinoxDate(float gameTime = -1) 
        {
            gameTime = gameTime < 0 ? Now(false) : gameTime;
            return FullDate(Mathf.FloorToInt(NextSpringEquinoxGameTime(gameTime + GameTimePerDay)));
        }

        /// <summary>
        /// Returns the FullDate() of the next Fall Equinox from the provided gameTime. If year is -1, the Now() time is used.
        /// </summary>
        /// <param name="gameTime"></param>
        /// <returns></returns>
        public string NextFallEquinoxDate(float gameTime = -1) 
        {
            gameTime = gameTime < 0 ? Now(false) : gameTime;
            return FullDate(Mathf.FloorToInt(NextFallEquinoxGameTime(gameTime + GameTimePerDay)));
        }

public float SunriseTime(float gameTime = -1)
        {
            gameTime = gameTime < 0 ? Now(false) : gameTime;
            // Calculate start of current day
            float startOfDay = GetStartOfDay(gameTime);
    
            float fractionOfYear = FractionOfYearFrom(solsticeDaysFromNewYear * GameTimePerDay, startOfDay);
            float sunriseFractionOfDay = Mathf.Lerp(summerSolsticeSunrise, winterSolsticeSunrise, fractionOfYear);
    
            Debug.Log($"gametime is {gameTime} and startOfDay is {startOfDay} fractionOfYear: {fractionOfYear}, sunriseFractionOfDay: {sunriseFractionOfDay}, returns {sunriseFractionOfDay * HoursPerDay}");
            return sunriseFractionOfDay * HoursPerDay;
        }

        public float SunsetTime(float gameTime = -1)
        {
            gameTime = gameTime < 0 ? Now(false) : gameTime;
            // Calculate start of current day
            float startOfDay = GetStartOfDay(gameTime);

            float fractionOfYear = FractionOfYearFrom(solsticeDaysFromNewYear * GameTimePerDay, startOfDay);
            float sunsetFractionOfDay = Mathf.Lerp(summerSolsticeSunset, winterSolsticeSunset, fractionOfYear);
            return sunsetFractionOfDay * HoursPerDay;
        }




        public float SunriseGameTime(float gameTime = -1) => SunriseTime(gameTime) * MinutesPerHour * HoursPerDay;
        public float SunsetGameTime(float gameTime = -1) => SunsetTime(gameTime) * MinutesPerHour * HoursPerDay;
        
        public float LengthOfDayAsFractionOfDay(float gameTime = -1) => SunsetTime(gameTime) - SunriseTime(gameTime);

        public TimeSpan LengthOfDay(float gameTime = -1)
        {
            var lengthOfDayAsFractionOfDay = LengthOfDayAsFractionOfDay(gameTime);

            // Convert to in-game units
            float lengthOfDayInHours = lengthOfDayAsFractionOfDay * HoursPerDay;
            float lengthOfDayInDays = lengthOfDayInHours / HoursPerDay;
            float remainingHours = lengthOfDayInHours % HoursPerDay;
            float remainingMinutes = (lengthOfDayAsFractionOfDay * HoursPerDay * MinutesPerHour) % MinutesPerHour;
            float remainingSeconds = (lengthOfDayAsFractionOfDay * HoursPerDay * MinutesPerHour * SecondsPerMinute) % SecondsPerMinute;
            
            return new TimeSpan(remainingSeconds, remainingMinutes, remainingHours, lengthOfDayInDays, 0, 0);
        }
        
        public TimeSpan LengthOfNight(float gameTime = -1)
        {
            // Calculate the start of the current day
            gameTime = gameTime < 0 ? GetStartOfDay(Now(false)) : GetStartOfDay(gameTime);

            var lengthOfDayAsFractionOfDay = LengthOfDayAsFractionOfDay(gameTime);
            var lengthOfNightAsFractionOfDay = 1 - lengthOfDayAsFractionOfDay;

            // Convert to in-game units
            float lengthOfNightInHours = lengthOfNightAsFractionOfDay * HoursPerDay;
            float remainingHours = lengthOfNightInHours % HoursPerDay;
            float remainingMinutes = (lengthOfNightAsFractionOfDay * HoursPerDay * MinutesPerHour) % MinutesPerHour;
            float remainingSeconds = (lengthOfNightAsFractionOfDay * HoursPerDay * MinutesPerHour * SecondsPerMinute) % SecondsPerMinute;

            return new TimeSpan(remainingSeconds, remainingMinutes, remainingHours);
        }


       
        
        public string SunriseTimeString(int gameTime = -1)
        {
            float sunriseTime = SunriseGameTime(gameTime < 0 ? Now(false) : gameTime);
            return TimeOnlyString(sunriseTime);
        }

        public string SunsetTimeString(int gameTime = -1)
        {
            float sunsetTime = SunsetGameTime(gameTime < 0 ? Now(false) : gameTime);
            return TimeOnlyString(sunsetTime);
        }

        public string LengthOfDayString(int gameTime = -1)
        {
            TimeSpan lengthOfDayTimeSpan = LengthOfDay(gameTime < 0 ? Now(false) : gameTime);
            return TimeOnlyString(TimespanToGameTime(lengthOfDayTimeSpan), true);
        }
        
        public string LengthOfNightString(int gameTime = -1)
        {
            TimeSpan lengthOfNightTimeSpan = LengthOfNight(gameTime < 0 ? Now(false) : gameTime);
            return TimeOnlyString(TimespanToGameTime(lengthOfNightTimeSpan), true);
        }
        
        public float FractionOfYearFrom(float fromGameTime, float gameTime = -1) 
            => (((gameTime < 0 ? Now(false) : gameTime) - fromGameTime) / GameTimePerYear % 1 + 1) % 1;


        public float FractionOfYearPassed(float gameTime = -1) 
            => FractionOfYearFrom(0, gameTime);

        public float FractionOfYearAtSummerSolstice(float gameTime = -1) 
            => ((float)solsticeDaysFromNewYear / DaysPerYear) + (northernHemisphere ? 0.5f : 0f);

        public float FractionOfYearSinceSummerSolstice(float gameTime = -1) 
        {
            float summerSolsticeGameTime = FractionOfYearAtSummerSolstice(gameTime) * GameTimePerYear;
            return FractionOfYearFrom(summerSolsticeGameTime, gameTime);
        }


        

        // Sunrise & Sunset
        public float SolsticeSunriseTime => GameTimePerDay * summerSolsticeSunrise;
        public float SolsticeSunsetTime => GameTimePerDay * summerSolsticeSunset;
        
        
        
*/