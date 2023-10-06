using Sirenix.OdinInspector;
using Systems.SmartObjects.Scripts;
using UnityEngine;

namespace ScriptableObjects
{
    [CreateAssetMenu(fileName = "RawFoodItemData", menuName = "ItemData/RawFoodItemData", order = 1)]
    public class RawFoodItemData : ItemData
    {
        [TitleGroup("Raw Food Data")] public EFoodType FoodType;
        [TitleGroup("Raw Food Data")] public float FoodNutrition;
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
