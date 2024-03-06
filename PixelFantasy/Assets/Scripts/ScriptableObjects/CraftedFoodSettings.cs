using Sirenix.OdinInspector;
using UnityEngine;

namespace ScriptableObjects
{
    [CreateAssetMenu(fileName = "CraftedFoodSettings", menuName = "Settings/Items/Crafted Food Settings")]
    public class CraftedFoodSettings : CraftedItemSettings, IFoodItem
    {
        [ShowInInspector, TitleGroup("Food Settings")] public EFoodType FoodType { get; set; }
        [ShowInInspector, TitleGroup("Food Settings")] public float FoodNutrition { get; set; }
    }
}
