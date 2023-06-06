using UnityEngine;

namespace ScriptableObjects
{
    [CreateAssetMenu(fileName = "DoorData", menuName = "CraftedData/DoorData", order = 1)]
    public class DoorData : ConstructionData
    {
        public GameObject HorizontalDoor;
        public GameObject VerticalDoorLeftEdge;
        public GameObject VerticalDoorRightEdge;
    }
}
