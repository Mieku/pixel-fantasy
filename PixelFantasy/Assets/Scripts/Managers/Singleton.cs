using UnityEngine;

namespace Managers
{
    /// <summary>
    /// Essentially just a singleton pattern with extra flavour!
    /// </summary>
    public class Singleton<T> : MonoBehaviour where T : MonoBehaviour
    {
        private static T _instance;

        public static T Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindObjectOfType<T>();
                    if(_instance == null)
                    {
                        GameObject newGO = new GameObject();
                        var parent = GameObject.Find("_Managers");
                        if (parent != null)
                        {
                            newGO.transform.parent = parent.transform;
                        }
                        newGO.name = "Spawned Singleton";
                        _instance = newGO.AddComponent<T>();
                    }
                }

                return _instance;
            }
        }

        protected virtual void Awake()
        {
            _instance = this as T;
        }
    }
}
