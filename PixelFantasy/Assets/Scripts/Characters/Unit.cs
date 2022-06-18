using System.Collections;
using System.Collections.Generic;
using Characters;
using DataPersistence;
using UnityEngine;

namespace Characters
{
    public class Unit : UniqueObject, IPersistent
    {
        [SerializeField] private UnitThought _unitThought;
        [SerializeField] private UnitAnimController _unitAnimController;
        [SerializeField] private UnitTaskAI _unitTaskAI;
        [SerializeField] private MovePositionAStarPathfinding _movePositionAStarPathfinding;
        
        public object CaptureState()
        {
            // Capture the data from all the Units components
            //var unitThoughtData = _unitThought.GetSaveData();
            //var unitAnimData = _unitAnimController.GetSaveData();
            var unitTaskData = _unitTaskAI.GetSaveData();
            //var movePosAStarData = _movePositionAStarPathfinding.GetSaveData();

            return new UnitData
            {
                UID = UniqueId,
                Position = transform.position,
                //UnitThoughtData = unitThoughtData,
                //UnitAnimData = unitAnimData,
                UnitTaskData = unitTaskData,
                
                //MovePosAStarData = movePosAStarData,
            };
        }

        public void RestoreState(object data)
        {
            var unitState = (UnitData)data;

            UniqueId = unitState.UID;
            transform.position = unitState.Position;
            
            // Send the data to all components
            //_unitThought.SetLoadData(unitState.UnitThoughtData);
            //_unitAnimController.SetLoadData(unitState.UnitAnimData);
            _unitTaskAI.SetLoadData(unitState.UnitTaskData);
            
            //_movePositionAStarPathfinding.SetLoadData(unitState.MovePosAStarData);

            // Trigger the components to initialize

        }

        public struct UnitData
        {
            public string UID;
            public Vector3 Position;
            
            //public UnitThought.UnitThoughtData UnitThoughtData;

            // UnitAnimController
            //public UnitAnimController.UnitAnimData UnitAnimData;

            // UnitTaskAI
            public UnitTaskAI.UnitTaskData UnitTaskData;

            // MovePositionAStarPathfinding
            //public MovePositionAStarPathfinding.MovePosAStarData MovePosAStarData;

        }
    }
}
