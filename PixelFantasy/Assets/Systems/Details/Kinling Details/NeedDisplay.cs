using Popups.Kinling_Info_Popup;
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

        private NeedState _needState;
        private float _prevValue;

        public void Init(NeedState needState)
        {
            _needState = needState;
            
            _thresholdDisplay.ShowThresholds(_needState.GetThresholds());

            _prevValue = _needState.Value;
            Refresh();
        }

        public void Refresh()
        {
            _barFill.fillAmount = _needState.Value;

            if (_prevValue > _needState.Value) // Decreasing
            {
                _increasingArrow.SetActive(false);
                _decreasingArrow.SetActive(true);
            } 
            else if (_prevValue < _needState.Value) // Increasing
            {
                _increasingArrow.SetActive(true);
                _decreasingArrow.SetActive(false);
                
            }
            else // nothing
            {
                _increasingArrow.SetActive(false);
                _decreasingArrow.SetActive(false);
            }

            _prevValue = _needState.Value;
            
            if (_needState.HasTargetValue)
            {
                _targetIndicator.DisplayIndicator(true);
                _targetIndicator.SetTargetIndicator(_needState.TargetValue);
            }
            else
            {
                _targetIndicator.DisplayIndicator(false);
            }
        }
    }
}
