using UnityEngine;

namespace Popups
{
    public class KinlingsPopup : Popup<KinlingsPopup>
    {
        public static void Show()
        {
            Open(() => Instance.Init(), false);
        }

        private void Init()
        {
            Debug.Log("Popup Opened");
        }
    
        public override void OnBackPressed()
        {
            Hide();
        }
    }
}
