using Managers;
using UnityEngine;
using UnityEngine.UI;

namespace Systems.Card.Scripts
{
    public class CardsManager : Singleton<CardsManager>
    {
        [SerializeField] private Canvas _cardCanvas;
        [SerializeField] private Transform _cardDetailsPos;
        [SerializeField] private CardDetails _cardDetails;
        [SerializeField] private CardDock _dock;

        [SerializeField] private Transform _buildingsParent;
        
        public Canvas CardCanvas => _cardCanvas;
        public Transform CardDetailsPosition => _cardDetailsPos;

        public Transform BuildingsParent => _buildingsParent;

        private CardBase _curCard;

        public void RemoveCard(CardBase card)
        {
            _dock.RemoveCard(card);
        }
        
        public void AssignCardSelected(CardBase card)
        {
            _curCard = card;
        }

        public bool HasACardSelected()
        {
            return _curCard != null;
        }

        public bool IsCardInDockArea(CardBase card)
        {
            return _dock.IsCardInDockArea(card);
        }

        public void ShowCardDetails(CardBase cardBase)
        {
            _cardDetails.gameObject.SetActive(true);
            _cardDetails.Show(cardBase);
        }

        public void HideCardDetails()
        {
            _cardDetails.gameObject.SetActive(false);
            _cardDetails.Hide();
        }
    }
}
