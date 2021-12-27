using UnityEngine;

namespace Gods
{
    /// <summary>
    /// Essentially just a singleton pattern with extra flavour!
    /// </summary>
    public class God<T> : MonoBehaviour where T : MonoBehaviour
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
                        var parent = GameObject.Find("_Gods");
                        if (parent != null)
                        {
                            newGO.transform.parent = parent.transform;
                        }
                        newGO.name = "Spawned God";
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
