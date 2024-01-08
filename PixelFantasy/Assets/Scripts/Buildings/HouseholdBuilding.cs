using System.Collections.Generic;
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
