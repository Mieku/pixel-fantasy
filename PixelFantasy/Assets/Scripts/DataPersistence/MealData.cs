using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using ScriptableObjects;
using UnityEngine;

public class MealData : ItemData
{
    public string CraftersUID;
    public List<RawFoodData> IngredientsUsed;
    
    [JsonIgnore] public MealSettings MealSettings => (MealSettings) Settings;
}