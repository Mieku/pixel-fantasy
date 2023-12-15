using System;
using ScriptableObjects;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Serialization;

namespace Characters
{
    public class GearPiece : MonoBehaviour
    {
        [FormerlySerializedAs("_equipmentData")] [SerializeField] private GearData _gearData;
        [SerializeField] private GameObject _sideView;
        [SerializeField] private GameObject _upView;
        [SerializeField] private GameObject _downView;

        private UnitActionDirection _curDirection;
        private DyePaletteData _assignedDyePalette;
        
        private readonly int colorSwapRed = Shader.PropertyToID("_ColorSwapRed");
        private readonly int colorSwapGreen = Shader.PropertyToID("_ColorSwapGreen");
        private readonly int colorSwapBlue = Shader.PropertyToID("_ColorSwapBlue");
        private readonly int cwRedLumin = Shader.PropertyToID("_ColorSwapRedLuminosity");
        private readonly int cwGreenLumin = Shader.PropertyToID("_ColorSwapGreenLuminosity");
        private readonly int cwBlueLumin = Shader.PropertyToID("_ColorSwapBlueLuminosity");
        
        [Button("Display Default Dye")]
        // For Debugging
        private void DisplayDefaultDye()
        {
            if (_gearData.CanBeDyed && _gearData.DefaultDyePalette != null)
            {
                AssignDyePallet(_gearData.DefaultDyePalette);
            }
            else
            {
                Debug.LogError("This Does Not Have Any Default Dye!");
            }
        }

        public void AssignDyePallet(DyePaletteData dyePalette)
        {
            if(!_gearData.CanBeDyed) return;
            
            if (dyePalette == null)
            {
                _assignedDyePalette = _gearData.DefaultDyePalette;
            }
            else
            {
                _assignedDyePalette = dyePalette;
            }

            DyeGear();
        }
        
        public void DyeGear()
        {
            if (_assignedDyePalette == null)
            {
                if (_gearData.CanBeDyed && _gearData.DefaultDyePalette != null)
                {
                    _assignedDyePalette = _gearData.DefaultDyePalette;
                }
                else
                {
                    return;
                }
            } 

            if (_sideView != null)
            {
                ApplyMaterialRecolour(_sideView, _assignedDyePalette);
            }

            if (_upView != null)
            {
                ApplyMaterialRecolour(_upView, _assignedDyePalette);
            }

            if (_downView != null)
            {
                ApplyMaterialRecolour(_downView, _assignedDyePalette);
            }
        }

        private void ApplyMaterialRecolour(GameObject view, DyePaletteData dye)
        {
            var spriteRenderer = view.GetComponent<SpriteRenderer>();
            if (spriteRenderer != null)
            {
                ApplyMaterialRecolour(spriteRenderer, dye.Primary, dye.PrimaryShade, dye.Accent);
            }
        }
        
        private void ApplyMaterialRecolour(SpriteRenderer spriteRenderer, Color primary, Color primaryShade, Color accent)
        {
            var mat = spriteRenderer.gameObject.GetComponent<Renderer>().material;
            mat.EnableKeyword("COLORSWAP_ON");
            mat.SetTexture("_ColorSwapTex", spriteRenderer.sprite.texture);
            
            mat.SetColor(colorSwapRed, primary);
            mat.SetColor(colorSwapGreen, primaryShade);
            mat.SetColor(colorSwapBlue, accent);
            
            mat.SetFloat(cwRedLumin, 1.0f);
            mat.SetFloat(cwGreenLumin, 1.0f);
            mat.SetFloat(cwBlueLumin, 1.0f);
        }

        public void AssignDirection(UnitActionDirection direction)
        {
            _curDirection = direction;

            switch (direction)
            {
                case UnitActionDirection.Side:
                    _sideView.SetActive(true);
                    _upView.SetActive(false);
                    _downView.SetActive(false);
                    break;
                case UnitActionDirection.Up:
                    _sideView.SetActive(false);
                    _upView.SetActive(true);
                    _downView.SetActive(false);
                    break;
                case UnitActionDirection.Down:
                    _sideView.SetActive(false);
                    _upView.SetActive(false);
                    _downView.SetActive(true);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(direction), direction, null);
            }
        }

        public bool IsToolType(EToolType toolType)
        {
            var toolData = _gearData as ToolData;
            if (toolData != null)
            {
                return toolData.ToolType == toolType;
            }

            return false;
        }
    }
}
