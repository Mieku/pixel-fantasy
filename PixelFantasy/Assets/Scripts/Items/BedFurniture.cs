using System;
using System.Collections.Generic;
using Characters;
using Managers;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Rendering;

namespace Items
{
    public class BedFurniture : Furniture
    {
        public Transform UsingParent;
        
        private string _assignedKinling;
        private string _assignedKinling2;
        private List<string> _kinlingsInBed = new List<string>();

        public bool IsUnassigned(Unit unit)
        {
            if (string.IsNullOrEmpty(_assignedKinling))
            {
                return true;
            }
            else
            {
                if (unit.Partner != null && unit.Partner.UniqueId == _assignedKinling)
                {
                    return true;
                }
            }

            return false;
        }

        public void AssignKinling(Unit unit)
        {
            unit.AssignBed(this);
            if (string.IsNullOrEmpty(_assignedKinling))
            {
                _assignedKinling = unit.UniqueId;
            }
            else
            {
                _assignedKinling2 = unit.UniqueId;
            }
        }

        public bool IsDouble
        {
            get
            {
                switch (CurrentDirection)
                {
                    case PlacementDirection.South:
                        return _southSleepMarker2 != null;
                    case PlacementDirection.North:
                        return _northSleepMarker2 != null;
                    case PlacementDirection.West:
                        return _westSleepMarker2 != null;
                    case PlacementDirection.East:
                        return _eastSleepMarker2 != null;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }
        
        [TitleGroup("South")] [SerializeField] private SpriteRenderer _southTopSheet;
        [TitleGroup("South")] [SerializeField] private Transform _southSleepMarker;
        [TitleGroup("South")] [SerializeField] private Transform _southSleepMarker2;
        
        [TitleGroup("West")] [SerializeField] private SpriteRenderer _westTopSheet;
        [TitleGroup("West")] [SerializeField] private Transform _westSleepMarker;
        [TitleGroup("West")] [SerializeField] private Transform _westSleepMarker2;
        
        [TitleGroup("North")] [SerializeField] private SpriteRenderer _northTopSheet;
        [TitleGroup("North")] [SerializeField] private Transform _northSleepMarker;
        [TitleGroup("North")] [SerializeField] private Transform _northSleepMarker2;
        
        [TitleGroup("East")] [SerializeField] private SpriteRenderer _eastTopSheet;
        [TitleGroup("East")] [SerializeField] private Transform _eastSleepMarker;
        [TitleGroup("East")] [SerializeField] private Transform _eastSleepMarker2;

        public Transform GetSleepLocation(Unit unit)
        {
            if (_assignedKinling == unit.UniqueId)
            {
                return CurrentDirection switch
                {
                    PlacementDirection.South => _southSleepMarker,
                    PlacementDirection.North => _northSleepMarker,
                    PlacementDirection.West => _westSleepMarker,
                    PlacementDirection.East => _eastSleepMarker,
                    _ => throw new ArgumentOutOfRangeException()
                };
            }

            if (_assignedKinling2 == unit.UniqueId)
            {
                return CurrentDirection switch
                {
                    PlacementDirection.South => _southSleepMarker2,
                    PlacementDirection.North => _northSleepMarker2,
                    PlacementDirection.West => _westSleepMarker2,
                    PlacementDirection.East => _eastSleepMarker2,
                    _ => throw new ArgumentOutOfRangeException()
                };
            }
            
            Debug.LogError("Attempted to sleep in a bed that isn't assigned");
            return CurrentDirection switch
            {
                PlacementDirection.South => _southSleepMarker,
                PlacementDirection.North => _northSleepMarker,
                PlacementDirection.West => _westSleepMarker,
                PlacementDirection.East => _eastSleepMarker,
                _ => throw new ArgumentOutOfRangeException()
            };
        }
        
        public void ShowTopSheet(bool isShown)
        {
            if (!isShown)
            {
                _southTopSheet.gameObject.SetActive(false);
                _westTopSheet.gameObject.SetActive(false);
                _northTopSheet.gameObject.SetActive(false);
                _eastTopSheet.gameObject.SetActive(false);
            }
            else
            {
                switch (CurrentDirection)
                {
                    case PlacementDirection.South:
                        _southTopSheet.gameObject.SetActive(true);
                        _southTopSheet.sortingOrder = 2;
                        break;
                    case PlacementDirection.North:
                        _northTopSheet.gameObject.SetActive(true);
                        _northTopSheet.sortingOrder = 2;
                        break;
                    case PlacementDirection.West:
                        _westTopSheet.gameObject.SetActive(true);
                        _westTopSheet.sortingOrder = 2;
                        break;
                    case PlacementDirection.East:
                        _eastTopSheet.gameObject.SetActive(true);
                        _eastTopSheet.sortingOrder = 2;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }

        public void EnterBed(Unit unit)
        {
            unit.transform.SetParent(UsingParent);
            unit.IsAsleep = true;
            ShowTopSheet(true);
            int orderlayer = GetBetweenTheSheetsLayerOrder();
            unit.AssignAndLockLayerOrder(orderlayer);
            
            _kinlingsInBed.Add(unit.UniqueId);
        }

        public void ExitBed(Unit unit)
        {
            unit.transform.SetParent(UnitsManager.Instance.transform);
            unit.IsAsleep = false;
            ShowTopSheet(false);
            unit.UnlockLayerOrder();
            
            _kinlingsInBed.Remove(unit.UniqueId);
        }

        public bool IsPartnerInBed(Unit kinling)
        {
            if (kinling.Partner == null) return false;

            string partnerID = kinling.Partner.UniqueId;
            return _kinlingsInBed.Contains(partnerID);
        }

        public int GetBetweenTheSheetsLayerOrder()
        {
            return 1;
        }
    }
}
