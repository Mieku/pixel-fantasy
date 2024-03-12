using System;
using System.Collections.Generic;
using Characters;
using Managers;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Rendering;

namespace Items
{
    [Serializable]
    public class Seat
    {
        public bool IsInUse;
        public Vector2 Position;
        public PlacementDirection Direction;
        public string claimedKinlingUID;

        public Seat(Vector2 position, PlacementDirection direction)
        {
            Position = position;
            Direction = direction;
            claimedKinlingUID = "";
            IsInUse = false;
        }

        public bool IsAvailable => !IsInUse && string.IsNullOrEmpty(claimedKinlingUID);
    }
    
    public class ChairFurniture : Furniture
    {
        [TitleGroup("General")] [SerializeField] protected Transform _usingParent;
        
        [TitleGroup("South")] [SerializeField] protected Transform _southTableCheckTransform;
        [TitleGroup("West")] [SerializeField] protected Transform _westTableCheckTransform;
        [TitleGroup("North")] [SerializeField] protected Transform _northTableCheckTransform;
        [TitleGroup("East")] [SerializeField] protected Transform _eastTableCheckTransform;

        private List<Seat> _seats = new List<Seat>();
        
        
        protected override void Built_Enter()
        {
            base.Built_Enter();
            GenerateSeats();
        }

        private void GenerateSeats()
        {
            _seats.Clear();
            
            foreach (var marker in _useageMarkers)
            {
                Seat seat = new Seat(marker.transform.position, _direction);
                _seats.Add(seat);
            }
        }

        private Seat GetKinlingsSeat(Kinling kinling)
        {
            foreach (var seat in _seats)
            {
                if (seat.claimedKinlingUID == kinling.UniqueId)
                {
                    return seat;
                }
            }

            return null;
        }

        public void EnterSeat(Kinling kinling)
        {
            var seat = GetKinlingsSeat(kinling);

            seat.IsInUse = true;
            kinling.KinlingAgent.TeleportToPosition(seat.Position, true);
            kinling.kinlingAnimController.SetUnitAction(UnitAction.Nothing, seat.Direction); // TODO: Add in sitting animation
            
            // Correct Layering
            kinling.transform.SetParent(_usingParent);
            kinling.AssignAndLockLayerOrder(GetSeatedLayerOrder());
            
            kinling.SetSeated(this);
        }

        private int GetSeatedLayerOrder()
        {
            var spritesSort = SpritesRoot().GetComponent<SortingGroup>().sortingOrder;
            return spritesSort + 1;
        }

        public void ExitSeat(Kinling kinling)
        {
            var seat = GetKinlingsSeat(kinling);
            seat.IsInUse = false;
            
            kinling.KinlingAgent.TeleportToPosition(seat.Position, false);
            kinling.SetSeated(null);
            
            kinling.transform.SetParent(KinlingsManager.Instance.transform);
            kinling.UnlockLayerOrder();
            
            kinling.kinlingAnimController.SetUnitAction(UnitAction.Nothing);
            
            UnclaimSeat(seat);
        }

        public Seat ClaimSeat(Kinling kinling)
        {
            var seat = GetAvailableSeat();
            seat.claimedKinlingUID = kinling.UniqueId;
            return seat;
        }

        public void UnclaimSeat(Seat seat)
        {
            seat.claimedKinlingUID = "";
        }

        private Seat GetAvailableSeat()
        {
            foreach (var seat in _seats)
            {
                if (seat.IsAvailable)
                {
                    return seat;
                }
            }

            return null;
        }
        
        public override bool CanKinlingUseThis()
        {
            bool result = base.CanKinlingUseThis();
            if (result)
            {
                var availableSeat = GetAvailableSeat();
                if (availableSeat != null)
                {
                    return true;
                }

                return false;
            }
            else
            {
                return false;
            }
        }
        
        public bool HasTable
        {
            get
            {
                var table = GetTable();
                return table != null;
            }
        }
        
        public TableFurniture GetTable()
        {
            Transform checkTrans;
            switch (_direction)
            {
                case PlacementDirection.South:
                    checkTrans = _southTableCheckTransform;
                    break;
                case PlacementDirection.North:
                    checkTrans = _northTableCheckTransform;
                    break;
                case PlacementDirection.West:
                    checkTrans = _westTableCheckTransform;
                    break;
                case PlacementDirection.East:
                    checkTrans = _eastTableCheckTransform;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            if (checkTrans == null)
            {
                return null;
            }

            TableFurniture table = Helper.GetObjectAtPosition<TableFurniture>(checkTrans.position);
            return table;
        }
    }
}
