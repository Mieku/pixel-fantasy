using UnityEngine;

namespace ScriptableObjects
{
    [CreateAssetMenu(fileName = "RoofData", menuName = "CraftedData/RoofData", order = 1)]
    public class RoofData : ConstructionData
    {
        public RuleTile RuleTile;
    }
}
