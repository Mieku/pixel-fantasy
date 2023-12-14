using System;
using System.Collections.Generic;
using Buildings;
using Characters;
using Items;
using ScriptableObjects;
using UnityEngine;

namespace Managers
{
    public class FamilyManager : Singleton<FamilyManager>
    {
        [SerializeField] private List<Family> _families = new List<Family>();

        public Family GetPlayerFamily()
        {
            foreach (var family in _families)
            {
                if (family.IsPlayer)
                {
                    return family;
                }
            }

            return null;
        }
        
        public Family GetFamily(string uid)
        {
            foreach (var family in _families)
            {
                if (family.UID == uid)
                {
                    return family;
                }
            }

            return null;
        }

        public Family GetFamily(UnitState unitState)
        {
            foreach (var family in _families)
            {
                if (family.IsFamilyMember(unitState))
                {
                    return family;
                }
            }

            return null;
        }

        public void AddFamily(Family family)
        {
            if (GetFamily(family.UID) != null)
            {
                Debug.LogError($"Family: {family.FamilyName} already exists");
                return;
            }
            
            _families.Add(family);
        }

        public Family FindOrCreateFamily(Unit unit)
        {
            var result = GetFamily(unit.GetUnitState());
            if (result != null)
            {
                return result;
            }
            
            var familyname = unit.GetUnitState().LastName;
            var members = new List<UnitState> { unit.GetUnitState() };
            var job = unit.GetUnitState().CurrentJob;

            Family family = new Family(familyname, members, job);
            _families.Add(family);
            return family;
        }
    }

    [Serializable]
    public class Family
    {
        public string FamilyName;
        public string UID;
        public List<UnitState> Members = new List<UnitState>();
        public List<Building> Buildings = new List<Building>();
        public List<Furniture> Furniture = new List<Furniture>();
        public JobData FamilyJob;
        public bool IsPlayer;

        public Family(string familyName, List<UnitState> members, JobData job, bool isPlayer = false)
        {
            FamilyName = familyName;
            Members = members;
            FamilyJob = job;
            IsPlayer = isPlayer;
            UID = $"{FamilyName}_{Guid.NewGuid()}";
        }

        public bool IsFamilyMember(UnitState unitState)
        {
            foreach (var member in Members)
            {
                if (member == unitState)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
