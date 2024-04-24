using System;
using Characters;
using Controllers;
using Popups.Kinling_Info_Popup;
using Sirenix.OdinInspector;
using Systems.Appearance.Scripts;
using Systems.Details.Build_Details.Scripts;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Systems.Details.Kinling_Details
{
    public class KinlingDetails : MonoBehaviour
    {
        [SerializeField] private PanelLayoutRebuilder _layoutRebuilder;
        [SerializeField] private GameObject _panelHandle;
        [SerializeField] private TextMeshProUGUI _kinlingName;
        [SerializeField] private Portrait _portrait;
        [SerializeField] private TextMeshProUGUI _currentAction;

        [SerializeField] private SkillsSection _skillsSection;
        
        [SerializeField, BoxGroup("Mood")] private BarThresholdDisplay _thresholdDisplay;
        [SerializeField, BoxGroup("Mood")] private BarTargetIndicator _targetIndicator;
        [SerializeField, BoxGroup("Mood")] private Image _overallMoodBarFill;
        [SerializeField, BoxGroup("Mood")] private Image _overallMoodBarBG;
        [SerializeField, BoxGroup("Mood")] private Color _positiveColour;
        [SerializeField, BoxGroup("Mood")] private Color _negativeColour;
        [SerializeField, BoxGroup("Mood")] private Color _sampleLight;
        [SerializeField, BoxGroup("Mood")] private Color _sampleDark;
        
        [SerializeField, BoxGroup("Buttons")] private Image _skillsBtnBG;
        [SerializeField, BoxGroup("Buttons")] private Image _socialBtnBG;
        [SerializeField, BoxGroup("Buttons")] private Image _gearBtnBG;
        [SerializeField, BoxGroup("Buttons")] private Image _healthBtnBG;
        [SerializeField, BoxGroup("Buttons")] private Image _needsBtnBG;
        [SerializeField, BoxGroup("Buttons")] private Image _logBtnBG;
        [SerializeField, BoxGroup("Buttons")] private Sprite _defaultBtnBG;
        [SerializeField, BoxGroup("Buttons")] private Sprite _activeBtnBG;

        private Kinling _kinling;
        private EDetailsState _detailsState;
        
        public void Show(Kinling kinling)
        {
            GameEvents.MinuteTick += GameEvent_MinuteTick;
            
            _kinling = kinling;
            _panelHandle.SetActive(true);
            _kinlingName.text = _kinling.FullName;
            _portrait.Init(kinling.RuntimeData);
            
            _thresholdDisplay.ShowThresholds(_kinling.KinlingMood.AllThresholds);
            RefreshOverallMoodDisplay();
            
            ChangeContentState(EDetailsState.Skills);
            
            RefreshLayout();
        }

        private void Update()
        {
            if(_kinling == null) return;
            
            _currentAction.text = _kinling.TaskAI.CurrentStateName;
        }

        public void Hide()
        {
            GameEvents.MinuteTick -= GameEvent_MinuteTick;
            
            _panelHandle.SetActive(false);
            _kinling = null;
        }
        
        private void RefreshLayout()
        {
            _layoutRebuilder.RefreshLayout();
        }

        private void ChangeContentState(EDetailsState state)
        {
            ClearBtns();
            HideAllSections();
            _detailsState = state;

            switch (state)
            {
                case EDetailsState.Skills:
                    _skillsBtnBG.sprite = _activeBtnBG;
                    _skillsSection.ShowSection(_kinling.RuntimeData);
                    break;
                case EDetailsState.Social:
                    _socialBtnBG.sprite = _activeBtnBG;
                    break;
                case EDetailsState.Gear:
                    _gearBtnBG.sprite = _activeBtnBG;
                    break;
                case EDetailsState.Health:
                    _healthBtnBG.sprite = _activeBtnBG;
                    break;
                case EDetailsState.Needs:
                    _needsBtnBG.sprite = _activeBtnBG;
                    break;
                case EDetailsState.Log:
                    _logBtnBG.sprite = _activeBtnBG;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(state), state, null);
            }
            
            RefreshLayout();
        }

        private void HideAllSections()
        {
            _skillsSection.Hide();
        }
        
        private void RefreshOverallMoodDisplay()
        {
            var overallMoodPercent = _kinling.KinlingMood.OverallMood / 100f;
            var targetMoodPercent = _kinling.KinlingMood.MoodTarget / 100f;
            
            Color lerpedColour = Color.Lerp(_negativeColour, _positiveColour, Mathf.Clamp(overallMoodPercent, 0.0f, 1.0f));
            _overallMoodBarFill.color = lerpedColour;
            
            var darkLuminance = Helper.AdjustColorLuminance(_sampleLight, _sampleDark, lerpedColour);
            _overallMoodBarBG.color = darkLuminance;

            _overallMoodBarFill.fillAmount = overallMoodPercent;
            _targetIndicator.SetTargetIndicator(targetMoodPercent);
        }

        private void GameEvent_MinuteTick()
        {
            RefreshOverallMoodDisplay();
        }

        #region Buttons

        private void ClearBtns()
        {
            _skillsBtnBG.sprite = _defaultBtnBG;
            _socialBtnBG.sprite = _defaultBtnBG;
            _gearBtnBG.sprite = _defaultBtnBG;
            _healthBtnBG.sprite = _defaultBtnBG;
            _needsBtnBG.sprite = _defaultBtnBG;
            _logBtnBG.sprite = _defaultBtnBG;
        }

        public void SkillsBtnPressed()
        {
            ChangeContentState(EDetailsState.Skills);
        }

        public void SocialBtnPressed()
        {
            ChangeContentState(EDetailsState.Social);
        }

        public void GearBtnPressed()
        {
            ChangeContentState(EDetailsState.Gear);
        }

        public void HealthBtnPressed()
        {
            ChangeContentState(EDetailsState.Health);
        }

        public void NeedsBtnPressed()
        {
            ChangeContentState(EDetailsState.Needs);
        }

        public void LogBtnPressed()
        {
            ChangeContentState(EDetailsState.Log);
        }

        public void LookAtBtnPressed()
        {
            CameraManager.Instance.LookAtPosition(_kinling.RuntimeData.Position);
        }

        #endregion

        private enum EDetailsState
        {
            Skills,
            Social,
            Gear,
            Health,
            Needs,
            Log,
        }
    }
}
