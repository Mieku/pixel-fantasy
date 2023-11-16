using UnityEngine;

namespace Systems.Notifications.Scripts
{
    public class Toaster : MonoBehaviour
    {
        [SerializeField] private Toast _toastPrefab;
        [SerializeField] private Transform _toastParent;

        public void Toast(string message)
        {
            var toast = Instantiate(_toastPrefab, _toastParent);
            toast.Init(message);
        }
        
        
    }
}
