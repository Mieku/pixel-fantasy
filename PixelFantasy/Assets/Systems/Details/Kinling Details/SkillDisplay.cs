using System;
using Systems.Stats.Scripts;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Systems.Details.Kinling_Details
{
    public class SkillDisplay : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _skillName;
        [SerializeField] private TextMeshProUGUI _skillRankText;
        [SerializeField] private TextMeshProUGUI _expDisplay;
        [SerializeField] private Image _passionIcon;
        [SerializeField] private Image _barFill;
        [SerializeField] private Image _barBG;
        [SerializeField] private Sprite _minorPassionIcon;
        [SerializeField] private Sprite _majorPassionIcon;
        
        [SerializeField] private Color _positiveColour;
        [SerializeField] private Color _negativeColour;
        [SerializeField] private Color _sampleLight;
        [SerializeField] private Color _sampleDark;

        private SkillData _skillData;

        public void Init(SkillData skillData, string skillName)
        {
            _skillData = skillData;

            _skillName.text = skillName;

            if (_skillData.Incapable)
            {
                _passionIcon.gameObject.SetActive(false);
            }
            else
            {
                switch (_skillData.Passion)
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

        private void Update()
        {
            if(_skillData == null) return;
            
            _skillRankText.text = $"{_skillData.RankString}";

            if (_skillData.Incapable)
            {
                _expDisplay.text = $"";
                _skillName.color = _negativeColour;
            }
            else
            {
                _expDisplay.text = $"Exp: {_skillData.PercentString}%";
                _skillName.color = Color.white;
            }

            float lvlProgress = _skillData.Level / 10f;
            _barFill.fillAmount = lvlProgress;

            Color lerpedColour = Color.Lerp(_negativeColour, _positiveColour, Mathf.Clamp(lvlProgress, 0.0f, 1.0f));
            _barFill.color = lerpedColour;
            
            var darkLuminance = Helper.AdjustColorLuminance(_sampleLight, _sampleDark, lerpedColour);
            _barBG.color = darkLuminance;
        }
    }
}
