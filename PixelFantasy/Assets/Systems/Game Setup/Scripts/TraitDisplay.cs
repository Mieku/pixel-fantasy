using HUD.Tooltip;
using Systems.Stats.Scripts;
using TMPro;
using UnityEngine;

namespace Systems.Game_Setup.Scripts
{
    public class TraitDisplay : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _name;
        [SerializeField] private TooltipTrigger _tooltip;

        public void Init(Trait trait, string kinlingName)
        {
            _name.text = trait.TraitName;
            _tooltip.Header = trait.TraitName;
            _tooltip.Content = trait.DescriptionString(kinlingName);
        }
    }
}
