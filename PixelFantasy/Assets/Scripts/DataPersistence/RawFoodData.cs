
using System;
using Newtonsoft.Json;
using ScriptableObjects;

[Serializable]
public class RawFoodData : ItemData
{
    [JsonIgnore] public RawFoodSettings RawFoodSettings => (RawFoodSettings) Settings;
}
