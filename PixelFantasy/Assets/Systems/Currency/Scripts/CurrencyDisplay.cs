using DG.Tweening;
using TMPro;
using UnityEngine;

namespace Systems.Currency.Scripts
{
    public class CurrencyDisplay : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _amountDisplay;
        [SerializeField] private Color _positiveChange;
        [SerializeField] private Color _negativeChange;

        private int _prevAmount;
        private Color _originalColor;
        private Vector3 _originalScale;

        private void Awake()
        {
            _originalColor = _amountDisplay.color;
            _originalScale = _amountDisplay.transform.localScale;

            _amountDisplay.text = "0";
            _prevAmount = 0;
        }
        
        public void UpdateAmount(int newTotal)
        {
            _amountDisplay.text = newTotal.ToString();

            if (newTotal < _prevAmount)
            {
                ShowNegativeChange();
            } 
            else if (newTotal > _prevAmount)
            {
                ShowPositiveChange();
            }

            _prevAmount = newTotal;
        }

        private void ShowPositiveChange()
        {
            PulseText(_positiveChange);
        }

        private void ShowNegativeChange()
        {
            PulseText(_negativeChange);
        }
        
        private void PulseText(Color changeColor)
        {
            _amountDisplay.transform.DOScale(_originalScale * 1.2f, 0.1f).OnComplete(() =>
            {
                _amountDisplay.transform.DOScale(_originalScale, 0.1f);
            });

            _amountDisplay.DOColor(changeColor, 0.1f).OnComplete(() =>
            {
                _amountDisplay.DOColor(_originalColor, 0.1f);
            });
        }
    }
}
