using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Systems.Game_Setup.Scripts
{
    public class RecolourButtonTextOnHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        public Color HoverColor = Color.white;
        
        private TextMeshProUGUI _buttonText;
        private Color _normalColor;

        private void Awake()
        {
            _buttonText = GetComponentInChildren<TextMeshProUGUI>();
            _normalColor = _buttonText.color;
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (_buttonText != null)
            {
                _buttonText.color = HoverColor;
            }
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (_buttonText != null)
            {
                _buttonText.color = _normalColor;
            }
        }
    }
}
