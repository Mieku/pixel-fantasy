using System.Collections.Generic;
using Databrain;
using Databrain.Attributes;
using Items;
using TaskSystem;
using UnityEngine;

namespace Data.Resource
{
    [DataObjectAddToRuntimeLibrary]
    public class ResourceData : DataObject
    {
        // Settings
        [SerializeField] protected BasicResource _prefab;
        [SerializeField] protected int _workToExtract;
        [SerializeField] protected HarvestableItems _harvestableItems;
        [SerializeField] protected List<Sprite> _potentialSprites = new List<Sprite>();
        [SerializeField] protected float _maxHealth;
        [SerializeField] protected float _minWorkDistance = 0.75f;
        [SerializeField] protected List<string> _invalidPlacementTags = new List<string>() { "Water", "Wall", "Obstacle", "Nature", "Structure"};

        // Accessors
        public int WorkToExtract => _workToExtract;
        public HarvestableItems HarvestableItems => _harvestableItems;
        public List<string> InvalidPlacementTags => _invalidPlacementTags;
        public float MinWorkDistance => _minWorkDistance;
        public BasicResource Prefab => _prefab;

        // Runtime
        [Foldout("Runtime"), ExposeToInspector, DatabrainSerialize] public int SpriteIndex;
        [Foldout("Runtime"), ExposeToInspector, DatabrainSerialize] public float Health;
        [Foldout("Runtime"), ExposeToInspector, DatabrainSerialize] public float RemainingExtractWork;
        [Foldout("Runtime"), ExposeToInspector, DatabrainSerialize] public Task CurrentTask;
        [Foldout("Runtime"), ExposeToInspector, DatabrainSerialize] public Vector2 Position;


        public virtual void InitData()
        {
            Health = _maxHealth;
            RemainingExtractWork = _workToExtract;
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
