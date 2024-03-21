using System.Collections.Generic;
using Databrain;
using Databrain.Attributes;
using Items;
using Managers;
using UnityEngine;

namespace Data.Resource
{
    public class ResourceSettings : DataObject
    {
        // Settings
        [SerializeField] protected BasicResource _prefab;
        [SerializeField] protected int _workToExtract;
        [SerializeField] protected HarvestableItems _harvestableItems;
        [SerializeField] protected List<Sprite> _potentialSprites = new List<Sprite>();
        [SerializeField] protected float _maxHealth;
        [SerializeField] protected float _minWorkDistance = 0.75f;
        [DataObjectDropdown(true), SerializeField] private ResourceData _baseData;
        
        // Accessors
        public int WorkToExtract => _workToExtract;
        public HarvestableItems HarvestableItems => _harvestableItems;
        public float MinWorkDistance => _minWorkDistance;
        public BasicResource Prefab => _prefab;
        public float MaxHealth => _maxHealth;
        public List<Sprite> PotentialSprites => _potentialSprites;
        
        public ResourceData CreateInitialDataObject()
        {
            var dataLibrary = Librarian.Instance.DataLibrary;
            var runtimeData = (ResourceData)dataLibrary.CloneDataObjectToRuntime(_baseData);
            return runtimeData;
        }
        
        public int GetRandomSpriteIndex()
        {
            int rand = Random.Range(0, _potentialSprites.Count - 1);
            return rand;
        }
        
        public Sprite GetSprite(int spriteIndex)
        {
            return _potentialSprites[spriteIndex];
        }
    }
}
