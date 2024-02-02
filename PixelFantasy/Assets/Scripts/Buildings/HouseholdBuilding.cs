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

        public void AssignHeadHousehold(Kinling kinling)
        {
            HeadHousehold = kinling.UniqueId;
            AddOccupant(kinling);
            DoubleBed.AssignKinling(kinling);
            BuildingName = $"{kinling.LastName} {BuildingData.ConstructionName}";
        }

        public void AssignPartner(Kinling kinling)
        {
            Partner = kinling.UniqueId;
            AddOccupant(kinling);
            DoubleBed.AssignKinling(kinling);
        }

        public void AssignChild(Kinling kinling)
        {
            Children.Add(kinling.UniqueId);
            AddOccupant(kinling);

            foreach (var singleBed in AdditionalBeds)
            {
                if (singleBed.IsUnassigned(kinling))
                {
                    singleBed.AssignKinling(kinling);
                    return;
                }
            }
            
            Debug.LogError($"Could not assign bed for child");
        }

        public override void AddOccupant(Kinling kinling)
        {
            _occupants.Add(kinling);
            
            kinling.AssignedHome = this;
        }

        public override void RemoveOccupant(Kinling kinling)
        {
            _occupants.Remove(kinling);
            
            kinling.AssignedHome = null;
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
