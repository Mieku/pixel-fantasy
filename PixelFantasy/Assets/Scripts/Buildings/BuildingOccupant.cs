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

        private Kinling _kinling;
        private Action<Kinling> _onPressedCallback;
        
        public void Init(Kinling kinling, Action<Kinling> onPressedCallback)
        {
            _kinling = kinling;
            _onPressedCallback = onPressedCallback;

            if (kinling != null)
            {
                _addSymbol.SetActive(false);
                _portrait.SetActive(true);
                _nameText.text = kinling.FullName;
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
            _onPressedCallback.Invoke(_kinling);
        }
    }
}
