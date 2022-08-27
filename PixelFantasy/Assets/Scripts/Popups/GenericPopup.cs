using TMPro;
using UnityEngine;

namespace Popups
{
    public class GenericPopup : Popup<GenericPopup>
    {
        [SerializeField] private TextMeshProUGUI _headerText, _bodyText;
    
        public static void Show(string header, string body)
        {
            Open(() => Instance.DisplayText(header, body), true);
        }
    
        public override void OnBackPressed()
        {
            Hide();
        }
    
        void AnimMoveOutHandler()
        {
            Close();
        }

        private void DisplayText(string header, string body)
        {
            _headerText.text = header;
            _bodyText.text = body;
        }
    }
}
