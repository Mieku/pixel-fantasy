using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Systems.Appearance.Scripts
{
    [CreateAssetMenu(fileName = "HairSettings", menuName = "Settings/Kinlings/Hair Settings")]
    public class HairSettings : ScriptableObject
    {
        public string Name;
        public List<HairColourOption> ColourOptions;

        public HairColourOption GetRandomHairColour()
        {
            int rand = Random.Range(0, ColourOptions.Count);
            return ColourOptions[rand];
        }
    }

    [Serializable]
    public class HairColourOption
    {
        public Color Color;
        public Sprite Side, Front, Back;
        public Sprite Portrait;
    }
}
