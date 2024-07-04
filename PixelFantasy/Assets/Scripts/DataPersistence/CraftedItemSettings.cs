using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Crafted Item Settings", menuName = "Settings/Crafted Item Settings")]
public class CraftedItemSettings : ItemSettings
{
    // Crafted Item Settings
    [SerializeField] protected CraftRequirements _craftRequirements;

    // Accessors
    public CraftRequirements CraftRequirements => _craftRequirements.Clone();

    public ItemSettings GetSettings => this;
    
    public override ItemData CreateItemData()
    {
        var data = new CraftedItemData();
        data.InitData(this);
        return data;
    }
}
