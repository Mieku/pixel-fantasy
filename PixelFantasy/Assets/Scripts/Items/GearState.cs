using System;
using Characters;
using ScriptableObjects;
using UnityEngine;

namespace Items
{
    [Serializable]
    public class GearState : ItemState
    {
        public Unit Owner;
        public GearData GearData => Data as GearData;
        public DyePaletteData AssignedDye;
        
        public GearState(GearData data, string uid) : base(data, uid)
        {
            Data = data;
            UID = uid;
        }
        
        public GearState(GearState other) : base(other)
        {
            Owner = other.Owner;
            AssignedDye = other.AssignedDye;
        }

        public bool CanBeEquippedByUnit(Unit unit)
        {
            if (Durability <= 0) return false;
            if (GearData.RequiredJob != null)
            {
                var unitJobData = unit.GetUnitState().CurrentJob.JobData;
                if (GearData.RequiredJob == unitJobData)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }

            return true;
        }
    }
}
