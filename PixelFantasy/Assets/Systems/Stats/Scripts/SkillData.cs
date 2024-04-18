using System;
using Databrain.Attributes;
using UnityEngine;

namespace Systems.Stats.Scripts
{
    [Serializable]
    public class SkillData
    {
        [ExposeToInspector, DatabrainSerialize]
        public int Level;
        
        [ExposeToInspector, DatabrainSerialize]
        public int Exp;
    }
}
