using System;
using JetBrains.Annotations;
using Managers;
using ScriptableObjects;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Rendering;

namespace Characters
{
    public class UnitAppearance : MonoBehaviour
    {
        [SerializeField] private KinlingEquipment _equipment;
        
        public HairData HairData;
        public BodyData BodyData;
        [SerializeField] private KinlingFaceHandler _face;

        [SerializeField] private SpriteRenderer _hairRenderer;
        [SerializeField] private SpriteRenderer _headRenderer;

        [SerializeField] private SpriteRenderer _outerHandRenderer;
        [SerializeField] private SpriteRenderer _backHandRenderer;

        [SerializeField] private SpriteRenderer _leftLegRenderer;
        [SerializeField] private SpriteRenderer _rightLegRenderer;
        [SerializeField] private SpriteRenderer _bodyNudeRenderer;
        [SerializeField] private SpriteRenderer _hipsRenderer;

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
        
        private readonly int colorSwapRed = Shader.PropertyToID("_ColorSwapRed");
        private readonly int colorSwapGreen = Shader.PropertyToID("_ColorSwapGreen");
        private readonly int colorSwapBlue = Shader.PropertyToID("_ColorSwapBlue");
        private readonly int cwRedLumin = Shader.PropertyToID("_ColorSwapRedLuminosity");
        private readonly int cwGreenLumin = Shader.PropertyToID("_ColorSwapGreenLuminosity");
        private readonly int cwBlueLumin = Shader.PropertyToID("_ColorSwapBlueLuminosity");

        private UnitActionDirection _curDirection;
        [CanBeNull] private AppearanceState _appearanceState;
        private Unit _unit;

        private void Start()
        {
            
        }

        public void Init(Unit unit)
        {
            _unit = unit;
            if (_appearanceState == null)
            {
                // Create a random one
                var randomAppearance = new AppearanceState(_unit.Race, _unit.Gender);
                randomAppearance.RandomizeAppearance();
                _appearanceState = randomAppearance;
            }
            ApplyAppearanceState(_appearanceState);
            _face.Init(BodyData, _curDirection);
        }

        public void ApplyAppearanceState(AppearanceState appearanceState)
        {
            _appearanceState = appearanceState;
            ApplySkinTone();
        }

        public AppearanceState GetAppearanceState()
        {
            return _appearanceState;
        }

        [Button("Apply Skin Tone")]
        private void ApplySkinTone()
        {
            var eyeColour = _appearanceState.EyeColour;
            var blushTone = _appearanceState.SkinTone.BlushTone;
            var shadeTone = _appearanceState.SkinTone.ShadeTone;
            var primaryTone = _appearanceState.SkinTone.PrimaryTone;
            
            // Face
            _face.AssignFaceColours(eyeColour, blushTone);
            //ApplyMaterialRecolour(_faceRanderer, eyeColour, blushTone, Color.black);
            
            // Head
            ApplyMaterialRecolour(_headRenderer, primaryTone, shadeTone, Color.black);
            
            // Body
            ApplyMaterialRecolour(_bodyNudeRenderer, primaryTone, shadeTone, Color.black);
            
            // Hips
            ApplyMaterialRecolour(_hipsRenderer, primaryTone, shadeTone, Color.black);
            
            // Arms
            ApplyMaterialRecolour(_outerHandRenderer, primaryTone, shadeTone, Color.black);
            ApplyMaterialRecolour(_backHandRenderer, primaryTone, shadeTone, Color.black);

            // Legs
            ApplyMaterialRecolour(_leftLegRenderer, primaryTone, shadeTone, Color.black);
            ApplyMaterialRecolour(_rightLegRenderer, primaryTone, shadeTone, Color.black);

        }

        private void ApplyMaterialRecolour(SpriteRenderer spriteRenderer, Color redSwap, Color greenSwap, Color blueSwap)
        {
            var mat = spriteRenderer.sharedMaterial;
            mat.EnableKeyword("COLORSWAP_ON");
            mat.SetTexture("_ColorSwapTex", spriteRenderer.sprite.texture);
            
            mat.SetColor(colorSwapRed, redSwap);
            mat.SetColor(colorSwapGreen, greenSwap);
            mat.SetColor(colorSwapBlue, blueSwap);
            
            mat.SetFloat(cwRedLumin, 1.0f);
            mat.SetFloat(cwGreenLumin, 1.0f);
            mat.SetFloat(cwBlueLumin, 1.0f);
        }

        public void Refresh()
        {
            SetDirection(_curDirection);
        }
        
        public void SetDirection(UnitActionDirection dir)
        {
            _curDirection = dir;

            if (_unit == null) return;
            
            // Set Layers and Enable/Disable
            switch (dir)
            {
                case UnitActionDirection.Side:
                    // _faceRanderer.gameObject.SetActive(true);
                    _mainHandSortingGroup.sortingOrder = OuterHandSideLayer;
                    _mainHandHeldSortingGroup.sortingOrder = ToolSideLayer;
                    _offHandSortingGroup.sortingOrder = BackHandSideLayer;
                    break;
                case UnitActionDirection.Up:
                    // _faceRanderer.gameObject.SetActive(false);
                    _mainHandSortingGroup.sortingOrder = OuterHandBackLayer;
                    _mainHandHeldSortingGroup.sortingOrder = ToolBackLayer;
                    _offHandSortingGroup.sortingOrder = BackHandBackLayer;
                    break;
                case UnitActionDirection.Down:
                    // _faceRanderer.gameObject.SetActive(true);
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
            _face.SetDirection(dir);
            // if (dir != UnitActionDirection.Up)
            // {
            //     _faceRanderer.sprite = BodyData.GetBodySprite(BodyPart.Face, dir);
            // }
            _headRenderer.sprite = BodyData.GetBodySprite(BodyPart.Head, dir);
            _bodyNudeRenderer.sprite = BodyData.GetBodySprite(BodyPart.Body, dir);
            _outerHandRenderer.sprite = BodyData.GetBodySprite(BodyPart.MainHand, dir);
            _backHandRenderer.sprite = BodyData.GetBodySprite(BodyPart.OffHand, dir);
            _leftLegRenderer.sprite = BodyData.GetBodySprite(BodyPart.LeftLeg, dir);
            _rightLegRenderer.sprite = BodyData.GetBodySprite(BodyPart.RightLeg, dir);
            _hipsRenderer.sprite = BodyData.GetBodySprite(BodyPart.Hips, dir);

            ApplySkinTone();
        }
    
        public void SetLoadData(AppearanceData data)
        {
            HairData = Librarian.Instance.GetHairData(data.Hair);
            //SetGender(data.Gender);
        }

        public AppearanceData GetSaveData()
        {
            return new AppearanceData
            {
                Hair = HairData.Name,
               // Gender = Gender,
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
