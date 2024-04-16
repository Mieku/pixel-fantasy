using System;
using UnityEngine;

namespace InfinityPBR.Modules
{
    [Serializable]
    public class MainBlackboard : MonoBehaviour
    {
        public static Blackboard blackboard; // Static reference to this script
        private Blackboard _mainBlackboard;
        
        private void Awake()
        {
            _mainBlackboard = GetComponent<Blackboard>();
            
            if (!blackboard)
            {
                blackboard = _mainBlackboard;
                gameObject.name = "Main Blackboard";
            }
            else if (blackboard != _mainBlackboard)
            {
                Destroy(gameObject);
            }
        }
    }
}
