using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConstructionSettings : ScriptableObject
{
    [SerializeField] protected CraftRequirements _craftRequirements;
    
    public CraftRequirements CraftRequirements => _craftRequirements.Clone();
}
