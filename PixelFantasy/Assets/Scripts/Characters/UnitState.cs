using UnityEngine;

namespace Characters
{
    public class UnitState : MonoBehaviour
    {
        public string FirstName, LastName;
        
        // Kinling Attributes
        public float SpeedModifier = 0f;
        public float ProductivityModifier = 0f;
        public float HealingModifier = 0f;
        public float AimModifier = 0f;
        public float ToughnessModifier = 0f;
        public float CombatModifier = 0f;

        public string FullName => FirstName + " " + LastName;

        private float _defaultSpeed = 6f;
        private float _defaultProductivity = 1f;
        private float _defaultHealing = 1f;
        private float _defaultAim = 1f;
        private float _defaultToughness = 1f;
        private float _defaultCombat = 1f;

        public float Speed => _defaultSpeed + (_defaultSpeed * SpeedModifier);
        public float Productivity => _defaultProductivity + (_defaultProductivity * ProductivityModifier);
        public float Healing => _defaultHealing + (_defaultHealing * HealingModifier);
        public float Aim => _defaultAim + (_defaultAim * AimModifier);
        public float Toughness => _defaultToughness + (_defaultToughness * ToughnessModifier);
        public float Combat => _defaultCombat + (_defaultCombat * CombatModifier);

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
