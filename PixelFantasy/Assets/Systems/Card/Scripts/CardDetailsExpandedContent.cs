using TMPro;
using UnityEngine;

namespace Systems.Card.Scripts
{
    public class CardDetailsExpandedContent : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _cardText;
        [SerializeField] private TextMeshProUGUI _expandedCardText;

        public void Show(CardContent cardContent)
        {
            _cardText.text = cardContent.ContentOnCard;
            _expandedCardText.text = cardContent.ExpandedContent;
        }
    }
}
