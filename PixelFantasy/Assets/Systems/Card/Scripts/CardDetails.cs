using System;
using System.Collections.Generic;
using UnityEngine;

namespace Systems.Card.Scripts
{
    public class CardDetails : MonoBehaviour
    {
        [SerializeField] private CardDetailsExpandedContent _detailsExpandedPrefab;
        [SerializeField] private Transform _detailsParent;

        private List<CardDetailsExpandedContent> _displayedDetails = new List<CardDetailsExpandedContent>();
        private CardBase _curCard;

        private void Awake()
        {
            gameObject.SetActive(false);
        }

        public void Show(CardBase cardBase)
        {
            if (_curCard != null)
            {
                _curCard.CloseDetails();
            }

            _curCard = cardBase;
            
            ClearDisplayedDetails();

            var contents = cardBase.GetCardData().CardContents;
            foreach (var content in contents)
            {
                if (content.HasExpandedContent)
                {
                    var details = Instantiate(_detailsExpandedPrefab, _detailsParent);
                    details.Show(content);
                    _displayedDetails.Add(details);
                }
            }
        }

        public void Hide()
        {
            _curCard = null;
        }

        private void ClearDisplayedDetails()
        {
            foreach (var displayedDetail in _displayedDetails)
            {
                Destroy(displayedDetail.gameObject);
            }
            _displayedDetails.Clear();
        }
    }
}
