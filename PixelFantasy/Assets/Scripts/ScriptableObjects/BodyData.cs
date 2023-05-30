using UnityEngine;

namespace ScriptableObjects
{
    [CreateAssetMenu(fileName = "BodyData", menuName = "AppearanceData/BodyData", order = 1)]
    public class BodyData : ScriptableObject
    {
        public string Name;
        public Sprite HeadSide, HeadFront, OuterHand, BackHand, BodyNudeSide, BodyNudeFront, LeftLeg, LeftLegFront, RightLeg;
        public Sprite Portrait;
    }
}
