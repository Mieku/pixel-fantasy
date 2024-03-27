using System;
using UnityEngine;

namespace Characters
{
    [Serializable]
    public class Schedule
    {
        [SerializeField] private ScheduleOption Hour0;
        [SerializeField] private ScheduleOption Hour1;
        [SerializeField] private ScheduleOption Hour2;
        [SerializeField] private ScheduleOption Hour3;
        [SerializeField] private ScheduleOption Hour4;
        [SerializeField] private ScheduleOption Hour5;
        [SerializeField] private ScheduleOption Hour6;
        [SerializeField] private ScheduleOption Hour7;
        [SerializeField] private ScheduleOption Hour8;
        [SerializeField] private ScheduleOption Hour9;
        [SerializeField] private ScheduleOption Hour10;
        [SerializeField] private ScheduleOption Hour11;
        [SerializeField] private ScheduleOption Hour12;
        [SerializeField] private ScheduleOption Hour13;
        [SerializeField] private ScheduleOption Hour14;
        [SerializeField] private ScheduleOption Hour15;
        [SerializeField] private ScheduleOption Hour16;
        [SerializeField] private ScheduleOption Hour17;
        [SerializeField] private ScheduleOption Hour18;
        [SerializeField] private ScheduleOption Hour19;
        [SerializeField] private ScheduleOption Hour20;
        [SerializeField] private ScheduleOption Hour21;
        [SerializeField] private ScheduleOption Hour22;
        [SerializeField] private ScheduleOption Hour23;

        public Schedule()
        {
            SetDefaultValues();
        }

        public ScheduleOption GetHour(int hour)
        {
            switch (hour)
            {
                case 0:
                    return Hour0;
                case 1:
                    return Hour1;
                case 2:
                    return Hour2;
                case 3:
                    return Hour3;
                case 4:
                    return Hour4;
                case 5:
                    return Hour5;
                case 6:
                    return Hour6;
                case 7:
                    return Hour7;
                case 8:
                    return Hour8;
                case 9:
                    return Hour9;
                case 10:
                    return Hour10;
                case 11:
                    return Hour11;
                case 12:
                    return Hour12;
                case 13:
                    return Hour13;
                case 14:
                    return Hour14;
                case 15:
                    return Hour15;
                case 16:
                    return Hour16;
                case 17:
                    return Hour17;
                case 18:
                    return Hour18;
                case 19:
                    return Hour19;
                case 20:
                    return Hour20;
                case 21:
                    return Hour21;
                case 22:
                    return Hour22;
                case 23:
                    return Hour23;
            }

            Debug.LogError($"Unknown Hour Requested: {hour}");
            return Hour0;
        }
        
        public void SetHour(int hour, ScheduleOption option)
        {
            switch (hour)
            {
                case 0:
                    Hour0 = option;
                    break;
                case 1:
                    Hour1 = option;
                    break;
                case 2:
                    Hour2 = option;
                    break;
                case 3:
                    Hour3 = option;
                    break;
                case 4:
                    Hour4 = option;
                    break;
                case 5:
                    Hour5 = option;
                    break;
                case 6:
                    Hour6 = option;
                    break;
                case 7:
                    Hour7 = option;
                    break;
                case 8:
                    Hour8 = option;
                    break;
                case 9:
                    Hour9 = option;
                    break;
                case 10:
                    Hour10 = option;
                    break;
                case 11:
                    Hour11 = option;
                    break;
                case 12:
                    Hour12 = option;
                    break;
                case 13:
                    Hour13 = option;
                    break;
                case 14:
                    Hour14 = option;
                    break;
                case 15:
                    Hour15 = option;
                    break;
                case 16:
                    Hour16 = option;
                    break;
                case 17:
                    Hour17 = option;
                    break;
                case 18:
                    Hour18 = option;
                    break;
                case 19:
                    Hour19 = option;
                    break;
                case 20:
                    Hour20 = option;
                    break;
                case 21:
                    Hour21 = option;
                    break;
                case 22:
                    Hour22 = option;
                    break;
                case 23:
                    Hour23 = option;
                    break;
            }
        }

        public void SetDefaultValues()
        {
            SetHour(0, ScheduleOption.Sleep);
            SetHour(1, ScheduleOption.Sleep);
            SetHour(2, ScheduleOption.Sleep);
            SetHour(3, ScheduleOption.Sleep);
            SetHour(4, ScheduleOption.Sleep);
            SetHour(5, ScheduleOption.Sleep);
            SetHour(6, ScheduleOption.Sleep);
            SetHour(7, ScheduleOption.Recreation);
            SetHour(8, ScheduleOption.Work);
            SetHour(9, ScheduleOption.Work);
            SetHour(10, ScheduleOption.Work);
            SetHour(11, ScheduleOption.Work);
            SetHour(12, ScheduleOption.Work);
            SetHour(13, ScheduleOption.Work);
            SetHour(14, ScheduleOption.Work);
            SetHour(15, ScheduleOption.Work);
            SetHour(16, ScheduleOption.Work);
            SetHour(17, ScheduleOption.Work);
            SetHour(18, ScheduleOption.Work);
            SetHour(19, ScheduleOption.Work);
            SetHour(20, ScheduleOption.Work);
            SetHour(21, ScheduleOption.Recreation);
            SetHour(22, ScheduleOption.Recreation);
            SetHour(23, ScheduleOption.Recreation);
        }
    }
    
    public enum ScheduleOption
    {
        Sleep,
        Work,
        Recreation,
    }
}
