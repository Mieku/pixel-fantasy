using UnityEngine;

namespace Managers
{
    public class Singleton<T> : MonoBehaviour where T : MonoBehaviour
    {
        private static T _instance;
        private static object _lock = new object();
        private static bool _applicationIsQuitting = false;

        public static T Instance
        {
            get
            {
                if (_applicationIsQuitting)
                {
                    return _instance;
                }

                lock (_lock)
                {
                    if (_instance == null)
                    {
                        _instance = FindObjectOfType<T>();

                        if (_instance == null)
                        {
                            GameObject newGO = new GameObject();
                            var parent = GameObject.Find("_Managers");
                            if (parent != null)
                            {
                                newGO.transform.parent = parent.transform;
                            }
                            newGO.name = typeof(T).Name;
                            _instance = newGO.AddComponent<T>();
                        }
                    }
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
            else if (_instance != this)
            {
                Destroy(gameObject);
            }
        }

        private void OnApplicationQuit()
        {
            _applicationIsQuitting = true;
        }
    }
}