using Sirenix.OdinInspector;
using UnityEngine;

namespace Systems.Card.Scripts
{
    public abstract class CardData : ScriptableObject
    {
        public abstract int CardCost { get; }
        public abstract string CardName { get; }
        public abstract string FlavourText { get; }
        public abstract CardContent[] CardContents { get; }
    }
}
