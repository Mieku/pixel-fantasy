using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Systems.Card.Scripts
{
    public class CardSlot : MonoBehaviour
    {
        [SerializeField] private KinlingCard _kinlingCardPrefab;
        [SerializeField] private BuildingCard _buildingCardPrefab;
        [SerializeField] private FurnitureCard _furnitureCardPrefab;
        [SerializeField] private Transform _cardHandle;
        [SerializeField] private Animator _anim;
        
        private CardBase _linkedCard;
        private Canvas _canvas;
        
        private static readonly int Appear = Animator.StringToHash("Appear");
        private static readonly int IsHovering = Animator.StringToHash("IsHovering");

        private const float DOCKED_CARD_SCALE = 0.9f;
        private const float HOVERED_CARD_SCALE = 1.2f;
        private const float CLICKED_CARD_SCALE = 1.3f;

        private void Awake()
        {
            _canvas = GetComponent<Canvas>();
        }

        public void StopAnimation()
        {
            _anim.enabled = false;
            _cardHandle.gameObject.GetComponent<RectTransform>().localScale = Vector3.one;
        }

        public void CreateCard(CardData cardData, float waitOffset)
        {
            // Determine the type of card
            var kinlingCardData = cardData as KinlingCardData;
            if (kinlingCardData != null)
            {
                var card = Instantiate(_kinlingCardPrefab, _cardHandle);
                card.GetComponent<RectTransform>().localScale = new Vector3(DOCKED_CARD_SCALE, DOCKED_CARD_SCALE, 1);
                card.Init(kinlingCardData);
                _linkedCard = card;
                _linkedCard.AssignSlot(this);
                AnimateCardAppear(waitOffset);
                return;
            }
            
            var furnitureCardData = cardData as FurnitureCardData;
            if (furnitureCardData != null)
            {
                var card = Instantiate(_furnitureCardPrefab, _cardHandle);
                card.GetComponent<RectTransform>().localScale = new Vector3(DOCKED_CARD_SCALE, DOCKED_CARD_SCALE, 1);
                card.Init(furnitureCardData);
                _linkedCard = card;
                _linkedCard.AssignSlot(this);
                AnimateCardAppear(waitOffset);
                return;
            }
            
            var buildingCardData = cardData as BuildingCardData;
            if (buildingCardData != null)
            {
                var card = Instantiate(_buildingCardPrefab, _cardHandle);
                card.GetComponent<RectTransform>().localScale = new Vector3(DOCKED_CARD_SCALE, DOCKED_CARD_SCALE, 1);
                card.Init(buildingCardData);
                _linkedCard = card;
                _linkedCard.AssignSlot(this);
                AnimateCardAppear(waitOffset);
                return;
            }
        }

        public void AnimateCardAppear(float offsetTime)
        {
            _anim.enabled = true;
            StartCoroutine(CardAppearSequence(offsetTime));
        }

        IEnumerator CardAppearSequence(float waitTime)
        {
            yield return new WaitForSeconds(waitTime);
            
            _anim.SetTrigger(Appear);
        }

        public void ToggleHover(bool isHovering)
        {
            _anim.enabled = true;
            
            //return;
            _anim.SetBool(IsHovering, isHovering);

            if (isHovering)
            {
                _canvas.sortingOrder = 2;
            }
            else
            {
                _canvas.sortingOrder = 1;
            }
        }
    }
}
