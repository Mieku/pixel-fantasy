using System.Collections.Generic;
using System.Linq;
using Characters;
using Items;
using ScriptableObjects;
using Sirenix.Utilities;
using UnityEngine;

namespace Buildings
{
    public interface IEateryBuilding : IBuilding
    {
        public Item ClaimBestAvailableFood();
    }
    
    public class HouseholdBuilding : Building, IEateryBuilding
    {
        public override BuildingType BuildingType => BuildingType.Home;
        public BedFurniture DoubleBed;
        public List<BedFurniture> AdditionalBeds;

        public string HeadHousehold;
        public string Partner;
        public List<string> Children;
        public bool InMatingMode { get; protected set; }

        private const float RECOMMENDED_DAILY_NUTRITION_PER_RESIDENT = 0.75f;

        public bool HasSpaceForChildren()
        {
            return Children.Count < AdditionalBeds.Count;
        }
        
        public int NumResidents
        {
            get
            {
                int result = 0;
                if (!string.IsNullOrEmpty(HeadHousehold)) result++;
                if (!string.IsNullOrEmpty(Partner)) result++;
                result += Children.Count(child => !string.IsNullOrEmpty(child));

                return result;
            }
        }

        public float SuggestedStoredNutrition => RECOMMENDED_DAILY_NUTRITION_PER_RESIDENT * NumResidents;

        public float CurrentStoredNutrition
        {
            get
            {
                float result = 0;
                var storages = GetBuildingStorages();
                List<Item> storedFood = new List<Item>();
                foreach (var storage in storages)
                {
                    var food = storage.GetAllFoodItems(false, true);
                    storedFood.AddRange(food);
                }

                foreach (var availableItem in storedFood)
                {
                    if (availableItem.GetItemData() is IFoodItem foodItem)
                    {
                        result += foodItem.FoodNutrition;
                    }
                }

                return result;
            }
        }

        public Item ClaimBestAvailableFood()
        {
            var storages = GetBuildingStorages();
            List<Item> storedFood = new List<Item>();
            foreach (var storage in storages)
            {
                var food = storage.GetAllFoodItems(false, false);
                storedFood.AddRange(food);
            }

            if (storedFood.Count == 0)
            {
                return null;
            }
            
            var selectedFood = storedFood.OrderByDescending(food => ((IFoodItem)food.GetItemData()).FoodNutrition).ToList()[0];
            selectedFood.ClaimItem();
            return selectedFood;
        }
        
        public bool IsVacant
        {
            get
            {
                if (_state != BuildingState.Built)
                {
                    return false;
                }

                return _occupants.IsNullOrEmpty();
            }
        }

        public void AssignHeadHousehold(Unit unit)
        {
            HeadHousehold = unit.UniqueId;
            AddOccupant(unit);
            DoubleBed.AssignKinling(unit);
            BuildingName = $"{unit.LastName} {BuildingData.ConstructionName}";
        }

        public void AssignPartner(Unit unit)
        {
            Partner = unit.UniqueId;
            AddOccupant(unit);
            DoubleBed.AssignKinling(unit);
        }

        public void AssignChild(Unit unit)
        {
            Children.Add(unit.UniqueId);
            AddOccupant(unit);

            foreach (var singleBed in AdditionalBeds)
            {
                if (singleBed.IsUnassigned(unit))
                {
                    singleBed.AssignKinling(unit);
                    return;
                }
            }
            
            Debug.LogError($"Could not assign bed for child");
        }

        public override void AddOccupant(Unit unit)
        {
            _occupants.Add(unit);
            
            unit.AssignedHome = this;
        }

        public override void RemoveOccupant(Unit unit)
        {
            _occupants.Remove(unit);
            
            unit.AssignedHome = null;
        }

        protected override bool IsInternalViewAllowed()
        {
            bool result = base.IsInternalViewAllowed();
            if (result)
            {
                result = !InMatingMode;
            }

            return result;
        }

        public void TriggerMatingMode(bool isOn)
        {
            if(InMatingMode == isOn) return;
            
            InMatingMode = isOn;

            if (isOn)
            {
                TryToggleInternalView(false);
                _animator.SetAnimation(EBuildingAnimation.Mating);
            }
            else
            {
                TryToggleInternalView(_defaultToInternalView);
                _animator.SetAnimation(EBuildingAnimation.None);
            }
        }
    }
}
