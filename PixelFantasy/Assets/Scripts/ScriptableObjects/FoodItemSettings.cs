using Sirenix.OdinInspector;
using UnityEngine;

namespace ScriptableObjects
{
    public interface IFoodItem
    {
        EFoodType FoodType { get; set; }
        float FoodNutrition { get; set; }
    }
    
    
    [CreateAssetMenu(fileName = "FoodItemSettings", menuName = "Settings/Items/Food Item Settings")]
    public class FoodItemSettings : ItemSettings, IFoodItem
    {
        [ShowInInspector, TitleGroup("Food Settings")] public EFoodType FoodType { get; set; }
        [ShowInInspector, TitleGroup("Food Settings")] public float FoodNutrition { get; set; }
    }

    public enum EFoodType
    {
        Fruit = 0,
        Vegitable = 1,
        Grain = 2,
        Meat = 3,
        Dairy = 4,
    }
}
