using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using UnityEngine;

[CreateAssetMenu(fileName = "ItemData", menuName = "ItemData", order = 1)]
public class ItemData : ScriptableObject
{
    public string ItemName;
    public int MaxStackSize;
    
    [PreviewField] public Sprite ItemSprite;
    public Vector2 DefaultSpriteScale;
}
