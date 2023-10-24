using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Systems.Card.Scripts
{
    public class KinlingCard : CardBase
    {
        [TitleGroup("Kinling Card")] [SerializeField] private TextMeshProUGUI _jobName;
        [TitleGroup("Kinling Card")] [SerializeField] private Image _jobIcon;
        [TitleGroup("Kinling Card")] [SerializeField] private TextMeshProUGUI _taxDisplay;

        private KinlingCardData _cardData;
        
        public void Init(KinlingCardData cardData)
        {
            _cardData = cardData;
            // TODO: Build me!
        }

        public override CardData GetCardData()
        {
            return _cardData;
        }
        
        public override void TriggerCardPower()
        {
            Debug.Log("Card Power Triggered!!!!!");
        }
    }
}
