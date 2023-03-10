using UnityEngine;
using UnityEngine.Tilemaps;

namespace ScriptableObjects
{
    [CreateAssetMenu(fileName = "StructureData", menuName = "CraftedData/StructureData", order = 1)]
    public class StructureData : ConstructionData
    {
        [SerializeField] private RuleOverrideTile _wallRuleTile;

        public RuleOverrideTile RuleTile => _wallRuleTile;
    }
}
