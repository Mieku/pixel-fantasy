using System;
using Interfaces;
using ScriptableObjects;
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
    }
}
