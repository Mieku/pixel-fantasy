using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Systems.Details.Generic_Details.Scripts
{
    public class DetailsEntryBarDisplay : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _titleText;
        [SerializeField] private Image _icon;
        [SerializeField] private Image _barFill;
        
        public delegate float GetFillPercentDelegate();
        public GetFillPercentDelegate FetchFillPercent;

        public void Init(string title, Sprite icon, GetFillPercentDelegate fetchFillPercent)
        {
            _titleText.text = $"{title}:";
            _icon.sprite = icon;
            FetchFillPercent = fetchFillPercent;
            RefreshFill();
        }

        public void RefreshFill()
        {
            float fillPercent = FetchFillPercent();
            _barFill.fillAmount = Mathf.Clamp01(fillPercent);
        }
    }
}
