using System;
using System.Collections.Generic;
using ScriptableObjects;
using UnityEngine;

namespace Gods
{
    public class Librarian : God<Librarian>
    {
        [SerializeField] private List<ColourData> _colourLibrary;
        [SerializeField] private List<StructureData> _structureLibrary;
        [SerializeField] private List<Sprite> _sprites;

        public Color GetColour(string colourName)
        {
            var result = _colourLibrary.Find(c => c.Name == colourName);
            if (result != null)
            {
                return result.Colour;
            }
            else
            {
                Debug.LogError("Unknown Colour: " + colourName);
                return Color.magenta;
            }
        }

        public StructureData GetStructureData(string key)
        {
            var result = _structureLibrary.Find(s => s.StructureName == key);
            if (result == null)
            {
                Debug.LogError("Unknown Structure: " + key);
            }
            return result;
        }

        public Sprite GetSprite(string spriteName)
        {
            var result = _sprites.Find(s => s.name == spriteName);
            return result;
        }
    }

    [Serializable]
    public class ColourData
    {
        public string Name;
        public Color Colour;
    }
}
