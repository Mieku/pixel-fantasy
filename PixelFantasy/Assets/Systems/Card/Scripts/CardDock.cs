using System.Collections.Generic;
using CodeMonkey.Utils;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Systems.Card.Scripts
{
    public class CardDock : MonoBehaviour
    {
        [SerializeField] private CardSlot _cardSlotPrefab;
        [SerializeField] private Transform _cardSlotParent;
        [SerializeField] private List<CardData> _cardDatas = new List<CardData>();

        private List<CardSlot> _displayedCardSlots = new List<CardSlot>();
        private const float DISPLAY_OFFSET_TIME = 0.3f;

        public void RemoveCard(CardBase card)
        {
            foreach (var slot in _displayedCardSlots)
            {
                if (slot.LinkedCard == card)
                {
                    _displayedCardSlots.Remove(slot);
                    Destroy(slot.gameObject);
                    return;
                }
            }
        }
        
        public bool IsCardInDockArea(CardBase card)
        {
            var topMostY = transform.position.y;
            var cardY = card.HoldPosition.y;//card.gameObject.transform.position.y;

            return cardY <= topMostY;
        }

        [Button("DEBUG Display Cards")]
        private void DisplayCards()
        {
            ClearDisplayedSlots();
            int offset = 0;
            
            foreach (var cardData in _cardDatas)
            {
                var slot = Instantiate(_cardSlotPrefab, _cardSlotParent);
                slot.CreateCard(cardData, DISPLAY_OFFSET_TIME * offset);
                _displayedCardSlots.Add(slot);
                offset++;
            }
        }
        
        [Button("DEBUG Clear Cards")]
        private void ClearDisplayedSlots()
        {
            foreach (var cardSlot in _displayedCardSlots)
            {
                Destroy(cardSlot.gameObject);
            }
            _displayedCardSlots.Clear();
        }
    }
}
