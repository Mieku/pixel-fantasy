// using Items;
// using Managers;
// using Systems.Appearance.Scripts;
// using UnityEngine;
//
// namespace TaskSystem
// {
//     public class ExtractResourceAction : TaskAction // ID: Extract Resource
//     {
//         private BasicResource _resource;
//         private float _timer;
//         private UnitAction _actionAnimation;
//         private Vector2? _movePos;
//         private bool _isMoving;
//
//         private float DistanceFromRequestor => Vector2.Distance((Vector2)_movePos, transform.position);
//
//         public override bool CanDoTask(Task task)
//         {
//             var result = base.CanDoTask(task);
//             if (!result) return false;
//
//             _resource = (BasicResource)task.Requestor;
//             _movePos = _resource.UseagePosition(_ai.Kinling.transform.position);
//             
//             if (_movePos == null)
//             {
//                 return false;
//             }
//
//             return true;
//         }
//
//         public override void PrepareAction(Task task)
//         {
//             _task = task;
//             _actionAnimation = _resource.GetExtractActionAnim();
//         }
//         
//         public override void ConcludeAction()
//         {
//             base.ConcludeAction();
//             
//             KinlingAnimController.SetUnitAction(UnitAction.Nothing);
//             _resource = null;
//             _task = null;
//             _actionAnimation = UnitAction.Nothing;
//             _movePos = null;
//             _isMoving = false;
//         }
//         
//         public override void DoAction()
//         {
//             if (DistanceFromRequestor <= MIN_DISTANCE_FROM_REQUESTOR)
//             {
//                 DoExtraction();
//             }
//             else
//             {
//                 MoveToRequestor();
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
//         private void DoExtraction()
//         {
//             KinlingAnimController.SetUnitAction(_actionAnimation, _ai.GetActionDirection(_resource.transform.position));
//             
//             _timer += TimeManager.Instance.DeltaTime;
//             if(_timer >= ActionSpeed) 
//             {   
//                 _timer = 0;
//                 if (_resource.DoExtractionWork(_ai.Kinling.RuntimeData.Stats)) 
//                 {
//                     // When work is complete
//                     ConcludeAction();
//                 } 
//             }
//         }
//     }
// }
