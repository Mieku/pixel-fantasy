using Interfaces;
using UnityEngine;

namespace ScriptableObjects
{
    [CreateAssetMenu(fileName = "DynamicFloorData", menuName = "DynamicData/DynamicFloorData", order = 1)]
    public class DynamicFloorSprites : ScriptableObject, IDynamicSprites
    {
        
        public Sprite GetSprite(WallNeighbourConnectionInfo neighbours)
        {
            throw new System.NotImplementedException();
        }
    }
}
