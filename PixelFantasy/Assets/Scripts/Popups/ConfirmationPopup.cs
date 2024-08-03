using System;
using TMPro;
using UnityEngine;

namespace Popups
{
    public class ConfirmationPopup : Popup<ConfirmationPopup>
    {
        [SerializeField] private TextMeshProUGUI _promptText;

        private Action<bool> _confirmationCallback;
        
        public static void Show(string promptMsg, Action<bool> confirmationCallback)
        {
            Open(() => Instance.Refresh(promptMsg, confirmationCallback), true);
        }
        
        public override void OnBackPressed()
        {
            OnNoPressed();
        }

        private void Refresh(string promptMsg, Action<bool> confirmationCallback)
        {
            _confirmationCallback = confirmationCallback;
            _promptText.text = promptMsg;
        }

        public void OnYesPressed()
        {
            Hide();
            _confirmationCallback?.Invoke(true);
        }

        public void OnNoPressed()
        {
            Hide();
            _confirmationCallback?.Invoke(false);
        }
    }
}
