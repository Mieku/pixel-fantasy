using ScriptableObjects;
using UnityEngine;

namespace Items
{
    public class Resource : MonoBehaviour
    {
        [SerializeField] protected ResourceData _resourceData;

        public ResourceData GetResourceData()
        {
            return _resourceData;
        }
    }
}
