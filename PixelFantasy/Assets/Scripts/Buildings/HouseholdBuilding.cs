using Characters;
using ScriptableObjects;
using Sirenix.Utilities;

namespace Buildings
{
    public class HouseholdBuilding : Building
    {
        public override BuildingType BuildingType => BuildingType.Home;

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
