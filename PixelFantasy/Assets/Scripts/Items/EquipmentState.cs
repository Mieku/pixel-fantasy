using System;
using Characters;
using ScriptableObjects;
using UnityEngine;

namespace Items
{
    [Serializable]
    public class EquipmentState : ItemState
    {
        public Unit Owner;
        public EquipmentData EquipmentData => Data as EquipmentData;
        
        public EquipmentState(EquipmentData data, string uid) : base(data, uid)
        {
            Data = data;
            UID = uid;
        }
    }
}
