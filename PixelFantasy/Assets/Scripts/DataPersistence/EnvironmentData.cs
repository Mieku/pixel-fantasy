using System;
using Newtonsoft.Json;

namespace DataPersistence
{
    [Serializable]
    public class EnvironmentData
    {
        [JsonRequired] public float TimeOfDay;
    }
}
