using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace HUD
{
    public class OptionButton : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _btnLabel;
        [SerializeField] private Image _image;

        private Action<OptionButton> _onPressed;

        public void Init(string optionName, Sprite icon, Action<OptionButton> onPressed)
        {
            _btnLabel.text = optionName;
            _image.sprite = icon;
            _onPressed = onPressed;
        }

        public Sprite Icon
        {
            get => _image.sprite;
            set => _image.sprite = value;
        }

        public void ButtonPressed()
        {
            _onPressed(this);
        }
    }
}
