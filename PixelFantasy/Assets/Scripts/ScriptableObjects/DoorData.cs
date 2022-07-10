using UnityEngine;

namespace ScriptableObjects
{
    [CreateAssetMenu(fileName = "DoorData", menuName = "CraftedData/DoorData", order = 1)]
    public class DoorData : ConstructionData
    {
        public Sprite HorizontalSprite;
        public Sprite VerticalSprite;
    }
}
