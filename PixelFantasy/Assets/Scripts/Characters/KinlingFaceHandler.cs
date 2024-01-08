using System;
using Managers;
using ScriptableObjects;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Characters
{
    [RequireComponent(typeof(SpriteRenderer))]
    public class KinlingFaceHandler : MonoBehaviour
    {
        private SpriteRenderer _faceRenderer;
        private BodyData _bodyData;
        private UnitActionDirection _dir;
        private bool _overrideEyesClosed;
        private bool _eyesClosed;
        private Color _eyeColour;
        private Color _blushColour;
        private float _eyesTimer;
        private float _stareTime;
        private float _blinkTime;
        private bool _isBlinking;
        private bool _isForceClosed;

        private readonly int colorSwapRed = Shader.PropertyToID("_ColorSwapRed");
        private readonly int colorSwapGreen = Shader.PropertyToID("_ColorSwapGreen");
        private readonly int colorSwapBlue = Shader.PropertyToID("_ColorSwapBlue");
        private readonly int cwRedLumin = Shader.PropertyToID("_ColorSwapRedLuminosity");
        private readonly int cwGreenLumin = Shader.PropertyToID("_ColorSwapGreenLuminosity");
        private readonly int cwBlueLumin = Shader.PropertyToID("_ColorSwapBlueLuminosity");

        private const float BLINK_SPEED = 0.1f;
        private const float MIN_EYES_OPEN = 1.0f;
        private const float MAX_EYES_OPEN = 4.0f;

        private void Awake()
        {
            _faceRenderer = GetComponent<SpriteRenderer>();
        }

        public void Init(BodyData bodyData, UnitActionDirection dir)
        {
            _bodyData = bodyData;
            SetDirection(dir);
        }

        private void Update()
        {
            if (_bodyData == null) return;
            
            HandleBlinking();
        }

        private void HandleBlinking()
        {
            if (_isForceClosed) return;
            
            _eyesTimer += TimeManager.Instance.DeltaTime;

            if (_isBlinking)
            {
                if (_eyesTimer >= BLINK_SPEED)
                {
                    _eyesTimer = 0f;
                    _isBlinking = false;
                    OpenEyes();
                }
            }
            else if (_eyesTimer >= _stareTime)
            {
                _eyesTimer = 0f;
                _stareTime = Random.Range(MIN_EYES_OPEN, MAX_EYES_OPEN);
                _isBlinking = true;
                CloseEyes();
            }
        }

        public void SetForcedEyesClosed(bool forceClosed)
        {
            if (forceClosed)
            {
                _isForceClosed = true;
                CloseEyes();
            }
            else
            {
                _isForceClosed = false;
                OpenEyes();
            }
        }

        private void CloseEyes()
        {
            _eyesClosed = true;
            _faceRenderer.sprite = _bodyData.GetBodySprite(BodyPart.Face_EyesClosed, _dir);
            RefreshMaterials();
        }

        private void OpenEyes()
        {
            _eyesClosed = false;
            _faceRenderer.sprite = _bodyData.GetBodySprite(BodyPart.Face, _dir);
            RefreshMaterials();
        }

        public void AssignFaceColours(Color eyeColour, Color blushColour)
        {
            _eyeColour = eyeColour;
            _blushColour = blushColour;
            RefreshMaterials();
        }

        private void RefreshMaterials()
        {
            ApplyMaterialRecolour(_faceRenderer, _eyeColour, _blushColour, Color.black);
        }

        public void SetDirection(UnitActionDirection dir)
        {
            if (dir != _dir)
            {
                switch (dir)
                {
                    case UnitActionDirection.Side:
                        _faceRenderer.gameObject.SetActive(true);
                        break;
                    case UnitActionDirection.Up:
                        _faceRenderer.gameObject.SetActive(false);
                        break;
                    case UnitActionDirection.Down:
                        _faceRenderer.gameObject.SetActive(true);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(dir), dir, null);
                }
            
                if (dir != UnitActionDirection.Up)
                {
                    if (_eyesClosed)
                    {
                        _faceRenderer.sprite = _bodyData.GetBodySprite(BodyPart.Face_EyesClosed, dir);
                    }
                    else
                    {
                        _faceRenderer.sprite = _bodyData.GetBodySprite(BodyPart.Face, dir);
                    }
                }
            }

            _dir = dir;
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
    }
}
