using System;
using UnityEngine;

namespace InfinityPBR.Modules
{
    [Serializable]
    public class ObjectReferenceObject
    {
        public string guid;
        //public object obj;
        public string typeName;
        
        [SerializeField] private Stat _stat;
        [SerializeField] private ItemObject _itemObject;
        [SerializeField] private ItemAttribute _itemAttribute;
        [SerializeField] private Condition _condition;
        [SerializeField] private Quest _quest;
        [SerializeField] private QuestCondition _questCondition;
        [SerializeField] private QuestReward _questReward;
        [SerializeField] private LootBox _lootBox;
        [SerializeField] private LootItems _lootItems;
        [SerializeField] private AnimationClip _animationClip;
        [SerializeField] private Texture2D _texture2D;
        [SerializeField] private AudioClip _audioClip;
        [SerializeField] private Sprite _sprite;
        [SerializeField] private Color _color;
        [SerializeField] private Vector4 _vector4;
        [SerializeField] private Vector3 _vector3;
        [SerializeField] private Vector2 _vector2;
        [SerializeField] private GameObject _gameObject;

        public object GetObject()
        {
            switch (typeName)
            {
                case "InfinityPBR.Modules.Stat":
                    return _stat;
                case "InfinityPBR.Modules.ItemObject":
                    return _itemObject;
                case "InfinityPBR.Modules.ItemAttribute":
                    return _itemAttribute;
                case "InfinityPBR.Modules.Condition":
                    return _condition;
                case "InfinityPBR.Modules.Quest":
                    return _quest;
                case "InfinityPBR.Modules.QuestCondition":
                    return _questCondition;
                case "InfinityPBR.Modules.QuestReward":
                    return _questReward;
                case "InfinityPBR.Modules.LootBox":
                    return _lootBox;
                case "InfinityPBR.Modules.LootItems":
                    return _lootItems;
                case "UnityEngine.AnimationClip":
                    return _animationClip;
                case "UnityEngine.Texture2D":
                    return _texture2D;
                case "UnityEngine.AudioClip":
                    return _audioClip;
                case "UnityEngine.Sprite":
                    return _sprite;
                case "UnityEngine.Color":
                    return _color;
                case "UnityEngine.Vector4":
                    return _vector4;
                case "UnityEngine.Vector3":
                    return _vector3;
                case "UnityEngine.Vector2":
                    return _vector2;
                case "UnityEngine.GameObject":
                    return _gameObject;
                default:
                    Debug.LogWarning($"Unsupported type: {typeName}");
                    return null;
            }
        }
        
        public ObjectReferenceObject(string guidValue, object objValue, string typeNameValue)
        {
            //Debug.Log($"Object Reference Object Constructor: Adding {guidValue} for type {typeNameValue}, object is null> {objValue == null}");
            guid = guidValue;
            //obj = objValue;
            typeName = typeNameValue;

            // Check object type and assign to respective field
            switch (typeName)
            {
                case "InfinityPBR.Modules.Stat":
                    _stat = (Stat)objValue;
                    break;
                case "InfinityPBR.Modules.ItemObject":
                    _itemObject = (ItemObject)objValue;
                    break;
                case "InfinityPBR.Modules.ItemAttribute":
                    _itemAttribute = (ItemAttribute)objValue;
                    break;
                case "InfinityPBR.Modules.Condition":
                    _condition = (Condition)objValue;
                    break;
                case "InfinityPBR.Modules.Quest":
                    _quest = (Quest)objValue;
                    break;
                case "InfinityPBR.Modules.QuestCondition":
                    _questCondition = (QuestCondition)objValue;
                    break;
                case "InfinityPBR.Modules.QuestReward":
                    _questReward = (QuestReward)objValue;
                    break;
                case "InfinityPBR.Modules.LootBox":
                    _lootBox = (LootBox)objValue;
                    break;
                case "InfinityPBR.Modules.LootItems":
                    _lootItems = (LootItems)objValue;
                    break;
                case "UnityEngine.AnimationClip":
                    _animationClip = (AnimationClip)objValue;
                    break;
                case "UnityEngine.Texture2D":
                    _texture2D = (Texture2D)objValue;
                    break;
                case "UnityEngine.AudioClip":
                    _audioClip = (AudioClip)objValue;
                    break;
                case "UnityEngine.Sprite":
                    _sprite = (Sprite)objValue;
                    break;
                case "UnityEngine.Color":
                    _color = (Color)objValue;
                    break;
                case "UnityEngine.Vector4":
                    _vector4 = (Vector4)objValue;
                    break;
                case "UnityEngine.Vector3":
                    _vector3 = (Vector3)objValue;
                    break;
                case "UnityEngine.Vector2":
                    _vector2 = (Vector2)objValue;
                    break;
                case "UnityEngine.GameObject":
                    _gameObject = (GameObject)objValue;
                    break;
                default:
                    Debug.LogWarning($"Unsupported type: {typeNameValue}");
                    break;
            }
        }
    }
}