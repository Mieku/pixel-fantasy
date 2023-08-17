using Characters;
using UnityEngine;

namespace Popups.Change_Job_Popup
{
    public class ChangeJobPopup : Popup<ChangeJobPopup>
    {
        
        public static void Show(Unit unit)
        {
            Open(() => Instance.Init(unit), false);
        }
        
        public override void OnBackPressed()
        {
            Hide();
        }
        
        private void Init(Unit unit)
        {
            
        }
    }
}
