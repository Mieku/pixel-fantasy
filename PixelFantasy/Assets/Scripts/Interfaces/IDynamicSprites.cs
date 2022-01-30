using ScriptableObjects;
using UnityEngine;

namespace Interfaces
{
    public interface IDynamicSprites
    {
        public Sprite GetSprite(WallNeighbourConnectionInfo neighbours);
    }
}
