using System;
using System.Collections;
using Controllers;
using Sirenix.OdinInspector;
using Systems.Currency.Scripts;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Systems.Card.Scripts
{
    public abstract class CardBase : MonoBehaviour
    {
        public enum ECardState
        {
            Docked,
            BeingDragged,
            ShowingDetails,
        }

        [TitleGroup("General")] [SerializeField] private GameObject _outlineObj;
        [TitleGroup("General")] [SerializeField] protected TextMeshProUGUI _cardNameDisplay;
        [TitleGroup("General")] [SerializeField] protected TextMeshProUGUI _cardCostDisplay;
        [TitleGroup("General")] [SerializeField] protected Image _cardFrameImage;
        [TitleGroup("General")] [SerializeField] protected TextMeshProUGUI _flavourText;
        [TitleGroup("General")] [SerializeField] protected CardContentDisplay _cardContentPrefab;
        [TitleGroup("General")] [SerializeField] protected Transform _cardContentParent;

        // Dragging Based Variables
        private Canvas _canvas;
        private RectTransform _rectTransform;
        private Vector2 _velocity = Vector2.zero;
        private Vector2 _smoothedVelocity = Vector2.zero; // Added for damping
        private const float TILT_AMOUNT_STRENGTH = 0.05f;
        private const float TILT_AMOUNT = 45f;

        public ECardState _cardState;
        private CardSlot _cardSlot;
        private bool _inDockArea;
        private CanvasGroup _canvasGroup;
        private Coroutine _fadeCoroutine;
        private Color _defaultCostColour;
        private bool _canAfford;
        
        private const float DOCKED_CARD_SCALE = 0.9f;
        private const float CLICKED_CARD_SCALE = 1.3f;
        private const float FADE_DURATION = 0.5f;

        public abstract CardData GetCardData();
        public abstract void TriggerCardPower();

        private void Awake()
        {
            _canvasGroup = GetComponent<CanvasGroup>();
            _defaultCostColour = _cardCostDisplay.color;

            GameEvents.OnGlimraTotalChanged += GameEvent_OnGlimraTotalChanged;
            GameEvents.OnRightClickUp += GameEvents_OnRightClickUp;
        }

        private void OnDestroy()
        {
            GameEvents.OnGlimraTotalChanged -= GameEvent_OnGlimraTotalChanged;
            GameEvents.OnRightClickUp -= GameEvents_OnRightClickUp;
        }
        
        protected void GameEvents_OnRightClickUp(Vector3 mousePos, PlayerInputState inputState, bool isOverUI) 
        {
            CancelCard();
        }

        protected void CancelCard()
        {
            if(_cardState == ECardState.Docked) return;
            
            //SnapToDock();
            ToggleCardPlanning(false);
            _cardState = ECardState.Docked;
            CloseDetails();
            CardsManager.Instance.HideCardDetails();
            _outlineObj.SetActive(false);
        }

        protected void RemoveCard()
        {
            CardsManager.Instance.RemoveCard(this);
            Destroy(gameObject);
        }

        private void GameEvent_OnGlimraTotalChanged(int totalGlimra)
        {
            CheckIfAffordCard(totalGlimra);
        }

        private void CheckIfAffordCard(int curGlimra)
        {
            if (curGlimra >= GetCardData().CardCost)
            {
                _canAfford = true;
                _cardCostDisplay.color = _defaultCostColour;
            }
            else
            {
                _canAfford = false;
                _cardCostDisplay.color = Color.red;
            }
        }

        private void Start()
        {
            _cardState = ECardState.Docked;
            
            _canvas = CardsManager.Instance.CardCanvas;
            _rectTransform = GetComponent<RectTransform>();
            _outlineObj.SetActive(false);
            _cardContentPrefab.gameObject.SetActive(false);
            
            CheckIfAffordCard(CurrencyManager.Instance.TotalGlimra);
        }

        public void AssignSlot(CardSlot cardSlot)
        {
            _cardSlot = cardSlot;
            _inDockArea = true;
        }
        
        private void SnapToDock()
        {
            transform.SetParent(_cardSlot.CardHandle.transform);
            transform.localPosition = Vector3.zero;
            
            _cardState = ECardState.Docked;
            _cardSlot.ToggleHover(false);
            CardsManager.Instance.AssignCardSelected(null);
        }

        protected void DisplayCardContent(CardContent[] content)
        {
            foreach (var cardContent in content)
            {
                var displayedContent = Instantiate(_cardContentPrefab, _cardContentParent);
                displayedContent.Init(cardContent);
                displayedContent.gameObject.SetActive(true);
            }
        }

        protected void ShowCardDetails()
        {
            _cardState = ECardState.ShowingDetails;
            
            _cardSlot.StopAnimation();

            var pos = CardsManager.Instance.CardDetailsPosition.position;
            transform.position = pos;
            
            GetComponent<RectTransform>().localScale = new Vector3(CLICKED_CARD_SCALE, CLICKED_CARD_SCALE, 1);
            
            CardsManager.Instance.ShowCardDetails(this);
        }

        protected void HideCardDetails()
        {
            GetComponent<RectTransform>().localScale = new Vector3(DOCKED_CARD_SCALE, DOCKED_CARD_SCALE, 1);
        }
        
        void Update()
        {
            DragUpdate();
            CheckOverInDockArea();
        }

        private void CheckOverInDockArea()
        {
            if(_cardState == ECardState.ShowingDetails) return;

            if (CardsManager.Instance.IsCardInDockArea(this))
            {
                if(!_inDockArea)
                {
                    _inDockArea = true;
                    EnteredDockArea();
                }
            }
            else
            {
                if (_inDockArea)
                {
                    _inDockArea = false;
                    ExitedDockArea();
                }
            }
        }
        
        
        private void EnteredDockArea()
        {
            ToggleCardPlanning(false);
        }

        private void ExitedDockArea()
        {
            if (_canAfford)
            {
                ToggleCardPlanning(true);
            }
        }

        protected virtual void ToggleCardPlanning(bool isEnabled)
        {
            if (isEnabled)
            {
                StartFadeCoroutine(FadeCard());
            }
            else
            {
                StartFadeCoroutine(UnFadeCard());
            }
        }

        private void StartFadeCoroutine(IEnumerator fadeFunction)
        {
            if (_fadeCoroutine != null)
            {
                StopCoroutine(_fadeCoroutine);  // Stop the ongoing coroutine if any
            }
            _fadeCoroutine = StartCoroutine(fadeFunction);  // Start the new coroutine
        }

        IEnumerator UnFadeCard()
        {
            float elapsedTime = 0f;
            var curAlpha = _canvasGroup.alpha;

            while (elapsedTime < FADE_DURATION)
            {
                float value = Mathf.Lerp(curAlpha, 1f, elapsedTime / FADE_DURATION);
                _canvasGroup.alpha = value;

                elapsedTime += Time.deltaTime;
                yield return null;
            }

            _canvasGroup.alpha = 1;
        }

        IEnumerator FadeCard()
        {
            float elapsedTime = 0f;
            var curAlpha = _canvasGroup.alpha;

            while (elapsedTime < FADE_DURATION)
            {
                float value = Mathf.Lerp(curAlpha, 0f, elapsedTime / FADE_DURATION);
                _canvasGroup.alpha = value;

                elapsedTime += Time.deltaTime;
                yield return null;
            }

            _canvasGroup.alpha = 0;
        }

        public void OnPointerEnter(BaseEventData data)
        {
            if(CardsManager.Instance.HasACardSelected()) return;
            
            _outlineObj.SetActive(true);

            if (_cardState == ECardState.Docked)
            {
                _cardSlot.ToggleHover(true);
            }
        }
        
        public void OnPointerExit(BaseEventData data)
        {
            if(CardsManager.Instance.HasACardSelected()) return;
            
            _outlineObj.SetActive(false);
            
            if (_cardState == ECardState.Docked)
            {
                _cardSlot.ToggleHover(false);
            }
        }
        
        public void OnPointerClick(BaseEventData data)
        {
            if(_cardState == ECardState.BeingDragged) return;
            
            if (_cardState == ECardState.ShowingDetails)
            {
                CloseDetails();
                CardsManager.Instance.HideCardDetails();
                CardsManager.Instance.AssignCardSelected(null);
                _cardSlot.ToggleHover(false);
            }
            else
            {
                if (CardsManager.Instance.HasACardSelected()) return;

                if (_cardState != ECardState.ShowingDetails) // Maybe also check dragging??
                {
                    CardsManager.Instance.AssignCardSelected(this);
                    ShowCardDetails();
                }
            }
        }

        public void CloseDetails()
        {
            HideCardDetails();
            SnapToDock();
        }

        #region Drag Handling

        public void OnBeginDrag(BaseEventData data)
        {
            if (!_canAfford) return;
            
            if (_cardState == ECardState.ShowingDetails)
            {
                HideCardDetails();
                CardsManager.Instance.HideCardDetails();
            }
            
            transform.SetParent(CardsManager.Instance.transform);
            GetComponent<RectTransform>().localScale = new Vector3(DOCKED_CARD_SCALE, DOCKED_CARD_SCALE, 1);
            
            _cardSlot.ToggleHover(false);
            _cardState = ECardState.BeingDragged;
            CardsManager.Instance.AssignCardSelected(this);
        }

        public Vector2 HoldPosition
        {
            get
            {
                if (_cardState == ECardState.BeingDragged)
                {
                    return _dragPosition;
                }
                else
                {
                    return transform.position;
                }
            }
        }

        private Vector2 _dragPosition;
        public void OnDrag(BaseEventData data)
        {
            if(_cardState != ECardState.BeingDragged) return;
            
            PointerEventData pointerData = (PointerEventData)data;
            _dragPosition = pointerData.position;
        
            Vector2 changeInPosition = pointerData.delta / _canvas.scaleFactor;
            _rectTransform.anchoredPosition += changeInPosition;

            // Calculate velocity
            _velocity = changeInPosition / Time.deltaTime;
        
            // Smooth out the velocity to prevent jitter
            float velocityDamping = 0.5f; // Adjust this value as needed
            _smoothedVelocity = Vector2.Lerp(_smoothedVelocity, _velocity, velocityDamping);
        }

        public void OnEndDrag(BaseEventData data)
        {
            if (_cardState != ECardState.BeingDragged) return;

            if (_inDockArea)
            {
                SnapToDock();
            }
            else
            {
                if (_canAfford)
                {
                    TriggerCardPower();
                }
                else
                {
                    SnapToDock();
                }
            }
        }

        private void DragUpdate()
        {
            // Introduce a decay factor
            float decayFactor = 0.9f; // This value can be adjusted for faster or slower decay

            if(_cardState != ECardState.BeingDragged)
            {
                _smoothedVelocity *= decayFactor;

                // A threshold to ensure the smoothedVelocity eventually becomes zero
                if (_smoothedVelocity.magnitude < 0.001f)
                {
                    _smoothedVelocity = Vector2.zero;
                }
            }
            
            float tiltAngleX = Mathf.Clamp(_smoothedVelocity.y * TILT_AMOUNT_STRENGTH, -TILT_AMOUNT, TILT_AMOUNT);
            float tiltAngleY = Mathf.Clamp(-_smoothedVelocity.x * TILT_AMOUNT_STRENGTH, -TILT_AMOUNT, TILT_AMOUNT);
    
            Quaternion targetRotation;
            if (_cardState == ECardState.BeingDragged || (_smoothedVelocity.magnitude > 0.01f)) // Check if still dragging or if there's some residual velocity
            {
                targetRotation = Quaternion.Euler(tiltAngleX, tiltAngleY, 0);
            }
            else
            {
                targetRotation = Quaternion.identity; // Flat orientation
            }
    
            float rotationSpeed = 0.1f; // Adjust this value for faster or slower rotation
            _rectTransform.localRotation = Quaternion.Slerp(_rectTransform.localRotation, targetRotation, rotationSpeed);
        }

        #endregion
    }
}
