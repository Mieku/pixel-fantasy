using System;
using System.Collections.Generic;
using Characters;
using UnityEngine;

namespace Gods
{
    public class FamilyManager : God<FamilyManager>
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
    }

    [Serializable]
    public class Family
    {
        public string FamilyName;
        public string UID;
        public UnitState FamilyHead;
        public List<UnitState> Members = new List<UnitState>();
        public bool IsPlayer;

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
