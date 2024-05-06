using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Systems.Appearance.Scripts
{
    [CreateAssetMenu(fileName = "HairSettings", menuName = "Settings/Kinlings/Hair Settings")]
    public class HairSettings : ScriptableObject
    {
        public string ID;
        public Sprite Portrait;
    }
}
