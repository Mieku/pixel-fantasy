using System;
using System.Collections.Generic;
using System.ComponentModel;
using Characters;
using Items;
using Managers;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Serialization;

namespace ScriptableObjects
{
    [CreateAssetMenu(fileName = "FurnitureItemData", menuName = "ItemData/CraftedItemData/FurnitureItemData/FurnitureItemData", order = 1)]
    public class FurnitureItemData : CraftedItemData
    {
        [Header("General")] 
        [TitleGroup("Furniture Item Data")] public List<FurnitureVarient> Varients;
        [TitleGroup("Furniture Item Data")] public Furniture FurniturePrefab;
        [TitleGroup("Furniture Item Data")] public NeedChange InUseNeedChange;
    }
    
    [Serializable]
    public class FurnitureVarient : MaterialVarient
    {
        public Furniture Prefab;
    }
}
