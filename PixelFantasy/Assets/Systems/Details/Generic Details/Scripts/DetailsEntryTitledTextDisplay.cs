using TMPro;
using UnityEngine;

namespace Systems.Details.Generic_Details.Scripts
{
    public class DetailsEntryTitledTextDisplay : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _title;
        [SerializeField] private TextMeshProUGUI _textMsg;

        public void Init(string title, string msg)
        {
            _title.text = $"{title}:";
            _textMsg.text = msg;
        }
    }
}
