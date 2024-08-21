// using Items;
// using Managers;
// using Systems.Appearance.Scripts;
// using UnityEngine;
//
// namespace TaskSystem
// {
//     public class DeconstructFurnitureAction : TaskAction // ID: Deconstruct Furniture
//     {
//         private Furniture _furniture;
//         private float _timer;
//         private Vector2? _movePos;
//         private bool _isDeconstructing;
//         
//         public override void PrepareAction(Task task)
//         {
//             _task = task;
//             _furniture = (Furniture)_task.Requestor;
//             _movePos = _furniture.UseagePosition(_ai.Kinling.transform.position);
//             _isDeconstructing = false;
//             
//             MoveToRequestor();
//         }
//
//         private void MoveToRequestor()
//         {
//             _ai.Kinling.KinlingAgent.SetMovePosition(_movePos, () =>
//             {
//                 KinlingAnimController.SetUnitAction(UnitAction.Swinging, _ai.GetActionDirection(_furniture.transform.position));
//                 _isDeconstructing = true;
//             }, OnTaskCancel);
//         }
//
//         public override void DoAction()
//         {
//             if (_isDeconstructing)
//             {
//                 DoDeconstruct();
//             }
//         }
//
//         private void DoDeconstruct()
//         {
//             _timer += TimeManager.Instance.DeltaTime;
//             if(_timer >= ActionSpeed) 
//             {
//                 _timer = 0;
//                 if (_furniture.DoDeconstruction(_ai.Kinling.Stats)) 
//                 {
//                     // When work is complete
//                     ConcludeAction();
//                 } 
//             }
//         }
//
//         public override void ConcludeAction()
//         {
//             base.ConcludeAction();
//             
//             KinlingAnimController.SetUnitAction(UnitAction.Nothing);
//             _furniture = null;
//             _task = null;
//             _movePos = null;
//             _isDeconstructing = false;
//         }
//
//         public override void OnTaskCancel()
//         {
//             base.OnTaskCancel();
//         }
//     }
// }
