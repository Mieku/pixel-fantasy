using TMPro;
using UnityEngine;

namespace Systems.Details.Generic_Details.Scripts
{
    public class DetailsEntryTextDisplay : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _textMsg;

        public void Init(string msg)
        {
            _textMsg.text = msg;
        }
    }
}
