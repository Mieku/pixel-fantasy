using System;
using System.Collections.Generic;
using Characters;
using Sirenix.OdinInspector;
using Systems.Details.Build_Details.Scripts;
using Systems.Details.Generic_Details.Scripts;
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
        [SerializeField] private Image _avatarDisplay;
        [SerializeField] private TextMeshProUGUI _currentAction;
        [SerializeField] private TextMeshProUGUI _currentSchedule;

        [SerializeField] private SkillsSection _skillsSection;
        [SerializeField] private NeedsSection _needsSection;
        [SerializeField] private MoodSection _moodSection;
        [SerializeField] private SocialSection _socialSection;
        [SerializeField] private StatsSection _statsSection;
        
        [SerializeField, BoxGroup("Tabs")] private TabDisplay _skillsTab;
        [SerializeField, BoxGroup("Tabs")] private TabDisplay _socialTab;
        [SerializeField, BoxGroup("Tabs")] private TabDisplay _gearTab;
        [SerializeField, BoxGroup("Tabs")] private TabDisplay _healthTab;
        [SerializeField, BoxGroup("Tabs")] private TabDisplay _needsTab;
        [SerializeField, BoxGroup("Tabs")] private TabDisplay _statsTab;
        [SerializeField, BoxGroup("Tabs")] private TabDisplay _moodTab;
        
        [SerializeField, BoxGroup("Commands")] private Transform _commandsParent;
        [SerializeField, BoxGroup("Commands")] private CommandBtn _commandBtnPrefab;

        private Kinling _kinling;
        private EDetailsState _detailsState;
        private List<CommandBtn> _displayedCmds = new List<CommandBtn>();
        
        public void Show(Kinling kinling)
        {
            _kinling = kinling;
            _panelHandle.SetActive(true);
            _commandBtnPrefab.gameObject.SetActive(false);
            _kinlingName.text = _kinling.FullName;
            _avatarDisplay.sprite = _kinling.RuntimeData.Avatar.GetBaseAvatarSprite();
            
            ChangeContentState(EDetailsState.Needs);
            
            GameEvents.OnKinlingChanged += GameEvent_OnKinlingChanged;
            
            RefreshLayout();
            RefreshCommands();
        }
        
        private void RefreshCommands()
        {
            foreach (var displayedCmd in _displayedCmds)
            {
                Destroy(displayedCmd.gameObject);
            }
            _displayedCmds.Clear();
            
            var commands = _kinling.GetCommands();
            foreach (var command in commands)
            {
                bool isActive = _kinling.PendingCommand == command;
                
                var cmdBtn = Instantiate(_commandBtnPrefab, _commandsParent);
                cmdBtn.Init(command, isActive, OnCommandPressed);
                cmdBtn.gameObject.SetActive(true);
                _displayedCmds.Add(cmdBtn);
            }
        }
        
        private void OnCommandPressed(Command command)
        {
            var pendingCmd = _kinling.PendingCommand;
            if (pendingCmd != null)
            {
                _kinling.CancelPendingTask();
            }
            
            if (pendingCmd != command)
            {
                _kinling.AssignCommand(command);
            }
            
            RefreshCommands();
        }

        private void Update()
        {
            if(_kinling == null) return;

            _currentAction.text = _kinling.GetCurrentTaskDisplay();
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
            ResetTabs();
            HideAllSections();
            _detailsState = state;

            switch (state)
            {
                case EDetailsState.Skills:
                    _skillsTab.SetActive(true);
                    _skillsSection.ShowSection(_kinling.RuntimeData);
                    break;
                case EDetailsState.Social:
                    _socialTab.SetActive(true);
                    _socialSection.ShowSection(_kinling.RuntimeData, RefreshLayout);
                    break;
                case EDetailsState.Gear:
                    _gearTab.SetActive(true);
                    break;
                case EDetailsState.Health:
                    _healthTab.SetActive(true);
                    break;
                case EDetailsState.Needs:
                    _needsTab.SetActive(true);
                    _needsSection.ShowSection(_kinling.RuntimeData);
                    break;
                case EDetailsState.Stats:
                    _statsTab.SetActive(true);
                    _statsSection.ShowSection(_kinling.RuntimeData, RefreshLayout);
                    break;
                case EDetailsState.Mood:
                    _moodTab.SetActive(true);
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
        
        private void ResetTabs()
        {
            _skillsTab.SetActive(false);
            _socialTab.SetActive(false);
            _gearTab.SetActive(false);
            _healthTab.SetActive(false);
            _needsTab.SetActive(false);
            _statsTab.SetActive(false);
            _moodTab.SetActive(false);
        }

        public void SkillsTabPressed()
        {
            ChangeContentState(EDetailsState.Skills);
        }

        public void SocialTabPressed()
        {
            ChangeContentState(EDetailsState.Social);
        }

        public void GearTabPressed()
        {
            ChangeContentState(EDetailsState.Gear);
        }

        public void HealthTabPressed()
        {
            ChangeContentState(EDetailsState.Health);
        }

        public void NeedsTabPressed()
        {
            ChangeContentState(EDetailsState.Needs);
        }

        public void StatsTabPressed()
        {
            ChangeContentState(EDetailsState.Stats);
        }
        
        public void MoodTabPressed()
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
