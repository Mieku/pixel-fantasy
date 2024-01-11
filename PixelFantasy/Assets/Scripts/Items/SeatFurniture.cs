using System;
using Characters;
using Managers;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Items
{
    public class SeatFurniture : Furniture
    {
        [TitleGroup("South")] [SerializeField] protected Transform _southTableCheckTransform;
        [TitleGroup("West")] [SerializeField] protected Transform _westTableCheckTransform;
        [TitleGroup("North")] [SerializeField] protected Transform _northTableCheckTransform;
        [TitleGroup("East")] [SerializeField] protected Transform _eastTableCheckTransform;

        private bool _isInUse;
        private bool _isClaimed;

        public void EnterSeat(Unit kinling)
        {
            _isInUse = true;
            kinling.UnitAgent.TeleportToPosition(UseagePosition().position, true);
        }

        public void ExitSeat(Unit kinling)
        {
            _isInUse = false;
            kinling.UnitAgent.TeleportToPosition(UseagePosition().position, false);
            UnclaimSeat();
        }

        public void ClaimSeat()
        {
            _isClaimed = true;
        }

        public void UnclaimSeat()
        {
            _isClaimed = false;
        }
        
        public override bool CanKinlingUseThis()
        {
            bool result = base.CanKinlingUseThis();
            if (result)
            {
                return !_isInUse && !_isClaimed;
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
            switch (CurrentDirection)
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
