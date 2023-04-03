using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Characters;
using Gods;
using UnityEngine;

public class UnitsManager : God<UnitsManager>
{
    public List<UnitState> AllUnits => GetComponentsInChildren<UnitState>().ToList();

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
