using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Databrain;
using Managers;
using ScriptableObjects;
using TaskSystem;
using UnityEngine;

namespace Data.Item
{
    public class MealSettings : ItemSettings
    {
        [SerializeField] private EMealQuality _mealQuality;
        [SerializeField] private float _nutrition;
        
        [SerializeField] private MealRequirements _mealRequirements;

        public EFoodType FoodType => EFoodType.Meal;
        public EMealQuality Quality => _mealQuality;
        public float Nutrition => _nutrition;
        public MealRequirements MealRequirements => _mealRequirements.Clone();
    }

    public enum EMealQuality
    {
        [Description("Simple")] Simple = 1,
        [Description("Fine")] Fine = 2, 
        [Description("Complex")] Complex = 3,
        [Description("Well Rounded")] WellRounded = 4,
    }
    
    [Serializable]
    public class MealRequirements
    {
        [SerializeField] private int _minCraftingSkillLevel;
        [SerializeField] private ETaskType _craftingSkill = ETaskType.Cooking;
        [SerializeField] private float _workCost;
        [SerializeField] private EToolType _requiredCraftingToolType;
        [SerializeField] private List<Ingredient> _ingredients;
        
        public float WorkCost => _workCost;
        public EToolType RequiredCraftingToolType => _requiredCraftingToolType;
        public int MinCraftingSkillLevel => _minCraftingSkillLevel;
        public ETaskType CraftingSkill => _craftingSkill;
        
        public List<Ingredient> GetIngredients()
        {
            List<Ingredient> clone = new List<Ingredient>(_ingredients);
            return clone;
        }
        
        public string IngredientsList
        {
            get
            {
                string materialsList = "";
                foreach (var cost in _ingredients)
                {
                    materialsList += cost.Amount + "x " + cost.FoodType.GetDescription() + "\n";
                }
                return materialsList;
            }
        }

        public bool IngredientsAreAvailable
        {
            get
            {
                foreach (var ingredient in _ingredients)
                {
                    if (!ingredient.CanAfford()) return false;
                }
        
                return true;
            }
        }

        public bool SomeoneHasCraftingSkillNeeded
        {
            get
            { 
                // TODO: Build me!
                return true;
            }
        }

        public bool CanBeCrafted => IngredientsAreAvailable && SomeoneHasCraftingSkillNeeded;
        
        public MealRequirements Clone()
        {
            MealRequirements copy = (MealRequirements)this.MemberwiseClone();
            copy._ingredients = this._ingredients.Select(Ingredient => Ingredient.Clone()).ToList();
            return copy;
        }
    }

    [Serializable]
    public class Ingredient
    {
        public EFoodType FoodType;
        public int Amount;

        public Ingredient Clone()
        {
            return new Ingredient()
            {
                FoodType = this.FoodType,
                Amount = this.Amount
            };
        }
        
        public bool CanAfford()
        {
            return InventoryManager.Instance.AreFoodTypesAvailable(FoodType, Amount);
        }
    }
}
