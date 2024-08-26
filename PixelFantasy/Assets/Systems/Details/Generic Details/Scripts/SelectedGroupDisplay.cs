using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace Systems.Details.Generic_Details.Scripts
{
    public class SelectedGroupDisplay : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _textDisplay;

        private List<PlayerInteractable> _piGroup = new List<PlayerInteractable>();
        private Action<List<PlayerInteractable>> _onSelectGroup;
        private Action<List<PlayerInteractable>> _onRemoveGroup;

        public void Init(List<PlayerInteractable> piGroup, Action<List<PlayerInteractable>> onSelectGroup,
            Action<List<PlayerInteractable>> onRemoveGroup, string displayTextOverride = null)
        {
            if (piGroup == null || piGroup.Count == 0)
            {
                Debug.LogError("Empty PI Group");
                return;
            }
            
            _piGroup = piGroup;
            _onSelectGroup = onSelectGroup;
            _onRemoveGroup = onRemoveGroup;

            if (string.IsNullOrEmpty(displayTextOverride))
            {
                int amount = _piGroup.Count;
                string displayName = _piGroup[0].DisplayName;
                _textDisplay.text = $"{amount} {displayName}";
            }
            else
            {
                _textDisplay.text = displayTextOverride;
            }
        }

        public void OnLeftClick()
        {
            _onSelectGroup?.Invoke(_piGroup);
        }

        public void OnRightClick()
        {
            _onRemoveGroup?.Invoke(_piGroup);
        }
    }
}
