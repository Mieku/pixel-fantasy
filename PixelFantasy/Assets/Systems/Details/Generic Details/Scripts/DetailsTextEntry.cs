using TMPro;
using UnityEngine;

namespace Systems.Details.Generic_Details.Scripts
{
    public class DetailsTextEntry : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _text;
        [SerializeField] private Color _headerColour;
        [SerializeField] private Color _msgColour;

        public void Init(string msg, string header = null)
        {
            _text.color = _msgColour;

            if (header != null)
            {
                _text.text = $"<color={Helper.ColorToHex(_headerColour)}>{header}:</color>  {msg}";
            }
            else
            {
                _text.text = $"{msg}";
            }
        }
    }
}
