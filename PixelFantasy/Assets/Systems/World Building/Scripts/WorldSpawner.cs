using Items;
using ScriptableObjects;
using UnityEngine;

namespace Systems.World_Building.Scripts
{
    public class WorldSpawner : MonoBehaviour
    {
        [SerializeField] private BoxCollider2D _boxCollider;

        private BiomeData _biome;
        private Transform _resourcesParent;

        public void Init(BiomeData biome, Transform resourcesParent)
        {
            _biome = biome;
            _resourcesParent = resourcesParent;
        }

        
    }
}
