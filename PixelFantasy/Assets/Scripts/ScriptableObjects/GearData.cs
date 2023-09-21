using Characters;
using Items;
using Sirenix.OdinInspector;
using UnityEngine;

namespace ScriptableObjects
{
    [CreateAssetMenu(fileName = "EquipmentData", menuName = "CraftedData/EquipmentData", order = 1)]
    public class GearData : CraftedItemData
    {
        [TitleGroup("Equipment")] public GearType Type;
        public bool CanBeDyed;
        [ShowIf("CanBeDyed")] public DyePaletteData DefaultDyePalette;
        [Tooltip("Can be left empty for no requirement")] public JobData RequiredJob;
        
        [ShowIf("@Type == GearType.MainHand || Type == GearType.BothHands")] public GearPiece MainHandHeldGear;
        [ShowIf("Type", GearType.OffHand)] public GearPiece OffHandHeldGear;
        [ShowIf("Type", GearType.Head)] public GearPiece HatGear;
        [ShowIf("Type", GearType.Body)] public GearPiece BodyGear;
        [ShowIf("Type", GearType.Hands)] public GearPiece MainHandGear;
        [ShowIf("Type", GearType.Hands)] public GearPiece OffHandGear;
        [ShowIf("Type", GearType.Pants)] public GearPiece HipsGear;
        [ShowIf("Type", GearType.Pants)] public GearPiece LeftLegGear;
        [ShowIf("Type", GearType.Pants)] public GearPiece RightLegGear;

        public override ItemState CreateState(string uid)
        {
            return new GearState(this, uid);
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
        BothHands = 6,
        Necklace = 7,
        Ring = 8,
        Carried = 9,
    }
}
