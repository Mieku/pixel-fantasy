using System;
using Managers;
using Newtonsoft.Json;

namespace DataPersistence
{
    [Serializable]
    public class EnvironmentData
    {
        [JsonRequired] public float TimeOfDay;
        public int Day = 1;
        public EMonth Month = EMonth.Solara;
        public int TotalDays = 1;
    }
}
