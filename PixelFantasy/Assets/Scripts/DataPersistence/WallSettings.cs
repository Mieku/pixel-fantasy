using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Wall Settings", menuName = "Settings/Wall Settings")]
public class WallSettings : ConstructionSettings
{
    [SerializeField] private string _wallName;

    public string WallName => _wallName;
    public Sprite OptionIcon;
    public int MaxDurability;
    public RuleTile ExteriorRuleTile;
    public RuleTile InteriorRuleTile;
        
    public List<string> GetStatsList()
    {
        // TODO: build me
        return new List<string>();
    }
}
