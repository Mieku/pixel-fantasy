using System;
using System.Collections.Generic;
using Characters;
using Items;
using Sirenix.OdinInspector;
using Systems.Skills.Scripts;
using UnityEngine;

namespace ScriptableObjects
{
    [CreateAssetMenu(fileName = "EquipmentData", menuName = "ItemData/CraftedItemData/EquipmentData", order = 1)]
    public class GearData : CraftedItemData
    {
        [TitleGroup("Equipment")] public GearType Type;
        public bool CanBeDyed;
        [ShowIf("CanBeDyed")] public DyePaletteData DefaultDyePalette;
        public int TierLevel;
        public List<Skill> SkillStats = new List<Skill>();
        
        [ShowIf("@Type == GearType.MainHand")] public GearPiece MainHandHeldGear;
        [ShowIf("Type", GearType.OffHand)] public GearPiece OffHandHeldGear;
        [ShowIf("Type", GearType.Head)] public GearPiece HatGear;
        [ShowIf("Type", GearType.Body)] public GearPiece BodyGear;
        [ShowIf("Type", GearType.Hands)] public GearPiece MainHandGear;
        [ShowIf("Type", GearType.Hands)] public GearPiece OffHandGear;
        [ShowIf("Type", GearType.Pants)] public GearPiece HipsGear;
        [ShowIf("Type", GearType.Pants)] public GearPiece LeftLegGear;
        [ShowIf("Type", GearType.Pants)] public GearPiece RightLegGear;

        public override ItemState CreateState(string uid, Item item)
        {
            return new GearState(this, uid, item);
        }
        
        public override ItemState CreateState()
        {
            string uid = $"{ItemName}_{Guid.NewGuid()}";
            return new GearState(this, uid, null);
        }
    }

    public enum GearType
    {
        Head = 0,
        Body = 1,
        Pants = 2,
        Hands = 3,
        MainHand = 4,
        OffHand = 5,
        Necklace = 6,
        Ring = 7,
        Carried = 8,
    }
}
