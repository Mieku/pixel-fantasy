using Buildings;
using Controllers;
using Managers;
using Sirenix.OdinInspector;
using Systems.Currency.Scripts;
using UnityEngine;
using UnityEngine.UI;

namespace Systems.Card.Scripts
{
    public class BuildingCard : CardBase
    {
        [TitleGroup("Building Card")] [SerializeField] private Image _cardArt;
        [TitleGroup("Building Card")] [SerializeField] private Transform _costsParent;
        [TitleGroup("Building Card")] [SerializeField] private CardBuildCostDisplay _costDisplayPrefab;

        private BuildingCardData _cardData;

        public void Init(BuildingCardData cardData)
        {
            _cardData = cardData;

            _cardCostDisplay.text = _cardData.CardCost.ToString();
            _flavourText.text = _cardData.FlavourText;
            _cardNameDisplay.text = _cardData.CardName;
            _cardArt.sprite = _cardData.CardArt;

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

        public override void TriggerCardPower()
        {
            if (_plannedBuilding.CheckPlacement() && CurrencyManager.Instance.RemoveCoins(GetCardData().CardCost))
            {
                _plannedBuilding.SetState(Building.BuildingState.Construction);
                RemoveCard();
            }
            else
            {
                CancelCard();
            }
        }

        protected override void ToggleCardPlanning(bool isEnabled)
        {
            base.ToggleCardPlanning(isEnabled);

            if (isEnabled)
            {
                PlanBuilding(_cardData.LinkedBuilding);
            }
            else
            {
                if (_plannedBuilding != null)
                {
                    Destroy(_plannedBuilding.gameObject);
                    _plannedBuilding = null;
                }
            }
        }

        private Building _plannedBuilding;
        private void PlanBuilding(Building building)
        {
            _plannedBuilding = Instantiate(building, CardsManager.Instance.BuildingsParent);
            _plannedBuilding.SetState(Building.BuildingState.Planning);
        }
    }
}
