using System;
using System.Collections.Generic;
using Interfaces;
using Items;
using ScriptableObjects;
using Sirenix.OdinInspector;
using UnityEngine;
using Object = UnityEngine.Object;

namespace HUD
{
    [Serializable]
    public class ConstructionOrder
    {
        public OrderType OrderType;
        public string OrderName;
        public Sprite Icon;
        public string DataKey;
        [ShowIf("OrderType", Value = HUD.OrderType.SubMenu)] public List<ConstructionOrder> SubMenu;
    }

    [Serializable]
    public class MassOrder
    {
        public Order MassOrderType;
        public string OrderName;
    }
    
    [Serializable]
    public enum OrderType
    {
        SpawnObject,
        BuildStructure,
        Zone,
        BuildFloor,
        ClearGrass,
        BuildFurniture,
        SubMenu,
        Menu,
    }
}
