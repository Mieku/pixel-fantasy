using Actions;
using DataPersistence;
using UnityEngine;

namespace Characters
{
    public class Unit : UniqueObject, IPersistent
    {
        [SerializeField] private UnitTaskAI _unitTaskAI;
        [SerializeField] private UnitState _unitState;

        public UnitState GetUnitState()
        {
            return _unitState;
        }

        public ActionBase GetCurrentAction()
        {
            return _unitTaskAI.currentAction;
        }

        public object CaptureState()
        {
            var unitTaskData = _unitTaskAI.GetSaveData();
            var unitStateData = _unitState.GetStateData();

            return new UnitData
            {
                UID = UniqueId,
                Position = transform.position,
                UnitTaskData = unitTaskData,
                UnitStateData = unitStateData,
            };
        }

        public void RestoreState(object data)
        {
            var unitData = (UnitData)data;

            UniqueId = unitData.UID;
            transform.position = unitData.Position;
            
            // Send the data to all components
            _unitTaskAI.SetLoadData(unitData.UnitTaskData);
            _unitState.SetLoadData(unitData.UnitStateData);
        }

        public struct UnitData
        {
            public string UID;
            public Vector3 Position;

            // UnitTaskAI
            public UnitTaskAI.UnitTaskData UnitTaskData;
            
            // Unit State
            public UnitState.UnitStateData UnitStateData;
        }
    }
}
