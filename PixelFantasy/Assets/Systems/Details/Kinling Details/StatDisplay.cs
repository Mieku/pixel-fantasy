using Characters;
using HUD.Tooltip;
using Systems.Stats.Scripts;
using TMPro;
using UnityEngine;

namespace Systems.Details.Kinling_Details
{
    public class StatDisplay : MonoBehaviour
    {
        public EAttributeType Attribute;
        [SerializeField] private TextMeshProUGUI _statName;
        [SerializeField] private TextMeshProUGUI _statValue;
        [SerializeField] private TooltipTrigger _tooltip;
        [SerializeField] private string _valueSuffix = "%";
        [SerializeField] private float _baseValue = 100;

        public void Refresh(KinlingData kinlingData)
        {
            var value = kinlingData.Stats.GetTotalAttributeModifier(Attribute);
            value = _baseValue + (value * 100f);
            _statValue.text = $"{value}{_valueSuffix}";
        }
    }
}
