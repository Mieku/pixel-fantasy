using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;

namespace Systems.Card.Scripts
{
    public class BuildingCard : CardBase
    {
        [TitleGroup("Building Card")] [SerializeField] private Image _buildingImage;
        [TitleGroup("Building Card")] [SerializeField] private Image _blueprint;
        [TitleGroup("Building Card")] [SerializeField] private Transform _costsParent;
        [TitleGroup("Building Card")] [SerializeField] private CardBuildCostDisplay _costDisplayPrefab;

        private BuildingCardData _cardData;

        public void Init(BuildingCardData cardData)
        {
            _cardData = cardData;

            _cardCostDisplay.text = _cardData.CardCost.ToString();
            _flavourText.text = _cardData.FlavourText;
            _cardNameDisplay.text = _cardData.CardName;

            var buildCosts = _cardData.LinkedBuilding.BuildingData.GetResourceCosts();
            foreach (var buildCost in buildCosts)
            {
                var costDisplay = Instantiate(_costDisplayPrefab, _costsParent);
                costDisplay.Init(buildCost);
            }
            
            DisplayCardContent(_cardData.CardContents);
        }

        public override CardData GetCardData()
        {
            return _cardData;
        }
    }
}
