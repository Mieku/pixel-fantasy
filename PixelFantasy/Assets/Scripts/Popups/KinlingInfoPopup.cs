using Characters;

namespace Popups
{
    public class KinlingInfoPopup : Popup<KinlingInfoPopup>
    {
        public static void Show(Unit unit)
        {
            Open(() => Instance.Refresh(), false);
        }
    
        public override void OnBackPressed()
        {
            Hide();
        }
        
        private void Refresh()
        {
        
        }
    }
}
