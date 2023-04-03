using System;
using DataPersistence;
using TaskSystem;
using UnityEngine;
using UnityEngine.AI;

namespace Characters
{
    public class Unit : UniqueObject, IPersistent
    {
        [SerializeField] private TaskAI _taskAI;
        [SerializeField] private UnitState _unitState;
        [SerializeField] private UnitAppearance _appearance;
        [SerializeField] private UnitInventory _inventory;

        public UnitAnimController UnitAnimController;
        public UnitAgent UnitAgent;

        public UnitState GetUnitState()
        {
            return _unitState;
        }
        
        public object CaptureState()
        {
            var unitStateData = _unitState.GetStateData();
            var appearanceData = _appearance.GetSaveData();

            return new UnitData
            {
                UID = UniqueId,
                Position = transform.position,
                UnitStateData = unitStateData,
                AppearanceData = appearanceData,
            };
        }

        public void RestoreState(object data)
        {
            var unitData = (UnitData)data;

            UniqueId = unitData.UID;
            transform.position = unitData.Position;
            
            // Send the data to all components
            _unitState.SetLoadData(unitData.UnitStateData);
            _appearance.SetLoadData(unitData.AppearanceData);
        }

        public struct UnitData
        {
            public string UID;
            public Vector3 Position;

            
            // Unit State
            public UnitState.UnitStateData UnitStateData;
            
            // Unit Appearance
            public UnitAppearance.AppearanceData AppearanceData;
        }
    }
}
