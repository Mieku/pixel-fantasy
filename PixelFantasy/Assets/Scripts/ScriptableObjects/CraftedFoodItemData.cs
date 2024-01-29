using Sirenix.OdinInspector;
using UnityEngine;

namespace ScriptableObjects
{
    [CreateAssetMenu(fileName = "CraftedFoodItemData", menuName = "ItemData/CraftedItemData/CraftedFoodItemData", order = 1)]
    public class CraftedFoodItemData : CraftedItemData, IFoodItem
    {
        [ShowInInspector, TitleGroup("Food Data")] [TitleGroup("Food Data")] public EFoodType FoodType { get; set; }
        [ShowInInspector, TitleGroup("Food Data")] [TitleGroup("Food Data")] public float FoodNutrition { get; set; }
    }
}
