using System;
using JetBrains.Annotations;
using Managers;
using ScriptableObjects;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Serialization;

namespace Characters
{
    public class KinlingAppearance : MonoBehaviour
    {
        //[SerializeField] private KinlingEquipment _equipment;
        
        [FormerlySerializedAs("HairData")] public HairSettings HairSettings;
        [FormerlySerializedAs("BodyData")] public BodySettings BodySettings;
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
        private Kinling _kinling;

        private void Start()
        {
            
        }

        public void Init(Kinling kinling, AppearanceState appearanceState)
        {
            _kinling = kinling;
            if (appearanceState == null)
            {
                // Create a random one
                var randomAppearance = new AppearanceState(_kinling.RuntimeData.Race, _kinling.RuntimeData.Gender);
                randomAppearance.RandomizeAppearance();
                _appearanceState = randomAppearance;
            }
            else
            {
                _appearanceState = appearanceState;
            }
            
            ApplyAppearanceState(_appearanceState);
            _face.Init(BodySettings, _curDirection);
        }

        public void SetEyesClosed(bool setClosed)
        {
            _face.SetForcedEyesClosed(setClosed);
        }

        public void ApplyAppearanceState(AppearanceState appearanceState)
        {
            _appearanceState = appearanceState;
            BodySettings = appearanceState.Race.GetBodyDataByMaturity(_kinling.RuntimeData.MaturityStage);
            HairSettings = appearanceState.Hair;
            ApplySkinTone();
            ApplyHair();
        }

        public AppearanceState GetAppearanceState()
        {
            return _appearanceState;
        }

        private void ApplyHair()
        {
            SetHairDirection(_curDirection);
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
            var mat = spriteRenderer.material;
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

            if (_kinling == null) return;
            
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
            //_equipment.AssignDirection(dir);
        }
        
        private void SetHairDirection(UnitActionDirection dir)
        {
            _hairRenderer.sprite = dir switch
            {
                UnitActionDirection.Side => HairSettings.Side,
                UnitActionDirection.Up => HairSettings.Back,
                UnitActionDirection.Down => HairSettings.Front,
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
            _headRenderer.sprite = BodySettings.GetBodySprite(BodyPart.Head, dir);
            _bodyNudeRenderer.sprite = BodySettings.GetBodySprite(BodyPart.Body, dir);
            _outerHandRenderer.sprite = BodySettings.GetBodySprite(BodyPart.MainHand, dir);
            _backHandRenderer.sprite = BodySettings.GetBodySprite(BodyPart.OffHand, dir);
            _leftLegRenderer.sprite = BodySettings.GetBodySprite(BodyPart.LeftLeg, dir);
            _rightLegRenderer.sprite = BodySettings.GetBodySprite(BodyPart.RightLeg, dir);
            _hipsRenderer.sprite = BodySettings.GetBodySprite(BodyPart.Hips, dir);

            ApplySkinTone();
        }
    
        public void SetLoadData(AppearanceData data)
        {
            HairSettings = Librarian.Instance.GetHairData(data.Hair);
            //SetGender(data.Gender);
        }

        public AppearanceData GetSaveData()
        {
            return new AppearanceData
            {
                Hair = HairSettings.Name,
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
