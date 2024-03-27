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
        
        private KinlingData _assignedKinling;
        private KinlingData _assignedKinling2;
        private List<KinlingData> _kinlingsInBed = new List<KinlingData>();

        public bool IsUnassigned(Kinling kinling)
        {
            if (_assignedKinling == null)
            {
                return true;
            }
            else
            {
                if (kinling.RuntimeData.Partner != null && kinling.RuntimeData.Partner == _assignedKinling)
                {
                    return true;
                }
            }

            return false;
        }

        public void AssignKinling(Kinling kinling)
        {
            kinling.AssignBed(this);
            if (_assignedKinling == null)
            {
                _assignedKinling = kinling.RuntimeData;
            }
            else
            {
                _assignedKinling2 = kinling.RuntimeData;
            }
        }

        public bool IsDouble
        {
            get
            {
                switch (_direction)
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

        public Transform GetSleepLocation(Kinling kinling)
        {
            if (_assignedKinling == kinling.RuntimeData)
            {
                return _direction switch
                {
                    PlacementDirection.South => _southSleepMarker,
                    PlacementDirection.North => _northSleepMarker,
                    PlacementDirection.West => _westSleepMarker,
                    PlacementDirection.East => _eastSleepMarker,
                    _ => throw new ArgumentOutOfRangeException()
                };
            }

            if (_assignedKinling2 == kinling.RuntimeData)
            {
                return _direction switch
                {
                    PlacementDirection.South => _southSleepMarker2,
                    PlacementDirection.North => _northSleepMarker2,
                    PlacementDirection.West => _westSleepMarker2,
                    PlacementDirection.East => _eastSleepMarker2,
                    _ => throw new ArgumentOutOfRangeException()
                };
            }
            
            Debug.LogError("Attempted to sleep in a bed that isn't assigned");
            return _direction switch
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
                switch (_direction)
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

        public void EnterBed(Kinling kinling)
        {
            kinling.transform.SetParent(UsingParent);
            kinling.RuntimeData.IsAsleep = true;
            ShowTopSheet(true);
            int orderlayer = GetBetweenTheSheetsLayerOrder();
            kinling.AssignAndLockLayerOrder(orderlayer);
            
            _kinlingsInBed.Add(kinling.RuntimeData);
        }

        public void ExitBed(Kinling kinling)
        {
            kinling.transform.SetParent(KinlingsManager.Instance.transform);
            kinling.RuntimeData.IsAsleep = false;
            ShowTopSheet(false);
            kinling.UnlockLayerOrder();
            
            _kinlingsInBed.Remove(kinling.RuntimeData);
        }

        public bool IsPartnerInBed(Kinling kinling)
        {
            if (kinling.RuntimeData.Partner == null) return false;
            
            return _kinlingsInBed.Contains(kinling.RuntimeData.Partner);
        }

        public int GetBetweenTheSheetsLayerOrder()
        {
            return 1;
        }
    }
}
