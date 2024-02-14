using Characters;
using UnityEngine;

namespace Popups.Kinling_Info_Popup
{
    public class KinlingInfoJobContent : MonoBehaviour
    {
        [SerializeField] private Transform _prevJobSkillsParent;
        
        private Kinling _kinling;

        public void Show(Kinling kinling)
        {
            _kinling = kinling;
            
            gameObject.SetActive(true);
        }

        public void Close()
        {
            gameObject.SetActive(false);
        }
    }
}
