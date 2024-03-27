using ScriptableObjects;
using UnityEngine;

namespace Data.Item
{
    // public interface IFoodItem
    // {
    //     RawFoodData.EFoodType FoodType { get; set; }
    //     float FoodNutrition { get; set; }
    // }
    
    public class RawFoodData : ItemData
    {
        public RawFoodSettings RawFoodSettings => Settings as RawFoodSettings;

    }
}
