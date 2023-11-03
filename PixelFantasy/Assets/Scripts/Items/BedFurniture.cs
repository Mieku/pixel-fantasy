using System;
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
        
        [TitleGroup("South")] [SerializeField] private SpriteRenderer _southTopSheet;
        [TitleGroup("South")] [SerializeField] private Transform _southSleepMarker;
        
        [TitleGroup("West")] [SerializeField] private SpriteRenderer _westTopSheet;
        [TitleGroup("West")] [SerializeField] private Transform _westSleepMarker;
        
        [TitleGroup("North")] [SerializeField] private SpriteRenderer _northTopSheet;
        [TitleGroup("North")] [SerializeField] private Transform _northSleepMarker;
        
        [TitleGroup("East")] [SerializeField] private SpriteRenderer _eastTopSheet;
        [TitleGroup("East")] [SerializeField] private Transform _eastSleepMarker;

        public Transform SleepLocation
        {
            get
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
            AssignFurnitureToKinling(unit);
            ShowTopSheet(true);
            int orderlayer = GetBetweenTheSheetsLayerOrder();
            unit.AssignAndLockLayerOrder(orderlayer);
        }

        public void ExitBed(Unit unit)
        {
            unit.transform.SetParent(UnitsManager.Instance.transform);
            unit.IsAsleep = false;
            ShowTopSheet(false);
            unit.UnlockLayerOrder();
        }

        public int GetBetweenTheSheetsLayerOrder()
        {
            return 1;
        }
    }
}
