using System.Collections.Generic;
using System.Linq;
using Characters;
using ScriptableObjects;

namespace Gods
{
    public class UnitsManager : God<UnitsManager>
    {
        public List<UnitState> AllUnits => GetComponentsInChildren<UnitState>().ToList();

        public List<UnitState> UnitsWithProfession(ProfessionData profession)
        {
            var allUnits = AllUnits;
            return allUnits.Where(unit => unit.Profession == profession).ToList();
        }
    
        public List<UnitState> UnemployedUnits
        {
            get
            {
                var allUnits = AllUnits;
                return allUnits.Where(unit => unit.Occupation == null).ToList();
            }
        }

        public UnitState GetUnit(string fullname)
        {
            return AllUnits.Find(unit => unit.FullName == fullname);
        }
    }
}
