// using Items;
// using Managers;
// using Systems.Appearance.Scripts;
// using UnityEngine;
//
// namespace TaskSystem
// {
//     public class HarvestResourceAction : TaskAction // ID: Harvest Resource
//     {
//         private GrowingResource _resource;
//         private float _timer;
//         private UnitAction _actionAnimation;
//         private Vector2? _movePos;
//         private bool _isMoving;
//
//         private float DistanceFromRequestor => Vector2.Distance((Vector2)_movePos, transform.position);
//
//         public override void PrepareAction(Task task)
//         {
//             _task = task;
//             _resource = (GrowingResource)task.Requestor;
//             _actionAnimation = UnitAction.Doing;
//             _movePos = _resource.UseagePosition(_ai.Kinling.transform.position);
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
//                 DoHarvest();
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
//         private void DoHarvest()
//         {
//             KinlingAnimController.SetUnitAction(_actionAnimation, _ai.GetActionDirection(_resource.transform.position));
//             
//             _timer += TimeManager.Instance.DeltaTime;
//             if(_timer >= ActionSpeed) 
//             {
//                 _timer = 0;
//                 if (_resource.DoHarvest(_ai.Kinling.Stats)) 
//                 {
//                     // When work is complete
//                     ConcludeAction();
//                 } 
//             }
//         }
//     }
// }
