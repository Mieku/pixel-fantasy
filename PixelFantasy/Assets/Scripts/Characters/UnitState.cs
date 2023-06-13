using Buildings;
using ScriptableObjects;
using TaskSystem;
using UnityEngine;
using Zones;

namespace Characters
{
    public class UnitState : MonoBehaviour
    {
        [SerializeField] private UID _uid;
        
        public string FirstName, LastName;
        public Abilities Abilities;
        public TaskPriorities Priorities;
        
        public RoomZone AssignedWorkRoom;
        public Schedule Schedule = new Schedule();

        public string FullName => FirstName + " " + LastName;
        public string UID => _uid.uniqueID;
        
        public void SetLoadData(UnitStateData data)
        {
            FirstName = data.FirstName;
            LastName = data.LastName;
        }

        public UnitStateData GetStateData()
        {
            return new UnitStateData
            {
                FirstName = FirstName,
                LastName = LastName,
            };
        }
        
        public struct UnitStateData
        {
            public string FirstName, LastName;
        }
    }
}
