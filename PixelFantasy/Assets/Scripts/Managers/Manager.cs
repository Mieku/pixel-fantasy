using UnityEngine;

namespace Managers
{
    public class Manager<T> : MonoBehaviour where T : MonoBehaviour
    {
        private static bool _applicationIsQuitting = false;
        private static T _instance;
    
        public static T Instance
        {
            get
            {
                if (_applicationIsQuitting)
                {
                    return _instance;
                }
            
                return _instance;
            }
        }
    
        protected virtual void Awake()
        {
            if (_instance == null)
            {
                _instance = this as T;
            }
        }

        private void OnApplicationQuit()
        {
            _applicationIsQuitting = true;
        }
    }
}
