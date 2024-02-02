using System;
using System.Collections.Generic;
using Characters;
using HUD.Tooltip;
using TMPro;
using UnityEngine;

namespace Systems.Details.Building_Details.Scripts
{
    public class OccupantOption : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _occupantName;
        [SerializeField] private Color _defaultTextColour;
        [SerializeField] private Color _hoverTextColour;
        [SerializeField] private TooltipTrigger _tooltip;

        private Action<Kinling> _onPressedCallback;
        private Kinling _kinling;

        public void Init(Kinling kinling, List<StatType> relevantAbilityTypes, Action<Kinling> onPressedCallback)
        {
            _kinling = kinling;
            _onPressedCallback = onPressedCallback;

            _occupantName.text = _kinling.FirstName;
            _tooltip.Header = _kinling.FirstName;
            var abilityList = _kinling.GetStatList(relevantAbilityTypes);
            _tooltip.Content = abilityList;
        }

        public void OnPressed()
        {
            _onPressedCallback.Invoke(_kinling);
        }

        public void OnHoverChanged(bool isHovering)
        {
            if (isHovering)
            {
                _occupantName.color = _hoverTextColour;
            }
            else
            {
                _occupantName.color = _defaultTextColour;
            }
        }
    }
}
