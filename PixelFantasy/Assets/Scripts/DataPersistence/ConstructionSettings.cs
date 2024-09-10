using System.Collections;
using System.Collections.Generic;
using DataPersistence;
using UnityEngine;

public class ConstructionSettings : Settings
{
    [SerializeField] protected string _constructionName;
    [SerializeField] protected CraftRequirements _craftRequirements;
    
    public CraftRequirements CraftRequirements => _craftRequirements.Clone();
    public string ConstructionName => _constructionName;
}
