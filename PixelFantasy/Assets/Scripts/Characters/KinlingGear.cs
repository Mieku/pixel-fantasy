// using System;
// using ScriptableObjects;
// using Sirenix.OdinInspector;
// using UnityEngine;
// using UnityEngine.Serialization;
//
// namespace Characters
// {
//     [Serializable]
//     public class KinlingGear
//     {
//         [FormerlySerializedAs("HeadGearData")] [BoxGroup("Head")] public GearSettings HeadGearSettings;
//         [ShowIf("@HeadGearData != null && HeadGearData.CanBeDyed")] [BoxGroup("Head")] public DyeSettings HeadGearDye;
//         
//         [FormerlySerializedAs("BodyGearData")] [BoxGroup("Body")] public GearSettings BodyGearSettings;
//         [ShowIf("@BodyGearData != null && BodyGearData.CanBeDyed")] [BoxGroup("Body")] public DyeSettings BodyGearDye;
//         
//         [FormerlySerializedAs("PantsGearData")] [BoxGroup("Pants")] public GearSettings PantsGearSettings;
//         [ShowIf("@PantsGearData != null && PantsGearData.CanBeDyed")] [BoxGroup("Pants")] public DyeSettings PantsGearDye;
//         
//         [FormerlySerializedAs("HandsGearData")] [BoxGroup("Hands")] public GearSettings HandsGearSettings;
//         [ShowIf("@HandsGearData != null && HandsGearData.CanBeDyed")] [BoxGroup("Hands")] public DyeSettings HandsGearDye;
//         
//         [FormerlySerializedAs("MainHandGearData")] [BoxGroup("Main Hand")] public GearSettings MainHandGearSettings;
//         [ShowIf("@MainHandGearData != null && MainHandGearData.CanBeDyed")] [BoxGroup("Main Hand")] public DyeSettings MainHandGearDye;
//         
//         [FormerlySerializedAs("OffHandGearData")] [BoxGroup("Off Hand")] public GearSettings OffHandGearSettings;
//         [ShowIf("@OffHandGearData != null && OffHandGearData.CanBeDyed")] [BoxGroup("Off Hand")] public DyeSettings OffHandGearDye;
//         
//         [FormerlySerializedAs("RingGearData")] [BoxGroup("Ring")] public GearSettings RingGearSettings;
//         
//         [FormerlySerializedAs("NecklaceGearData")] [BoxGroup("Necklace")] public GearSettings NecklaceGearSettings;
//
//         public GearSettings GetGearData(GearType gearType)
//         {
//             switch (gearType)
//             {
//                 case GearType.Head:
//                     return HeadGearSettings;
//                 case GearType.Body:
//                     return BodyGearSettings;
//                 case GearType.Pants:
//                     return PantsGearSettings;
//                 case GearType.Hands:
//                     return HandsGearSettings;
//                 case GearType.MainHand:
//                     return MainHandGearSettings;
//                 case GearType.OffHand:
//                     return OffHandGearSettings;
//                 case GearType.Necklace:
//                     return NecklaceGearSettings;
//                 case GearType.Ring:
//                     return RingGearSettings;
//                 case GearType.Carried:
//                     Debug.LogError("This gear type is not supported");
//                     return null;
//                 default:
//                     throw new ArgumentOutOfRangeException(nameof(gearType), gearType, null);
//             }
//         }
//         
//         public DyeSettings GetDyePaletteData(GearType gearType)
//         {
//             switch (gearType)
//             {
//                 case GearType.Head:
//                     return HeadGearDye;
//                 case GearType.Body:
//                     return BodyGearDye;
//                 case GearType.Pants:
//                     return PantsGearDye;
//                 case GearType.Hands:
//                     return HandsGearDye;
//                 case GearType.MainHand:
//                     return MainHandGearDye;
//                 case GearType.OffHand:
//                     return OffHandGearDye;
//                 case GearType.Necklace:
//                 case GearType.Ring:
//                 case GearType.Carried:
//                     Debug.LogError("This gear type is not supported");
//                     return null;
//                 default:
//                     throw new ArgumentOutOfRangeException(nameof(gearType), gearType, null);
//             }
//         }
//     }
// }
