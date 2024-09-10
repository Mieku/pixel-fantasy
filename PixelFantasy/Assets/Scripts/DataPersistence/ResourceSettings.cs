using System;
using System.Collections;
using System.Collections.Generic;
using DataPersistence;
using Items;
using Systems.Stats.Scripts;
using UnityEngine;
using Random = UnityEngine.Random;

[CreateAssetMenu(fileName = "Resource Settings", menuName = "Settings/Resource Settings")]
public class ResourceSettings : Settings
{
    // Settings
    [SerializeField] protected string _resourceName;
    [SerializeField] protected BasicResource _prefab;
    [SerializeField] protected HarvestableItems _harvestableItems;
    [SerializeField] protected List<Sprite> _potentialSprites = new List<Sprite>();
    [SerializeField] protected float _maxHealth;
    [SerializeField] protected float _minWorkDistance = 0.75f;
        
    // Type of skill to extract
    [SerializeField] protected ESkillType _extractionSkillType;
    [SerializeField] protected int _extractionMinSkillLevel;

        
    // Accessors
    public HarvestableItems HarvestableItems => _harvestableItems;
    public float MinWorkDistance => _minWorkDistance;
    public BasicResource Prefab => _prefab;
    public float MaxHealth => _maxHealth;
    public List<Sprite> PotentialSprites => _potentialSprites;
    public ESkillType ExtractionSkillType => _extractionSkillType;
    public int ExtractionMinSkillLevel => _extractionMinSkillLevel;
    public string ResourceName => _resourceName;
        
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
