using HUD.Tooltip;
using TMPro;
using UnityEngine;

namespace Systems.Details.Kinling_Details
{
    public class TraitDisplay : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _traitTitle;
        [SerializeField] private TooltipTrigger _tooltip;

        public void Init(string header, string content)
        {
            _traitTitle.text = header;
            _tooltip.Header = header;
            _tooltip.Content = content;
        }
    }
}
