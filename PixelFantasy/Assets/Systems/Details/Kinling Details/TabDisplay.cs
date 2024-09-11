using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Systems.Details.Kinling_Details
{
    public class TabDisplay : MonoBehaviour
    {
        [SerializeField] private Sprite _defaultSprite;
        [SerializeField] private Sprite _activeSprite;
        [SerializeField] private Color _inactiveColour;
        [SerializeField] private Color _activeColour;
        [SerializeField] private TextMeshProUGUI _tabText;
        [SerializeField] private Image _tabImage;
        
        public UnityEvent OnTabSelected;

        public void OnTabPressed()
        {
            OnTabSelected.Invoke();
        }

        public void SetActive(bool active)
        {
            if (active)
            {
                _tabText.color = _activeColour;
                _tabImage.sprite = _activeSprite;
            }
            else
            {
                _tabText.color = _inactiveColour;
                _tabImage.sprite = _defaultSprite;
            }
        }
    }
}
