using Characters;
using UnityEngine;

namespace AI
{
    public class DraftedHandler : MonoBehaviour
    {
        [SerializeField] private Kinling _kinling;
        
        private KinlingData _kinlingData => _kinling.RuntimeData;

        public void SetDrafted(bool isDrafted)
        {
            
        }
    }
}
