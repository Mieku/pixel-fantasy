using UnityEngine;

namespace Characters
{
    public class UnitState : MonoBehaviour
    {
        public string FirstName, LastName;


        public string FullName => FirstName + " " + LastName;

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
