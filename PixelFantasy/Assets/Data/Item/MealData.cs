using System.Collections.Generic;
using Databrain.Attributes;
using UnityEngine;

namespace Data.Item
{
    public class MealData : ItemData
    {
        [ExposeToInspector, DatabrainSerialize] public string CraftersUID;
        [ExposeToInspector, DatabrainSerialize] public List<RawFoodData> IngredientsUsed;
        
        public MealSettings MealSettings => Settings as MealSettings;
    }
}
