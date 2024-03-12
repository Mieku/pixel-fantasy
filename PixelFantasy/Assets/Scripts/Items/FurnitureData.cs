// using System;
// using Managers;
// using ScriptableObjects;
// using Sirenix.OdinInspector;
// using UnityEngine;
//
// namespace Items
// {
//     [Serializable]
//     public class FurnitureData
//     {
//         [Serializable]
//         public enum EFurnitureState
//         {
//             Planning,
//             InProduction,
//             Built,
//         }
//         
//         [ShowInInspector] public string UniqueID { get; private set; }
//         [ShowInInspector] public EFurnitureState State { get; private set; }
//         [ShowInInspector] public FurnitureSettings Settings { get; private set; }
//         [ShowInInspector] public FurnitureVarient Variant { get; private set; }
//         [ShowInInspector] public DyeSettings DyeSettings { get; private set; }
//         [ShowInInspector] public float CurrentDurability { get; private set; }
//         [ShowInInspector] public float RemainingWork { get; set; }
//         [ShowInInspector] public string CraftersUID { get; set; }
//         [ShowInInspector] public PlacementDirection Direction { get; set; }
//         [ShowInInspector] public bool IsAllowed { get; set; }
//         [ShowInInspector] public bool InUse { get; set; }
//         
//         public FurnitureData(FurnitureSettings settings)
//             : this(settings, null, null) { }
//
//         // Constructor for furniture with variants and/or dye settings
//         public FurnitureData(FurnitureSettings settings, FurnitureVarient selectedVariant, DyeSettings selectedDyeSettings)
//         {
//             Initialize(settings, selectedVariant, selectedDyeSettings);
//         }
//
//         protected void Initialize(FurnitureSettings settings, FurnitureVarient selectedVariant, DyeSettings selectedDyeSettings)
//         {
//             Settings = settings;
//             Variant = selectedVariant;
//             DyeSettings = selectedDyeSettings;
//
//             var furnitureName = Variant != null ? Variant.VarientName : Settings.ItemName;
//             UniqueID = $"{furnitureName}_{Guid.NewGuid()}";
//             
//             // Set defaults
//             Direction = PlacementDirection.South;
//             IsAllowed = true;
//             InUse = false;
//             State = EFurnitureState.Planning;
//             
//             // Calculate durability and remaining work based on whether a variant is provided
//             CurrentDurability = MaxDurability;
//             RemainingWork = CraftRequirements.WorkCost;
//         }
//         
//         public float MaxDurability => Variant != null ? Variant.Durability : Settings.Durability;
//
//         public float DurabilityPercentage() => CurrentDurability / MaxDurability;
//
//         public CraftRequirements CraftRequirements => Variant != null ? Variant.CraftRequirements : Settings.CraftRequirements;
//
//         public void ChangeState(EFurnitureState state)
//         {
//             State = state;
//         }
//     }
// }
