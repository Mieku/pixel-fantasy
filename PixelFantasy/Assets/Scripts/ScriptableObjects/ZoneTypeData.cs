using UnityEngine;

namespace ScriptableObjects
{
    [CreateAssetMenu(fileName = "ZoneTypeData", menuName = "ZoneTypeData", order = 1)]
    public class ZoneTypeData : ScriptableObject
    {
        public string ZoneTypeName;
        public ZoneType ZoneType;
        public Sprite Icon;
        public Color Colour;
    }
    
    public enum ZoneType
    {
        Storage,
        Farm,
        Home,
    }
}
