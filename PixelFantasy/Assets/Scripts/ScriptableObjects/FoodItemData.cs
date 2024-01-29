using Sirenix.OdinInspector;
using UnityEngine;

namespace ScriptableObjects
{
    public interface IFoodItem
    {
        EFoodType FoodType { get; set; }
        float FoodNutrition { get; set; }
    }
    
    
    [CreateAssetMenu(fileName = "RawFoodItemData", menuName = "ItemData/RawFoodItemData", order = 1)]
    public class FoodItemData : ItemData, IFoodItem
    {
        [ShowInInspector, TitleGroup("Food Data")] [TitleGroup("Food Data")] public EFoodType FoodType { get; set; }
        [ShowInInspector, TitleGroup("Food Data")] [TitleGroup("Food Data")] public float FoodNutrition { get; set; }
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
