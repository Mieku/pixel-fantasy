using System.ComponentModel;
using Databrain;
using UnityEngine;

namespace Data.Item
{
    public class RawFoodSettings : ItemDataSettings
    {
        [SerializeField] private EFoodType _foodType;
        [SerializeField] private float _nutrition;

        public EFoodType FoodType => _foodType;
        public float Nutrition => _nutrition;
    }
    
    public enum EFoodType
    {
        [Description("Produce")] Produce = 0,
        [Description("Grain")] Grain = 1,
        [Description("Meat")] Meat = 2,
        [Description("Dairy")] Dairy = 3,
        [Description("Meal")] Meal = 5,
    }
}
