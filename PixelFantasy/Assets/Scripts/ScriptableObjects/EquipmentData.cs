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
        
        [ShowIf("@Type == EquipmentType.MainHand || Type == EquipmentType.BothHands")][BoxGroup("Side View")] public Sprite MainHandHeldStraight_Side;
        [ShowIf("@Type == EquipmentType.MainHand || Type == EquipmentType.BothHands")][BoxGroup("Side View")] public Sprite MainHandHeldDiagonal_Side;
        [ShowIf("@Type == EquipmentType.MainHand || Type == EquipmentType.BothHands")][BoxGroup("Side View")] public bool HoldDiagonal_Side;
        [ShowIf("Type", EquipmentType.OffHand)][BoxGroup("Side View")] public Sprite OffHandHeldStraight_Side;
        [ShowIf("Type", EquipmentType.Head)][BoxGroup("Side View")] public Sprite Hat_Side;
        [ShowIf("Type", EquipmentType.Body)][BoxGroup("Side View")] public Sprite Body_Side;
        [ShowIf("Type", EquipmentType.Hands)][BoxGroup("Side View")] public Sprite MainHand_Side;
        [ShowIf("Type", EquipmentType.Hands)][BoxGroup("Side View")] public Sprite OffHand_Side;
        [ShowIf("Type", EquipmentType.Pants)][BoxGroup("Side View")] public Sprite Hips_Side;
        [ShowIf("Type", EquipmentType.Pants)][BoxGroup("Side View")] public Sprite LeftLeg_Side;
        [ShowIf("Type", EquipmentType.Pants)][BoxGroup("Side View")] public Sprite RightLeg_Side;
        
        [ShowIf("@Type == EquipmentType.MainHand || Type == EquipmentType.BothHands")][BoxGroup("Up View")] public Sprite MainHandHeldStraight_Up;
        [ShowIf("@Type == EquipmentType.MainHand || Type == EquipmentType.BothHands")][BoxGroup("Up View")] public Sprite MainHandHeldDiagonal_Up;
        [ShowIf("@Type == EquipmentType.MainHand || Type == EquipmentType.BothHands")][BoxGroup("Up View")] public bool HoldDiagonal_Up;
        [ShowIf("Type", EquipmentType.OffHand)][BoxGroup("Up View")] public Sprite OffHandHeldStraight_Up;
        [ShowIf("Type", EquipmentType.Head)][BoxGroup("Up View")] public Sprite Hat_Up;
        [ShowIf("Type", EquipmentType.Body)][BoxGroup("Up View")] public Sprite Body_Up;
        [ShowIf("Type", EquipmentType.Hands)][BoxGroup("Up View")] public Sprite MainHand_Up;
        [ShowIf("Type", EquipmentType.Hands)][BoxGroup("Up View")] public Sprite OffHand_Up;
        [ShowIf("Type", EquipmentType.Pants)][BoxGroup("Up View")] public Sprite Hips_Up;
        [ShowIf("Type", EquipmentType.Pants)][BoxGroup("Up View")] public Sprite LeftLeg_Up;
        [ShowIf("Type", EquipmentType.Pants)][BoxGroup("Up View")] public Sprite RightLeg_Up;
        
        [ShowIf("@Type == EquipmentType.MainHand || Type == EquipmentType.BothHands")][BoxGroup("Down View")] public Sprite MainHandHeldStraight_Down;
        [ShowIf("@Type == EquipmentType.MainHand || Type == EquipmentType.BothHands")][BoxGroup("Down View")] public Sprite MainHandHeldDiagonal_Down;
        [ShowIf("@Type == EquipmentType.MainHand || Type == EquipmentType.BothHands")][BoxGroup("Down View")] public bool HoldDiagonal_Down;
        [ShowIf("Type", EquipmentType.OffHand)][BoxGroup("Down View")] public Sprite OffHandHeldStraight_Down;
        [ShowIf("Type", EquipmentType.Head)][BoxGroup("Down View")] public Sprite Hat_Down;
        [ShowIf("Type", EquipmentType.Body)][BoxGroup("Down View")] public Sprite Body_Down;
        [ShowIf("Type", EquipmentType.Hands)][BoxGroup("Down View")] public Sprite MainHand_Down;
        [ShowIf("Type", EquipmentType.Hands)][BoxGroup("Down View")] public Sprite OffHand_Down;
        [ShowIf("Type", EquipmentType.Pants)][BoxGroup("Down View")] public Sprite Hips_Down;
        [ShowIf("Type", EquipmentType.Pants)][BoxGroup("Down View")] public Sprite LeftLeg_Down;
        [ShowIf("Type", EquipmentType.Pants)][BoxGroup("Down View")] public Sprite RightLeg_Down;

        
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
