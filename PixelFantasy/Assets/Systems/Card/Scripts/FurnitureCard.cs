using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;

namespace Systems.Card.Scripts
{
    public class FurnitureCard : CardBase
    {
        [TitleGroup("Furniture Card")] [SerializeField] private Image _furnitureImage;
        [TitleGroup("Furniture Card")] [SerializeField] private Image _blueprint;
        [TitleGroup("Furniture Card")] [SerializeField] private Transform _costsParent;
        [TitleGroup("Furniture Card")] [SerializeField] private CardBuildCostDisplay _costDisplayPrefab;

        private FurnitureCardData _cardData;
        
        public void Init(FurnitureCardData cardData)
        {
            _cardData = cardData;
            // TODO: Build me!
        }

        public override CardData GetCardData()
        {
            return _cardData;
        }
    }
}
