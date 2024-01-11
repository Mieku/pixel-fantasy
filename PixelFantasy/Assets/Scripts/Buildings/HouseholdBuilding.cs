using System.Collections.Generic;
using System.Linq;
using Characters;
using Items;
using ScriptableObjects;
using Sirenix.Utilities;
using UnityEngine;

namespace Buildings
{
    public class HouseholdBuilding : Building
    {
        public override BuildingType BuildingType => BuildingType.Home;
        public BedFurniture DoubleBed;
        public List<BedFurniture> AdditionalBeds;

        public string HeadHousehold;
        public string Partner;
        public List<string> Children;

        private const float RECOMMENDED_DAILY_NUTRITION_PER_RESIDENT = 0.75f;

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
                    var foodItem = availableItem.GetItemData() as RawFoodItemData;
                    if (foodItem != null)
                    {
                        result += foodItem.FoodNutrition;
                    }
                }

                return result;
            }
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
            BuildingName = $"{unit.GetUnitState().LastName} {BuildingData.ConstructionName}";
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
                if (singleBed.IsAvailable(unit))
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
            
            unit.GetUnitState().AssignedHome = this;
        }

        public override void RemoveOccupant(Unit unit)
        {
            _occupants.Remove(unit);
            
            unit.GetUnitState().AssignedHome = null;
        }
    }
}
