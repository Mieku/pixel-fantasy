using System;
using Managers;
using ScriptableObjects;
using UnityEngine;
using UnityEngine.Rendering;

namespace Characters
{
    public class UnitAppearance : MonoBehaviour
    {
        [SerializeField] private KinlingEquipment _equipment;
        
        public HairData HairData;
        public FaceData FaceData;
        public BodyData BodyData;
        public Gender Gender;
        
        [SerializeField] private SpriteRenderer _hairRenderer;
        [SerializeField] private SpriteRenderer _headRenderer;
        [SerializeField] private SpriteRenderer _faceRanderer;

        [SerializeField] private SpriteRenderer _outerHandRenderer;
        [SerializeField] private SpriteRenderer _backHandRenderer;

        [SerializeField] private SpriteRenderer _leftLegRenderer;
        [SerializeField] private SpriteRenderer _rightLegRenderer;
        [SerializeField] private SpriteRenderer _bodyNudeRenderer;

        [SerializeField] private SortingGroup _mainHandSortingGroup;
        [SerializeField] private SortingGroup _mainHandHeldSortingGroup;
        [SerializeField] private SortingGroup _offHandSortingGroup;

        private const int OuterHandSideLayer = 5;
        private const int OuterHandFrontLayer = 5;
        private const int OuterHandBackLayer = -1;
        
        private const int BackHandSideLayer = -1;
        private const int BackHandFrontLayer = 5;
        private const int BackHandBackLayer = -1;
        
        private const int ToolSideLayer = 4;
        private const int ToolFrontLayer = 4;
        private const int ToolBackLayer = -2;

        private UnitActionDirection _curDirection;

        private void Start()
        {
            SetGender(Gender);
            Refresh();
        }

        public void Refresh()
        {
            SetDirection(_curDirection);
        }
        
        private void SetGender(Gender gender)
        {
            Gender = gender;
        }

        public void SetDirection(UnitActionDirection dir)
        {
            _curDirection = dir;
            
            // Set Layers and Enable/Disable
            switch (dir)
            {
                case UnitActionDirection.Side:
                    _faceRanderer.gameObject.SetActive(true);
                    _mainHandSortingGroup.sortingOrder = OuterHandSideLayer;
                    _mainHandHeldSortingGroup.sortingOrder = ToolSideLayer;
                    _offHandSortingGroup.sortingOrder = BackHandSideLayer;
                    break;
                case UnitActionDirection.Up:
                    _faceRanderer.gameObject.SetActive(false);
                    _mainHandSortingGroup.sortingOrder = OuterHandBackLayer;
                    _mainHandHeldSortingGroup.sortingOrder = ToolBackLayer;
                    _offHandSortingGroup.sortingOrder = BackHandBackLayer;
                    break;
                case UnitActionDirection.Down:
                    _faceRanderer.gameObject.SetActive(true);
                    _mainHandSortingGroup.sortingOrder = OuterHandFrontLayer;
                    _mainHandHeldSortingGroup.sortingOrder = ToolFrontLayer;
                    _offHandSortingGroup.sortingOrder = BackHandFrontLayer;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(dir), dir, null);
            }
            
            SetHairDirection(dir);
            SetBodyDirection(dir);
            _equipment.AssignDirection(dir);
        }
        
        private void SetHairDirection(UnitActionDirection dir)
        {
            _hairRenderer.sprite = dir switch
            {
                UnitActionDirection.Side => HairData.Side,
                UnitActionDirection.Up => HairData.Back,
                UnitActionDirection.Down => HairData.Front,
                _ => throw new ArgumentOutOfRangeException(nameof(dir), dir, null)
            };
        }
        
        private void SetBodyDirection(UnitActionDirection dir)
        {
            // Face
            _faceRanderer.sprite = dir switch
            {
                UnitActionDirection.Side => FaceData.Side,
                UnitActionDirection.Up => FaceData.Front,
                UnitActionDirection.Down => FaceData.Front,
                _ => throw new ArgumentOutOfRangeException(nameof(dir), dir, null)
            };
            
            // Head
            _headRenderer.sprite = dir switch
            {
                UnitActionDirection.Side => BodyData.HeadSide,
                UnitActionDirection.Up => BodyData.HeadFront,
                UnitActionDirection.Down => BodyData.HeadFront,
                _ => throw new ArgumentOutOfRangeException(nameof(dir), dir, null)
            };
            
            // Torso
            _bodyNudeRenderer.sprite = dir switch
            {
                UnitActionDirection.Side => BodyData.BodyNudeSide,
                UnitActionDirection.Up => BodyData.BodyNudeFront,
                UnitActionDirection.Down => BodyData.BodyNudeFront,
                _ => throw new ArgumentOutOfRangeException(nameof(dir), dir, null)
            };
            
            // Outer Hand
            _outerHandRenderer.sprite = dir switch
            {
                UnitActionDirection.Side => BodyData.OuterHand,
                UnitActionDirection.Up => BodyData.OuterHand,
                UnitActionDirection.Down => BodyData.OuterHand,
                _ => throw new ArgumentOutOfRangeException(nameof(dir), dir, null)
            };
            
            // Back Hand
            _backHandRenderer.sprite = dir switch
            {
                UnitActionDirection.Side => BodyData.BackHand,
                UnitActionDirection.Up => BodyData.BackHand,
                UnitActionDirection.Down => BodyData.BackHand,
                _ => throw new ArgumentOutOfRangeException(nameof(dir), dir, null)
            };
            
            // Left Leg
            _leftLegRenderer.sprite = dir switch
            {
                UnitActionDirection.Side => BodyData.LeftLeg,
                UnitActionDirection.Up => BodyData.LeftLegFront,
                UnitActionDirection.Down => BodyData.LeftLegFront,
                _ => throw new ArgumentOutOfRangeException(nameof(dir), dir, null)
            };
            
            // Right Leg
            _rightLegRenderer.sprite = dir switch
            {
                UnitActionDirection.Side => BodyData.RightLeg,
                UnitActionDirection.Up => BodyData.RightLeg,
                UnitActionDirection.Down => BodyData.RightLeg,
                _ => throw new ArgumentOutOfRangeException(nameof(dir), dir, null)
            };
        }

        // private void SetClothingDirection(UnitActionDirection dir)
        // {
        //     // Top
        //     _bodyRenderer.sprite = dir switch
        //     {
        //         UnitActionDirection.Side => ClothingData.Side,
        //         UnitActionDirection.Up => ClothingData.Back,
        //         UnitActionDirection.Down => ClothingData.Front,
        //         _ => throw new ArgumentOutOfRangeException(nameof(dir), dir, null)
        //     };
        //     
        //     // Pants
        //     _hipsRenderer.sprite = dir switch
        //     {
        //         UnitActionDirection.Side => ClothingData.PantsSide,
        //         UnitActionDirection.Up => ClothingData.PantsFront,
        //         UnitActionDirection.Down => ClothingData.PantsFront,
        //         _ => throw new ArgumentOutOfRangeException(nameof(dir), dir, null)
        //     };
        // }
    
        public void SetLoadData(AppearanceData data)
        {
            HairData = Librarian.Instance.GetHairData(data.Hair);
            SetGender(data.Gender);
        }

        public AppearanceData GetSaveData()
        {
            return new AppearanceData
            {
                Hair = HairData.Name,
                Gender = Gender,
            };
        }

        public struct AppearanceData
        {
            public string Hair;
            public Gender Gender;
        }
    }

    public enum Gender
    {
        Male,
        Female
    }
}
