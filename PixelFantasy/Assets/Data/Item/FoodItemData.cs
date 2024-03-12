using ScriptableObjects;
using UnityEngine;

namespace Data.Item
{
    public interface IFoodItem
    {
        FoodItemData.EFoodType FoodType { get; set; }
        float FoodNutrition { get; set; }
    }
    
    public class FoodItemData : ItemData, IFoodItem
    {
        public EFoodType FoodType { get; set; }
        public float FoodNutrition { get; set; }
        
        public enum EFoodType
        {
            Fruit = 0,
            Vegitable = 1,
            Grain = 2,
            Meat = 3,
            Dairy = 4,
        }
    }
}
