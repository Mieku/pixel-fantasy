using Buildings;
using UnityEngine;

namespace Characters
{
    public class UnitState : MonoBehaviour
    {
        [SerializeField] private UID _uid;
        
        public string FirstName, LastName;
        
        // Kinling Attributes
        public float SpeedModifier = 0f;
        public float ProductivityModifier = 0f;
        public float HealingModifier = 0f;
        public float AimModifier = 0f;
        public float ToughnessModifier = 0f;
        public float CombatModifier = 0f;

        public Building Home;
        public Building Occupation;

        public string FullName => FirstName + " " + LastName;
        public string UID => _uid.uniqueID;

        private const float _defaultSpeed = 6f;
        private const float _defaultProductivity = 1f;
        private const float _defaultHealing = 1f;
        private const float _defaultAim = 1f;
        private const float _defaultToughness = 1f;
        private const float _defaultCombat = 1f;

        public float Speed => _defaultSpeed + (_defaultSpeed * SpeedModifier);
        public float Productivity => _defaultProductivity + (_defaultProductivity * ProductivityModifier);
        public float Healing => _defaultHealing + (_defaultHealing * HealingModifier);
        public float Aim => _defaultAim + (_defaultAim * AimModifier);
        public float Toughness => _defaultToughness + (_defaultToughness * ToughnessModifier);
        public float Combat => _defaultCombat + (_defaultCombat * CombatModifier);

        public void RemoveOccupation()
        {
            Occupation = null;
        }

        public void SetLoadData(UnitStateData data)
        {
            FirstName = data.FirstName;
            LastName = data.LastName;

            SpeedModifier = data.SpeedModifier;
            ProductivityModifier = data.ProductivityModifier;
            HealingModifier = data.HealingModifier;
            AimModifier = data.AimModifier;
            ToughnessModifier = data.ToughnessModifier;
            CombatModifier = data.CombatModifier;
        }

        public UnitStateData GetStateData()
        {
            return new UnitStateData
            {
                FirstName = FirstName,
                LastName = LastName,
                SpeedModifier = SpeedModifier,
                ProductivityModifier = ProductivityModifier,
                HealingModifier = HealingModifier,
                AimModifier = AimModifier,
                ToughnessModifier = ToughnessModifier,
                CombatModifier = CombatModifier,
            };
        }
        
        public struct UnitStateData
        {
            public string FirstName, LastName;

            public float SpeedModifier;
            public float ProductivityModifier;
            public float HealingModifier;
            public float AimModifier;
            public float ToughnessModifier;
            public float CombatModifier;
        }
    }
}
