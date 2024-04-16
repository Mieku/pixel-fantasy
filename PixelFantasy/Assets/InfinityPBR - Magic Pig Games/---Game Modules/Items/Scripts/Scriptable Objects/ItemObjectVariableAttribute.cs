using System;
using UnityEngine;

namespace InfinityPBR.Modules
{
    [Serializable]
    public class ItemObjectVariableAttribute
    {
        public bool active = true;
        public ItemAttribute ItemAttribute => GetAttribute();
        [SerializeField] internal string _itemAttributeUid;
        [SerializeField] internal ItemAttribute _itemAttribute;
        public float value;

        private ItemAttribute GetAttribute() => _itemAttribute != null 
            ? _itemAttribute 
            : _itemAttribute = Utilities.GetItemAttribute(_itemAttributeUid);
    }
}