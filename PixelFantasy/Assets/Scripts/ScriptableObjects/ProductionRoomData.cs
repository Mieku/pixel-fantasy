using UnityEngine;

namespace ScriptableObjects
{
    [CreateAssetMenu(fileName = "ProductionRoomData", menuName = "Rooms/ProductionRoomData", order = 1)]
    public class ProductionRoomData : RoomData
    {
        public string WorkersTitle;
    }
}
