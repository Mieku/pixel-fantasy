using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Wall Settings", menuName = "Settings/Wall Settings")]
public class WallSettings : ConstructionSettings
{
    [SerializeField] private string _wallName;
    [SerializeField] private CraftRequirements _craftRequirements;

    public string WallName => _wallName;
    public Sprite OptionIcon;
    public int MaxDurability;
    public RuleTile ExteriorRuleTile;
    public RuleTile InteriorRuleTile;
    public CraftRequirements CraftRequirements => _craftRequirements.Clone();
        
    public List<string> GetStatsList()
    {
        // TODO: build me
        return new List<string>();
    }
}
