using UnityEngine;

// Throw this on any object to keep it from being destroyed between scene changes.

namespace InfinityPBR.Modules
{
    public class DontDestroyOnLoad : MonoBehaviour
    {
        private void Start() => DontDestroyOnLoad(gameObject);
    }
}
