using HUD;
using Systems.Needs.Scripts;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Systems.Details.Kinling_Details
{
    public class NeedDisplay : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _needName;
        [SerializeField] private BarThresholdDisplay _thresholdDisplay;
        [SerializeField] private BarTargetIndicator _targetIndicator;
        [SerializeField] private Image _barFill;
        [SerializeField] private GameObject _increasingArrow;
        [SerializeField] private GameObject _decreasingArrow;

        private Need _need;
        private float _prevValue;

        public void Init(Need need)
        {
            _need = need;
            
            _thresholdDisplay.ShowThresholds(_need.GetThresholds());

            _prevValue = _need.Value;
            Refresh();
        }

        public void Refresh()
        {
            _barFill.fillAmount = _need.Value;

            if (_prevValue > _need.Value) // Decreasing
            {
                _increasingArrow.SetActive(false);
                _decreasingArrow.SetActive(true);
            } 
            else if (_prevValue < _need.Value) // Increasing
            {
                _increasingArrow.SetActive(true);
                _decreasingArrow.SetActive(false);
                
            }
            else // nothing
            {
                _increasingArrow.SetActive(false);
                _decreasingArrow.SetActive(false);
            }

            _prevValue = _need.Value;
            
            if (_need.HasTargetValue)
            {
                _targetIndicator.DisplayIndicator(true);
                _targetIndicator.SetTargetIndicator(_need.TargetValue);
            }
            else
            {
                _targetIndicator.DisplayIndicator(false);
            }
        }
    }
}
