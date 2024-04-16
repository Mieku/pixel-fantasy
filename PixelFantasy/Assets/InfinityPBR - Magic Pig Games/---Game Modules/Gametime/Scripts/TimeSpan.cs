
namespace InfinityPBR.Modules
{
    [System.Serializable]
    public class TimeSpan
    {
        public float Seconds { get; set; }
        public float Minutes { get; set; }
        public float Hours { get; set; }
        public float Days { get; set; }
        public float Months { get; set; }
        public float Years { get; set; }
    
        // Constructor
        public TimeSpan(float seconds = 0f, float minutes = 0f, float hours = 0f, float days = 0f, float months = 0f, float years = 0f) 
        {
            Years = years;
            Months = months;
            Days = days;
            Hours = hours;
            Minutes = minutes;
            Seconds = seconds;
        }
            
        // Add another TimeSpan to this one.
        public TimeSpan Add(TimeSpan other)
        {
            return new TimeSpan
            {
                Seconds = Seconds + other.Seconds,
                Minutes = Minutes + other.Minutes,
                Hours = Hours + other.Hours,
                Days = Days + other.Days,
                Months = Months + other.Months,
                Years = Years + other.Years,
            };
        }
        
        // Add another TimeSpan to this one.
        public TimeSpan Subtract(TimeSpan other)
        {
            return new TimeSpan
            {
                Seconds = Seconds - other.Seconds,
                Minutes = Minutes - other.Minutes,
                Hours = Hours - other.Hours,
                Days = Days - other.Days,
                Months = Months - other.Months,
                Years = Years - other.Years,
            };
        }
            
        // Multiply this TimeSpan by a float.
        public TimeSpan Multiply(float multiplier)
        {
            return new TimeSpan
            {
                Seconds = Seconds * multiplier,
                Minutes = Minutes * multiplier,
                Hours = Hours * multiplier,
                Days = Days * multiplier,
                Months = Months * multiplier,
                Years = Years * multiplier,
            };
        }
    }
}
