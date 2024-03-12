// using System;
// using Characters;
// using Managers;
// using ScriptableObjects;
// using UnityEngine;
// using UnityEngine.Serialization;
//
// namespace Items
// {
//     [Serializable]
//     public class ItemState
//     {
//         [FormerlySerializedAs("Data")] public ItemSettings Settings;
//         public int Durability;
//         public string UID;
//         public Storage Storage => LinkedItem.AssignedStorage;
//         public string CraftersUID;
//
//         protected Item _linkedItem;
//         public Item LinkedItem
//         {
//             get
//             {
//                 // Return the linked item if there is one, if not spawn one
//                 if (_linkedItem == null) _linkedItem = Spawner.Instance.SpawnItem(Settings, Vector3.zero, false, this);
//                 
//                 return _linkedItem;
//             }
//         }
//
//         public ItemState(ItemSettings settings, string uid, Item linkedItem)
//         {
//             Settings = settings;
//             Durability = Settings.Durability;
//             UID = uid;
//             _linkedItem = linkedItem;
//         }
//
//         public ItemState(ItemState other)
//         {
//             Settings = other.Settings;
//             Durability = other.Durability;
//             UID = other.UID;
//             _linkedItem = other.LinkedItem;
//         }
//
//         public float DurabilityPercentage()
//         {
//             float percent = (float)Durability / Settings.Durability;
//             return percent;
//         }
//
//         public override bool Equals(object obj)
//         {
//             if (obj == null || GetType() != obj.GetType())
//             {
//                 return false;
//             }
//
//             ItemState other = (ItemState)obj;
//             return UID == other.UID;
//         }
//
//         public override int GetHashCode()
//         {
//             return UID.GetHashCode();
//         }
//
//         public Kinling GetCrafter()
//         {
//             if (!WasCrafted) return null;
//
//             return KinlingsManager.Instance.GetUnit(CraftersUID);
//         }
//
//         public bool WasCrafted => !string.IsNullOrEmpty(CraftersUID);
//     }
// }
