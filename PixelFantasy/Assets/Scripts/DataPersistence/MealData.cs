using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MealData : ItemData
{
    public string CraftersUID;
    public List<RawFoodData> IngredientsUsed;
        
    public MealSettings MealSettings => Settings as MealSettings;
}