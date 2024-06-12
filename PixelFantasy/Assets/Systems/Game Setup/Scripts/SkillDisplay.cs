using System;
using Systems.Stats.Scripts;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Systems.Game_Setup.Scripts
{
    public class SkillDisplay : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _skillValueText;
        [SerializeField] private Image _passionIcon;
        [SerializeField] private Sprite _minorPassionIcon;
        [SerializeField] private Sprite _majorPassionIcon;
        [SerializeField] private Color _positiveColour;
        [SerializeField] private Color _negativeColour;

        public void Init(int value, ESkillPassion passion)
        {
            float lvlProgress = value / 10f;
            Color lerpedColour = Color.Lerp(_negativeColour, _positiveColour, Mathf.Clamp(lvlProgress, 0.0f, 1.0f));

            _skillValueText.text = value.ToString();
            _skillValueText.color = lerpedColour;
            
            switch (passion)
            {
                case ESkillPassion.None:
                    _passionIcon.gameObject.SetActive(false);
                    break;
                case ESkillPassion.Minor:
                    _passionIcon.gameObject.SetActive(true);
                    _passionIcon.sprite = _minorPassionIcon;
                    break;
                case ESkillPassion.Major:
                    _passionIcon.gameObject.SetActive(true);
                    _passionIcon.sprite = _majorPassionIcon;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}
