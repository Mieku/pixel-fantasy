using UnityEngine;

namespace Systems.Card.Scripts
{
    [CreateAssetMenu(fileName = "KinlingCardData", menuName = "Cards/KinlingCardData", order = 1)]
    public class KinlingCardData : CardData
    {
        public override int CardCost { get; }
        public override string CardName { get; }
        public override string FlavourText { get; }
        public override CardContent[] CardContents { get; }
        public override Sprite CardArt { get; }
    }
}
