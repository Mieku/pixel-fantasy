using System.Collections.Generic;
using Data.Item;
using Databrain;
using Systems.Buildings.Scripts;
using UnityEngine;

namespace Data.Structure
{
    public class DoorSettings : DataObject
    {
        [SerializeField] private Door _doorPrefab;
        [SerializeField] private CraftRequirements _craftRequirements;
        [SerializeField] private Sprite _optionIcon;
        [SerializeField] private int _maxDurability;
        [SerializeField] private Sprite _horizontalDoorframe;
        [SerializeField] private Sprite _horizontalDoormat;
        [SerializeField] private Sprite _verticalDoorframe;
        [SerializeField] private Sprite _verticalDoormat;

        public Door DoorPrefab => _doorPrefab;
        public CraftRequirements CraftRequirements => _craftRequirements.Clone();
        public Sprite OptionIcon => _optionIcon;
        public int MaxDurability => _maxDurability;
        public Sprite HorizontalDoorframe => _horizontalDoorframe;
        public Sprite HorizontalDoormat => _horizontalDoormat;
        public Sprite VerticalDoorframe => _verticalDoorframe;
        public Sprite VerticalDoormat => _verticalDoormat;


        public List<string> GetStatsList()
        {
            // TODO: build me
            return new List<string>();
        }
    }
}
