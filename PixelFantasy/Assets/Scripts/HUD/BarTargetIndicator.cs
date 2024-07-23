using UnityEngine;

namespace HUD
{
    public class BarTargetIndicator : MonoBehaviour
    {
        [SerializeField] private RectTransform _targetIndicator;
        
        private float _barWidth;
        
        public void SetTargetIndicator(float percentage)
        {
            var xPos = GetXPosForPercentage(percentage);
            
            Vector2 anchoredPosition = _targetIndicator.anchoredPosition;

            // Modify the x component of the anchored position.
            anchoredPosition.x = xPos;

            // Assign the modified anchored position back to the RectTransform.
            _targetIndicator.anchoredPosition = anchoredPosition;
        }

        public void DisplayIndicator(bool showIndicator)
        {
            _targetIndicator.gameObject.SetActive(showIndicator);
        }
        
        private float GetXPosForPercentage(float percentage)
        {
            var rectTransform = GetComponent<RectTransform>();
            _barWidth = rectTransform.rect.width;
            return _barWidth * percentage;
        }
    }
}
