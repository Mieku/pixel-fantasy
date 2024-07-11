
using Newtonsoft.Json;
using ScriptableObjects;

public class RawFoodData : ItemData
{
    [JsonIgnore] public RawFoodSettings RawFoodSettings => (RawFoodSettings) Settings;
}
