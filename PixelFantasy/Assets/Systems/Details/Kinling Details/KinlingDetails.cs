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
        [SerializeField] private NeedsSection _needsSection;
        [SerializeField] private MoodSection _moodSection;
        
        [SerializeField, BoxGroup("Buttons")] private Image _skillsBtnBG;
        [SerializeField, BoxGroup("Buttons")] private Image _socialBtnBG;
        [SerializeField, BoxGroup("Buttons")] private Image _gearBtnBG;
        [SerializeField, BoxGroup("Buttons")] private Image _healthBtnBG;
        [SerializeField, BoxGroup("Buttons")] private Image _needsBtnBG;
        [SerializeField, BoxGroup("Buttons")] private Image _logBtnBG;
        [SerializeField, BoxGroup("Buttons")] private Image _moodBtnBG;
        [SerializeField, BoxGroup("Buttons")] private Sprite _defaultBtnBG;
        [SerializeField, BoxGroup("Buttons")] private Sprite _activeBtnBG;

        private Kinling _kinling;
        private EDetailsState _detailsState;
        
        public void Show(Kinling kinling)
        {
            _kinling = kinling;
            _panelHandle.SetActive(true);
            _kinlingName.text = _kinling.FullName;
            _portrait.Init(kinling.RuntimeData);
            
            ChangeContentState(EDetailsState.Skills);
            
            RefreshLayout();

            GameEvents.OnKinlingChanged += GameEvent_OnKinlingChanged;
        }

        private void Update()
        {
            if(_kinling == null) return;
            
            _currentAction.text = _kinling.TaskAI.CurrentStateName;
        }

        public void Hide()
        {
            GameEvents.OnKinlingChanged -= GameEvent_OnKinlingChanged;
            
            _panelHandle.SetActive(false);
            HideAllSections();
            _kinling = null;
        }

        private void GameEvent_OnKinlingChanged(Kinling kinling)
        {
            if(kinling != _kinling) return;
            
            RefreshContentState(_detailsState);
        }
        
        public void RefreshLayout()
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
                    _needsSection.ShowSection(_kinling.RuntimeData);
                    break;
                case EDetailsState.Log:
                    _logBtnBG.sprite = _activeBtnBG;
                    break;
                case EDetailsState.Mood:
                    _moodBtnBG.sprite = _activeBtnBG;
                    _moodSection.ShowSection(_kinling.RuntimeData, RefreshLayout);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(state), state, null);
            }
            
            RefreshLayout();
        }
        
        private void RefreshContentState(EDetailsState state)
        {
            switch (state)
            {
                case EDetailsState.Skills:
                    
                    break;
                case EDetailsState.Social:
                    
                    break;
                case EDetailsState.Gear:
                    
                    break;
                case EDetailsState.Health:
                    
                    break;
                case EDetailsState.Needs:
                    
                    break;
                case EDetailsState.Log:
                    
                    break;
                case EDetailsState.Mood:
                    _moodSection.KinlingUpdateRefresh();
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(state), state, null);
            }
        }

        private void HideAllSections()
        {
            _skillsSection.Hide();
            _needsSection.Hide();
            _moodSection.Hide();
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
            _moodBtnBG.sprite = _defaultBtnBG;
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

        public void MoodBtnPressed()
        {
            ChangeContentState(EDetailsState.Mood);
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
            Mood,
        }
    }
}
