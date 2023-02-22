using System;
using Actions;
using DataPersistence;
using UnityEngine;

namespace Characters
{
    public class Unit : UniqueObject, IPersistent
    {
        [SerializeField] private KinlingAgent _kinlingAgent;
        [SerializeField] private UnitState _unitState;
        [SerializeField] private UnitAppearance _appearance;
        
        public UnitState GetUnitState()
        {
            return _unitState;
        }

        public UnitTaskAI GetUnitTaskAI()
        {
            return null;//_unitTaskAI;
        }

        public ActionBase GetCurrentAction()
        {
            return null;//_unitTaskAI.currentAction;
        }

        public object CaptureState()
        {
            //var unitTaskData = _unitTaskAI.GetSaveData();
            var unitStateData = _unitState.GetStateData();
            var appearanceData = _appearance.GetSaveData();
            var agentData = _kinlingAgent.GetSaveData();

            return new UnitData
            {
                UID = UniqueId,
                Position = transform.position,
                //UnitTaskData = unitTaskData,
                UnitStateData = unitStateData,
                AppearanceData = appearanceData,
                KinlingAgentStateData = agentData,
            };
        }

        public void RestoreState(object data)
        {
            var unitData = (UnitData)data;

            UniqueId = unitData.UID;
            transform.position = unitData.Position;
            
            // Send the data to all components
            //_unitTaskAI.SetLoadData(unitData.UnitTaskData);
            _unitState.SetLoadData(unitData.UnitStateData);
            _appearance.SetLoadData(unitData.AppearanceData);
            _kinlingAgent.SetLoadData(unitData.KinlingAgentStateData);
        }

        public struct UnitData
        {
            public string UID;
            public Vector3 Position;

            // UnitTaskAI
            //public UnitTaskAI.UnitTaskData UnitTaskData;

            public KinlingAgent.AgentStateData KinlingAgentStateData;
            
            // Unit State
            public UnitState.UnitStateData UnitStateData;
            
            // Unit Appearance
            public UnitAppearance.AppearanceData AppearanceData;
        }
    }
}
