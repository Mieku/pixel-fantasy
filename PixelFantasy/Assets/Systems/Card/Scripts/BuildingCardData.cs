using Buildings;
using ScriptableObjects;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Systems.Card.Scripts
{
    [CreateAssetMenu(fileName = "BuildingCardData", menuName = "Cards/BuildingCardData", order = 1)]
    public class BuildingCardData : CardData
    {
        [TitleGroup("Building Card")] [SerializeField] private Building _building;
        [TitleGroup("Building Card")] [SerializeField] private int _cardCost;
        [TitleGroup("Building Card")] [SerializeField] private CardContent[] _cardContent;

        public override int CardCost => _cardCost;
        public override string CardName => _building.BuildingName;
        public Building LinkedBuilding => _building;
        

        public override string FlavourText
        {
            get
            {
                string requiredJobName = _building.BuildingData.RequiredConstructorJob.JobName;
                return $"Requires a {requiredJobName} to construct";
            }
        }

        public override CardContent[] CardContents => _cardContent;
    }
}
