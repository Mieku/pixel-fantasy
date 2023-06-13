using System.Collections.Generic;
using System.Linq;
using Characters;
using ScriptableObjects;

namespace Managers
{
    public class UnitsManager : Singleton<UnitsManager>
    {
        public List<UnitState> AllUnits => GetComponentsInChildren<UnitState>().ToList();
        
        public List<UnitState> UnemployedUnits
        {
            get
            {
                var allUnits = AllUnits;
                return allUnits.Where(unit => unit.AssignedWorkRoom == null).ToList();
            }
        }

        public UnitState GetUnit(string fullname)
        {
            return AllUnits.Find(unit => unit.FullName == fullname);
        }
    }
}
