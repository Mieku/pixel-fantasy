using System.Collections.Generic;
using Managers;
using UnityEngine;

namespace Systems.SmartObjects.Scripts
{
    public class SmartObjectManager : Singleton<SmartObjectManager>
    {
        public List<SmartObject> RegisteredSmartObjects { get; private set; } = new List<SmartObject>();

        public void RegisterSmartObject(SmartObject toRegister)
        {
            RegisteredSmartObjects.Add(toRegister);
        }

        public void DeregisterSmartObject(SmartObject toDeregister)
        {
            RegisteredSmartObjects.Remove(toDeregister);
        }
    }
}
