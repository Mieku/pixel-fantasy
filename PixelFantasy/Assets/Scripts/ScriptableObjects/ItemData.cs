using System.Collections.Generic;
using Items;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.EventSystems;

namespace ScriptableObjects
{
    [CreateAssetMenu(fileName = "ItemData", menuName = "ItemData", order = 1)]
    public class ItemData : ScriptableObject
    {
        public string ItemName;
        public int MaxStackSize;
    
        [PreviewField] public Sprite ItemSprite;
        public Vector2 DefaultSpriteScale;

        [SerializeField] private List<Option> _options;
        
        public List<Option> Options
        {
            get
            {
                List<Option> clone = new List<Option>();
                foreach (var option in _options)
                {
                    clone.Add(option);
                }

                return clone;
            }
        }
    }
}
