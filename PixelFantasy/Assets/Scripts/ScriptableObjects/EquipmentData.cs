using Characters;
using Items;
using Sirenix.OdinInspector;
using UnityEngine;

namespace ScriptableObjects
{
    [CreateAssetMenu(fileName = "EquipmentData", menuName = "CraftedData/EquipmentData", order = 1)]
    public class EquipmentData : CraftedItemData
    {
        [TitleGroup("Equipment")] public EquipmentType Type;
        public bool CanBeDyed;
        [ShowIf("CanBeDyed")] public DyePaletteData DefaultDyePalette;
        
        [ShowIf("@Type == EquipmentType.MainHand || Type == EquipmentType.BothHands")] public GearPiece MainHandHeldGear;
        [ShowIf("Type", EquipmentType.OffHand)] public GearPiece OffHandHeldGear;
        [ShowIf("Type", EquipmentType.Head)] public GearPiece HatGear;
        [ShowIf("Type", EquipmentType.Body)] public GearPiece BodyGear;
        [ShowIf("Type", EquipmentType.Hands)] public GearPiece MainHandGear;
        [ShowIf("Type", EquipmentType.Hands)] public GearPiece OffHandGear;
        [ShowIf("Type", EquipmentType.Pants)] public GearPiece HipsGear;
        [ShowIf("Type", EquipmentType.Pants)] public GearPiece LeftLegGear;
        [ShowIf("Type", EquipmentType.Pants)] public GearPiece RightLegGear;

        public override ItemState CreateState(string uid)
        {
            return new EquipmentState(this, uid);
        }
    }

    public enum EquipmentType
    {
        Head = 0,
        Body = 1,
        Pants = 2,
        Hands = 3,
        MainHand = 4,
        OffHand = 5,
        BothHands = 6,
        Necklace = 7,
        Ring = 8,
        Carried = 9,
    }
}
