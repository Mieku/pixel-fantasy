using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace HUD
{
    [Serializable]
    public class Order
    {
        public OrderType OrderType;
        public string OrderName;
        public Sprite Icon;
        public string DataKey;
        [ShowIf("OrderType", Value = HUD.OrderType.SubMenu)] public List<Order> SubMenu;
    }

    [Serializable]
    public class MassOrder
    {
        //public ActionBase MassOrderType;
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
        BuildDoor,
    }
}
