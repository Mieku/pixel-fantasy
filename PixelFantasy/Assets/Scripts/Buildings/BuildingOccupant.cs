using System;
using Characters;
using TMPro;
using UnityEngine;

namespace Buildings
{
    public class BuildingOccupant : MonoBehaviour
    {
        [SerializeField] private GameObject _portrait;
        [SerializeField] private GameObject _addSymbol;
        [SerializeField] private TextMeshProUGUI _nameText;
        [SerializeField] private TextMeshProUGUI _titleText;

        private Unit _unit;
        private Action<Unit> _onPressedCallback;
        
        public void Init(Unit unit, Action<Unit> onPressedCallback)
        {
            _unit = unit;
            _onPressedCallback = onPressedCallback;

            if (unit != null)
            {
                _addSymbol.SetActive(false);
                _portrait.SetActive(true);
                _nameText.text = unit.FullName;
                _titleText.text = "Resident";
            }
            else
            {
                _addSymbol.SetActive(true);
                _portrait.SetActive(false);
                _nameText.text = "Select Kinling";
                _titleText.text = "Resident";
            }
        }
        
        public void OnPressed()
        {
            _onPressedCallback.Invoke(_unit);
        }
    }
}
