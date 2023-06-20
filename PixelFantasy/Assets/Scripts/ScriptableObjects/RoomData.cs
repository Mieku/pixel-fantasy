using System.Collections.Generic;
using UnityEngine;

namespace ScriptableObjects
{
    [CreateAssetMenu(fileName = "RoomData", menuName = "Rooms/RoomData", order = 1)]
    public class RoomData : ScriptableObject
    {
        public string RoomName;
        public int MaxOccupants;
        public List<FurnitureItemData> BuildFurnitureOptions = new List<FurnitureItemData>();
    }
}
