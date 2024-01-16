using System;
using ScriptableObjects;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Characters
{
    [Serializable]
    public class KinlingGearData
    {
        [BoxGroup("Head")] public GearData HeadGearData;
        [ShowIf("@HeadGearData != null")] [BoxGroup("Head")] public DyePaletteData HeadGearDye;
        
        [BoxGroup("Body")] public GearData BodyGearData;
        [ShowIf("@BodyGearData != null")] [BoxGroup("Body")] public DyePaletteData BodyGearDye;
        
        [BoxGroup("Pants")] public GearData PantsGearData;
        [ShowIf("@PantsGearData != null")] [BoxGroup("Pants")] public DyePaletteData PantsGearDye;
        
        [BoxGroup("Hands")] public GearData HandsGearData;
        [ShowIf("@HandsGearData != null")] [BoxGroup("Hands")] public DyePaletteData HandsGearDye;
        
        [BoxGroup("Main Hand")] public GearData MainHandGearData;
        [ShowIf("@MainHandGearData != null")] [BoxGroup("Main Hand")] public DyePaletteData MainHandGearDye;
        
        [BoxGroup("Off Hand")] public GearData OffHandGearData;
        [ShowIf("@OffHandGearData != null")] [BoxGroup("Off Hand")] public DyePaletteData OffHandGearDye;

        public GearData GetGearData(GearType gearType)
        {
            switch (gearType)
            {
                case GearType.Head:
                    return HeadGearData;
                case GearType.Body:
                    return BodyGearData;
                case GearType.Pants:
                    return PantsGearData;
                case GearType.Hands:
                    return HandsGearData;
                case GearType.MainHand:
                    return MainHandGearData;
                case GearType.OffHand:
                    return OffHandGearData;
                case GearType.BothHands:
                    return MainHandGearData;
                case GearType.Necklace:
                case GearType.Ring:
                case GearType.Carried:
                    Debug.LogError("This gear type is not supported");
                    return null;
                default:
                    throw new ArgumentOutOfRangeException(nameof(gearType), gearType, null);
            }
        }
        
        public DyePaletteData GetDyePaletteData(GearType gearType)
        {
            switch (gearType)
            {
                case GearType.Head:
                    return HeadGearDye;
                case GearType.Body:
                    return BodyGearDye;
                case GearType.Pants:
                    return PantsGearDye;
                case GearType.Hands:
                    return HandsGearDye;
                case GearType.MainHand:
                    return MainHandGearDye;
                case GearType.OffHand:
                    return OffHandGearDye;
                case GearType.BothHands:
                    return MainHandGearDye;
                case GearType.Necklace:
                case GearType.Ring:
                case GearType.Carried:
                    Debug.LogError("This gear type is not supported");
                    return null;
                default:
                    throw new ArgumentOutOfRangeException(nameof(gearType), gearType, null);
            }
        }
    }
}
