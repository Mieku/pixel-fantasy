using System;
using TMPro;
using UnityEngine;

namespace Systems.Card.Scripts
{
    [RequireComponent(typeof(TextMeshProUGUI))]
    public class CardContentDisplay : MonoBehaviour
    {
        [SerializeField] private Color _positiveColour;
        [SerializeField] private Color _negativeColour;
        
        private TextMeshProUGUI _contentText;

        private void Awake()
        {
            _contentText = GetComponent<TextMeshProUGUI>();
        }

        public void Init(CardContent content)
        {
            switch (content.ContentType)
            {
                case CardContent.EContentType.Normal:
                    _contentText.color = Color.white;
                    break;
                case CardContent.EContentType.Positive:
                    _contentText.color = _positiveColour;
                    break;
                case CardContent.EContentType.Negative:
                    _contentText.color = _negativeColour;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            _contentText.text = content.ContentOnCard;
        }
    }
}
