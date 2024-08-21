// using Managers;
// using Systems.Appearance.Scripts;
// using Systems.Floors.Scripts;
// using UnityEngine;
//
// namespace TaskSystem
// {
//     public class ClearGrassAction : TaskAction // ID: Clear Grass
//     {
//         private Dirt _dirt;
//         private float _timer;
//         private Vector2? _movePos;
//         private bool _isMoving;
//
//         private float DistanceFromRequestor => Vector2.Distance((Vector2)_movePos, transform.position);
//         
//         public override void PrepareAction(Task task)
//         {
//             _task = task;
//             _dirt = (Dirt)task.Requestor;
//             _movePos = _dirt.UseagePosition(_ai.Kinling.transform.position);
//         }
//
//         public override void DoAction()
//         {
//             if (DistanceFromRequestor <= MIN_DISTANCE_FROM_REQUESTOR)
//             {
//                 DoClearGrass();
//             }
//             else
//             {
//                 MoveToRequestor();
//             }
//         }
//         
//         private void DoClearGrass()
//         {
//             KinlingAnimController.SetUnitAction(UnitAction.Digging, _ai.GetActionDirection(_dirt.transform.position));
//             
//             _timer += TimeManager.Instance.DeltaTime;
//             if(_timer >= ActionSpeed) 
//             {
//                 _timer = 0;
//                 if (_dirt.DoConstruction(_ai.Kinling.Stats))
//                 {
//                     // When work is complete
//                     ConcludeAction();
//                 } 
//             }
//         }
//         
//         private void MoveToRequestor()
//         {
//             if (!_isMoving)
//             {
//                 _ai.Kinling.KinlingAgent.SetMovePosition((Vector2)_movePos);
//                 _isMoving = true;
//             }
//         }
//
//         public override void ConcludeAction()
//         {
//             base.ConcludeAction();
//             
//             KinlingAnimController.SetUnitAction(UnitAction.Nothing);
//             _dirt = null;
//             _task = null;
//             _movePos = null;
//             _isMoving = false;
//         }
//     }
// }
