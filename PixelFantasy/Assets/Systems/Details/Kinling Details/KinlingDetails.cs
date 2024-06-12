using System;
using Characters;
using Controllers;
using Sirenix.OdinInspector;
using Systems.Appearance.Scripts;
using Systems.Details.Build_Details.Scripts;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace Systems.Details.Kinling_Details
{
    public class KinlingDetails : MonoBehaviour
    {
        [SerializeField] private PanelLayoutRebuilder _layoutRebuilder;
        [SerializeField] private GameObject _panelHandle;
        [SerializeField] private TextMeshProUGUI _kinlingName;
        [SerializeField] private Image _avatarDisplay;
        [SerializeField] private TextMeshProUGUI _currentAction;
        [SerializeField] private TextMeshProUGUI _currentSchedule;

        [SerializeField] private SkillsSection _skillsSection;
        [SerializeField] private NeedsSection _needsSection;
        [SerializeField] private MoodSection _moodSection;
        [SerializeField] private SocialSection _socialSection;
        [SerializeField] private StatsSection _statsSection;
        
        [SerializeField, BoxGroup("Buttons")] private Image _skillsBtnBG;
        [SerializeField, BoxGroup("Buttons")] private Image _socialBtnBG;
        [SerializeField, BoxGroup("Buttons")] private Image _gearBtnBG;
        [SerializeField, BoxGroup("Buttons")] private Image _healthBtnBG;
        [SerializeField, BoxGroup("Buttons")] private Image _needsBtnBG;
        [SerializeField, BoxGroup("Buttons")] private Image _statsBtnBG;
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
            _avatarDisplay.sprite = _kinling.RuntimeData.Avatar.GetBaseAvatarSprite();
            
            ChangeContentState(EDetailsState.Needs);
            
            GameEvents.OnKinlingChanged += GameEvent_OnKinlingChanged;
            
            RefreshLayout();
        }

        private void Update()
        {
            if(_kinling == null) return;
            
            _currentAction.text = _kinling.TaskAI.CurrentStateName;
            _currentSchedule.text = _kinling.RuntimeData.Schedule.GetCurrentScheduleOption().GetDescription();
        }

        public void Hide()
        {
            GameEvents.OnKinlingChanged -= GameEvent_OnKinlingChanged;
            
            _panelHandle.SetActive(false);
            HideAllSections();
            _kinling = null;
        }

        private void GameEvent_OnKinlingChanged(KinlingData kinling)
        {
            if(kinling != _kinling.RuntimeData) return;
            
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
                    _socialSection.ShowSection(_kinling.RuntimeData, RefreshLayout);
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
                case EDetailsState.Stats:
                    _statsBtnBG.sprite = _activeBtnBG;
                    _statsSection.ShowSection(_kinling.RuntimeData, RefreshLayout);
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
                    _socialSection.RefreshContent();
                    break;
                case EDetailsState.Gear:
                    
                    break;
                case EDetailsState.Health:
                    
                    break;
                case EDetailsState.Needs:
                    
                    break;
                case EDetailsState.Stats:
                    _statsSection.RefreshContent();
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
            _socialSection.Hide();
            _statsSection.Hide();
        }

        #region Buttons

        private void ClearBtns()
        {
            _skillsBtnBG.sprite = _defaultBtnBG;
            _socialBtnBG.sprite = _defaultBtnBG;
            _gearBtnBG.sprite = _defaultBtnBG;
            _healthBtnBG.sprite = _defaultBtnBG;
            _needsBtnBG.sprite = _defaultBtnBG;
            _statsBtnBG.sprite = _defaultBtnBG;
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

        public void StatsBtnPressed()
        {
            ChangeContentState(EDetailsState.Stats);
        }

        public void LookAtBtnPressed()
        {
            CameraManager.Instance.LookAtPosition(_kinling.RuntimeData.Position);
        }

        public void MoodBtnPressed()
        {
            ChangeContentState(EDetailsState.Mood);
        }

        public void DraftBtnPressed()
        {
            
        }

        #endregion

        private enum EDetailsState
        {
            Skills,
            Social,
            Gear,
            Health,
            Needs,
            Stats,
            Mood,
        }
    }
}
