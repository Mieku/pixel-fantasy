// using System;
// using System.Collections.Generic;
// using Characters;
// using DataPersistence;
// using UnityEngine;
//
// namespace Handlers
// {
//     public class KinlingsHandler : Saveable
//     {
//         protected override string StateName => "Units";
//         public override int LoadOrder => 2;
//
//         private List<Kinling> _units = new List<Kinling>();
//
//         [SerializeField] private GameObject _unitPrefab;
//
//         private void Start()
//         {
//             var currentChildren = GetPersistentChildren();
//             foreach (var child in currentChildren)
//             {
//                 var unit = child.GetComponent<Kinling>();
//                 if (unit != null)
//                 {
//                     _units.Add(unit);
//                 }
//             }
//         }
//
//         public void AddUnit(Kinling kinling)
//         {
//             _units.Add(kinling);
//         }
//
//         public List<Kinling> GetAllUnits()
//         {
//             return _units;
//         }
//
//         // protected override void ClearChildStates(List<object> childrenStates)
//         // {
//         //     // Delete current persistent children
//         //     var currentChildren = GetPersistentChildren();
//         //     foreach (var child in currentChildren)
//         //     {
//         //         child.GetComponent<UID>().RemoveUID();
//         //     }
//         //     
//         //     foreach (var child in currentChildren)
//         //     {
//         //         Destroy(child);
//         //     }
//         //     currentChildren.Clear();
//         // }
//         //
//         // protected override void SetChildStates(List<object> childrenStates)
//         // {
//         //     // Instantiate all the children in data, Trigger RestoreState with their state data
//         //     foreach (var childState in childrenStates)
//         //     {
//         //         var data = (Characters.Kinling.UnitData)childState;
//         //         var childObj = Instantiate(_unitPrefab, transform);
//         //         childObj.GetComponent<IPersistent>().RestoreState(data);
//         //     }
//         // }
//     }
// }
