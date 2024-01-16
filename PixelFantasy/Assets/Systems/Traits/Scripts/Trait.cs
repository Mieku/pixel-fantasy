using System;
using ScriptableObjects;
using Systems.Needs.Scripts;
using UnityEngine;
using UnityEngine.Serialization;

namespace Systems.Traits.Scripts
{
    public class Trait : ScriptableObject
    {
        public string DisplayName;
        public string Description;
        public Sprite Icon;
        public int ImpactScore;
    }
}
