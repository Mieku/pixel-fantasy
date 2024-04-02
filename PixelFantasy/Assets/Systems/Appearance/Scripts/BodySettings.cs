using System;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Serialization;

namespace Systems.Appearance.Scripts
{
    [CreateAssetMenu(fileName = "BodySettings", menuName = "Settings/Kinlings/Body Settings")]
    public class BodySettings : ScriptableObject
    {
        [FormerlySerializedAs("IsUpDownSame")] public bool IsFrontBackSame;
        
        // Side
        [TitleGroup("Side Sprites")]
        public Sprite HeadSide;
        public Sprite MainHandSide;
        public Sprite OffHandSide;
        public Sprite BodySide;
        public Sprite HipsSide;
        public Sprite LeftLegSide;
        public Sprite RightLegSide;
        public Sprite FaceSide;
        public Sprite FaceSide_EyesClosed;
            
        // Front
        [FormerlySerializedAs("HeadDown")] [TitleGroup("Front Sprites")] public Sprite HeadFront;
        [FormerlySerializedAs("MainHandDown")] public Sprite MainHandFront;
        [FormerlySerializedAs("OffHandDown")] public Sprite OffHandFront;
        [FormerlySerializedAs("BodyDown")] public Sprite BodyFront;
        [FormerlySerializedAs("HipsDown")] public Sprite HipsFront;
        [FormerlySerializedAs("LeftLegDown")] public Sprite LeftLegFront;
        [FormerlySerializedAs("RightLegDown")] public Sprite RightLegFront;
        [FormerlySerializedAs("FaceDown")] public Sprite FaceFront;
        public Sprite FaceFront_EyesClosed;
        
        // Back
        [ShowIf("@IsFrontBackSame == false")][TitleGroup("Back Sprites")]
        [ShowIf("@IsFrontBackSame == false")]public Sprite HeadBack;
        [ShowIf("@IsFrontBackSame == false")]public Sprite MainHandBack;
        [ShowIf("@IsFrontBackSame == false")]public Sprite OffHandBack;
        [ShowIf("@IsFrontBackSame == false")]public Sprite BodyBack;
        [ShowIf("@IsFrontBackSame == false")]public Sprite HipsBack;
        [ShowIf("@IsFrontBackSame == false")]public Sprite LeftLegBack;
        [ShowIf("@IsFrontBackSame == false")]public Sprite RightLegBack;

        public Sprite GetBodySprite(BodyPart bodyPart, UnitActionDirection direction)
        {
            switch (direction)
            {
                case UnitActionDirection.Side:
                    return GetSidePart(bodyPart);
                case UnitActionDirection.Up:
                    if (IsFrontBackSame)
                    {
                        return GetFrontPart(bodyPart);
                    }
                    else
                    {
                        return GetBackPart(bodyPart);
                    }
                case UnitActionDirection.Down:
                    return GetFrontPart(bodyPart);
                default:
                    throw new ArgumentOutOfRangeException(nameof(direction), direction, null);
            }
        }

        private Sprite GetSidePart(BodyPart bodyPart)
        {
            switch (bodyPart)
            {
                case BodyPart.Head:
                    return HeadSide;
                case BodyPart.Body:
                    return BodySide;
                case BodyPart.MainHand:
                    return MainHandSide;
                case BodyPart.OffHand:
                    return OffHandSide;
                case BodyPart.Hips:
                    return HipsSide;
                case BodyPart.LeftLeg:
                    return LeftLegSide;
                case BodyPart.RightLeg:
                    return RightLegSide;
                case BodyPart.Face:
                    return FaceSide;
                case BodyPart.Face_EyesClosed:
                    return FaceSide_EyesClosed;
                default:
                    throw new ArgumentOutOfRangeException(nameof(bodyPart), bodyPart, null);
            }
        }
        
        private Sprite GetBackPart(BodyPart bodyPart)
        {
            switch (bodyPart)
            {
                case BodyPart.Head:
                    return HeadBack;
                case BodyPart.Body:
                    return BodyBack;
                case BodyPart.MainHand:
                    return MainHandBack;
                case BodyPart.OffHand:
                    return OffHandBack;
                case BodyPart.Hips:
                    return HipsBack;
                case BodyPart.LeftLeg:
                    return LeftLegBack;
                case BodyPart.RightLeg:
                    return RightLegBack;
                default:
                    throw new ArgumentOutOfRangeException(nameof(bodyPart), bodyPart, null);
            }
        }
        
        private Sprite GetFrontPart(BodyPart bodyPart)
        {
            switch (bodyPart)
            {
                case BodyPart.Head:
                    return HeadFront;
                case BodyPart.Body:
                    return BodyFront;
                case BodyPart.MainHand:
                    return MainHandFront;
                case BodyPart.OffHand:
                    return OffHandFront;
                case BodyPart.Hips:
                    return HipsFront;
                case BodyPart.LeftLeg:
                    return LeftLegFront;
                case BodyPart.RightLeg:
                    return RightLegFront;
                case BodyPart.Face:
                    return FaceFront;
                case BodyPart.Face_EyesClosed:
                    return FaceFront_EyesClosed;
                default:
                    throw new ArgumentOutOfRangeException(nameof(bodyPart), bodyPart, null);
            }
        }
    }

    public enum BodyPart
    {
        Head,
        Body,
        MainHand,
        OffHand,
        Hips,
        LeftLeg,
        RightLeg,
        Face,
        Face_EyesClosed,
    }
}
