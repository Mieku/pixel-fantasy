using System;
using System.Collections.Generic;
using System.Linq;
using Characters;
using HUD.Tooltip;
using ScriptableObjects;
using Sirenix.OdinInspector;
using Systems.Appearance.Scripts;
using Systems.Stats.Scripts;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace Systems.Game_Setup.Scripts
{
    public class ChooseKinlingsPanel : MonoBehaviour
    {
        public List<KinlingData> PlayersKinlings = new List<KinlingData>();
        
        [SerializeField] private NewGameSection _newGameSection;
        [SerializeField] private int _numberOfKinlings = 5;
        [SerializeField] private KinlingOptionDisplay _kinlingOptionDisplayPrefab;
        [SerializeField] private Transform _kinlingOptionsParent;

        [BoxGroup("Colony Skills"), SerializeField]
        private SkillDisplay _meleeColonySkill, 
            _rangedColonySkill, 
            _constructionColonySkill, 
            _miningColonySkill, 
            _botanyColonySkill, 
            _cookingColonySkill, 
            _craftingColonySkill, 
            _beastMasteryColonySkill, 
            _medicalColonySkill, 
            _socialColonySkill, 
            _intelligenceColonySkill;

        [BoxGroup("Current Kinling Details"), SerializeField] private Image _kinlingAvatar;
        [BoxGroup("Current Kinling Details"), SerializeField] private TMP_InputField _firstNameInput, _lastNameInput, _nicknameInput;
        [BoxGroup("Current Kinling Details"), SerializeField] private TextMeshProUGUI _raceText, _ageText, _genderText, _preferenceText, _historyText;
        [BoxGroup("Current Kinling Details"), SerializeField] private TooltipTrigger _historyTooltip;
        [BoxGroup("Current Kinling Details"), SerializeField] private TraitDisplay _traitDisplayPrefab;
        [BoxGroup("Current Kinling Details"), SerializeField] private Transform _traitParent;
        [BoxGroup("Current Kinling Details"), SerializeField]
        private SkillDisplay _meleeSkill, 
            _rangedSkill, 
            _constructionSkill, 
            _miningSkill, 
            _botanySkill, 
            _cookingSkill, 
            _craftingSkill, 
            _beastMasterySkill, 
            _medicalSkill, 
            _socialSkill, 
            _intelligenceSkill;
        private List<TraitDisplay> _displayedTraits = new List<TraitDisplay>();

        [BoxGroup("Relationships"), SerializeField] private RelationshipDisplay _relationshipDisplayPrefab;
        [BoxGroup("Relationships"), SerializeField] private Transform _relationshipParent;
        private List<RelationshipDisplay> _displayedRelationships = new List<RelationshipDisplay>();
        
        private KinlingData _selectedKinling;
        private List<KinlingOptionDisplay> _displayedKinlings = new List<KinlingOptionDisplay>();

        public void Show()
        {
            gameObject.SetActive(true);
            _kinlingOptionDisplayPrefab.gameObject.SetActive(false);
            _relationshipDisplayPrefab.gameObject.SetActive(false);

            if (PlayersKinlings == null || PlayersKinlings.Count == 0)
            {
                // Generate new kinlings
                var race = GameSettings.Instance.LoadRaceSettings("Human");
                PlayersKinlings = GameManager.Instance.GenerateNewKinlings(_numberOfKinlings, race);
                _selectedKinling = null;
            }
            
            RefreshDisplayedKinlings();
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }

        private void RefreshDisplayedKinlings()
        {
            foreach (var displayedKinling in _displayedKinlings)
            {
                Destroy(displayedKinling.gameObject);
            }
            _displayedKinlings.Clear();

            foreach (var kinling in PlayersKinlings)
            {
                var display = Instantiate(_kinlingOptionDisplayPrefab, _kinlingOptionsParent);
                display.gameObject.SetActive(true);
                display.Init(kinling, this);
                _displayedKinlings.Add(display);
            }

            if (_selectedKinling == null) 
            {
                SetCurrentKinling(PlayersKinlings.First());
            }
            else
            {
                OnKinlingSelected(_selectedKinling);
            }
            
            RefreshColonySkills();
        }

        public void OnKinlingSelected(KinlingData kinlingData)
        {
            foreach (var displayedKinling in _displayedKinlings)
            {
                displayedKinling.SetHighlight(false);
            }
            
            SetCurrentKinling(kinlingData);
        }

        private void SetCurrentKinling(KinlingData kinlingData)
        {
            _selectedKinling = kinlingData;
            var display = _displayedKinlings.Find(d => d.KinlingData == kinlingData);
            display.SetHighlight(true);
            
            RefreshCurrentKinlingDetails();
        }

        private void RefreshCurrentKinlingDetails()
        {
            _traitDisplayPrefab.gameObject.SetActive(false);
            foreach (var traitDisplay in _displayedTraits)
            {
                Destroy(traitDisplay.gameObject);
            }
            _displayedTraits.Clear();

            _kinlingAvatar.sprite = _selectedKinling.Avatar.GetBaseAvatarSprite();
            _firstNameInput.SetTextWithoutNotify(_selectedKinling.Firstname);
            _lastNameInput.SetTextWithoutNotify(_selectedKinling.Lastname);
            _nicknameInput.SetTextWithoutNotify(_selectedKinling.Nickname);
            _raceText.text = _selectedKinling.Race.RaceName;
            _ageText.text = _selectedKinling.Age.ToString();
            _genderText.text = _selectedKinling.Gender.GetDescription();
            _preferenceText.text = _selectedKinling.SexualPreference.GetDescription();
            _historyText.text = _selectedKinling.Stats.History.HistoryName;
            _historyTooltip.Header = _selectedKinling.Stats.History.HistoryName;
            _historyTooltip.Content = _selectedKinling.Stats.History.DescriptionString(_selectedKinling.Nickname);

            var traits = _selectedKinling.Stats.Traits;
            foreach (var trait in traits)
            {
                var display = Instantiate(_traitDisplayPrefab, _traitParent);
                display.gameObject.SetActive(true);
                display.Init(trait, _selectedKinling.Nickname);
                _displayedTraits.Add(display);
            }

            var melee = _selectedKinling.Stats.GetSkillByType(ESkillType.Melee);
            _meleeSkill.Init(melee.Level, melee.Passion);
            
            var ranged = _selectedKinling.Stats.GetSkillByType(ESkillType.Ranged);
            _rangedSkill.Init(ranged.Level, ranged.Passion);
            
            var construction = _selectedKinling.Stats.GetSkillByType(ESkillType.Construction);
            _constructionSkill.Init(construction.Level, construction.Passion);
            
            var mining = _selectedKinling.Stats.GetSkillByType(ESkillType.Mining);
            _miningSkill.Init(mining.Level, mining.Passion);
            
            var botany = _selectedKinling.Stats.GetSkillByType(ESkillType.Botany);
            _botanySkill.Init(botany.Level, botany.Passion);
            
            var cooking = _selectedKinling.Stats.GetSkillByType(ESkillType.Cooking);
            _cookingSkill.Init(cooking.Level, cooking.Passion);
            
            var crafting = _selectedKinling.Stats.GetSkillByType(ESkillType.Crafting);
            _craftingSkill.Init(crafting.Level, crafting.Passion);
            
            var beastMastery = _selectedKinling.Stats.GetSkillByType(ESkillType.BeastMastery);
            _beastMasterySkill.Init(beastMastery.Level, beastMastery.Passion);
            
            var medical = _selectedKinling.Stats.GetSkillByType(ESkillType.Medical);
            _medicalSkill.Init(medical.Level, medical.Passion);
            
            var social = _selectedKinling.Stats.GetSkillByType(ESkillType.Social);
            _socialSkill.Init(social.Level, social.Passion);
            
            var intelligence = _selectedKinling.Stats.GetSkillByType(ESkillType.Intelligence);
            _intelligenceSkill.Init(intelligence.Level, intelligence.Passion);
            
            // Relationships
            foreach (var relationship in _displayedRelationships)
            {
                Destroy(relationship.gameObject);
            }
            _displayedRelationships.Clear();

            var relations = _selectedKinling.Relationships;
            int relationshipIndex = 0;
            foreach (var relationship in relations)
            {
                bool showBG = relationshipIndex % 2 == 0;
                var display = Instantiate(_relationshipDisplayPrefab, _relationshipParent);
                display.gameObject.SetActive(true);
                display.Init(relationship, showBG, _displayedKinlings);
                _displayedRelationships.Add(display);
                relationshipIndex++;
            }
        }

        private void RefreshColonySkills()
        {
            var meleeSkill = GetColonySkillByType(ESkillType.Melee);
            _meleeColonySkill.Init(meleeSkill.Item1, meleeSkill.Item2);
            
            var rangedSkill = GetColonySkillByType(ESkillType.Ranged);
            _rangedColonySkill.Init(rangedSkill.Item1, rangedSkill.Item2);
            
            var constructionSkill = GetColonySkillByType(ESkillType.Construction);
            _constructionColonySkill.Init(constructionSkill.Item1, constructionSkill.Item2);
            
            var miningSkill = GetColonySkillByType(ESkillType.Mining);
            _miningColonySkill.Init(miningSkill.Item1, miningSkill.Item2);
            
            var botanySkill = GetColonySkillByType(ESkillType.Botany);
            _botanyColonySkill.Init(botanySkill.Item1, botanySkill.Item2);
            
            var cookingSkill = GetColonySkillByType(ESkillType.Cooking);
            _cookingColonySkill.Init(cookingSkill.Item1, cookingSkill.Item2);
            
            var craftingSkill = GetColonySkillByType(ESkillType.Crafting);
            _craftingColonySkill.Init(craftingSkill.Item1, craftingSkill.Item2);
            
            var beastMasterySkill = GetColonySkillByType(ESkillType.BeastMastery);
            _beastMasteryColonySkill.Init(beastMasterySkill.Item1, beastMasterySkill.Item2);
            
            var medicalSkill = GetColonySkillByType(ESkillType.Medical);
            _medicalColonySkill.Init(medicalSkill.Item1, medicalSkill.Item2);
            
            var socialSkill = GetColonySkillByType(ESkillType.Social);
            _socialColonySkill.Init(socialSkill.Item1, socialSkill.Item2);
            
            var intelligenceSkill = GetColonySkillByType(ESkillType.Intelligence);
            _intelligenceColonySkill.Init(intelligenceSkill.Item1, intelligenceSkill.Item2);
        }

        private (int, ESkillPassion) GetColonySkillByType(ESkillType skillType)
        {
            int highestLevel = 0;
            ESkillPassion highestPassion = ESkillPassion.None;

            foreach (var kinling in PlayersKinlings)
            {
                var skill = kinling.Stats.GetSkillByType(skillType);
                if (highestLevel < skill.Level) highestLevel = skill.Level;
                if (highestPassion < skill.Passion) highestPassion = skill.Passion;
            }

            return (highestLevel, highestPassion);
        }
        
        #region Button Hooks

        public void OnBackPressed()
        {
            _newGameSection.OnBack();
        }

        public void OnContinuePressed()
        {
            _newGameSection.OnContinue();
        }

        public void OnRerollColonyPressed()
        {
            var race = GameSettings.Instance.LoadRaceSettings("Human");
            PlayersKinlings = GameManager.Instance.GenerateNewKinlings(_numberOfKinlings, race);
            _selectedKinling = null;
            
            RefreshDisplayedKinlings();
        }

        public void OnRerollKinlingPressed()
        {
            var newKinling = _selectedKinling;
            _selectedKinling.Randomize(_selectedKinling.Race);
            _selectedKinling.Mood.JumpMoodToTarget();
            AppearanceBuilder.Instance.UpdateAppearance(_selectedKinling);
            
            GameManager.Instance.GenerateNewRelationships(PlayersKinlings);
            
            RefreshDisplayedKinlings();
            OnKinlingSelected(newKinling);
        }

        public void OnFirstnameChanged(string value)
        {
            if (string.IsNullOrEmpty(value)) value = _selectedKinling.Firstname;
            
            _selectedKinling.Firstname = value;
            RefreshDisplayedKinlings();
        }

        public void OnLastnameChanged(string value)
        {
            if (string.IsNullOrEmpty(value)) value = _selectedKinling.Lastname;
            
            _selectedKinling.Lastname = value;
            RefreshDisplayedKinlings();
        }

        public void OnNicknameChanged(string value)
        {
            _selectedKinling.Nickname = value;
            RefreshDisplayedKinlings();
        }

        public void OnRefreshNicknamePressed()
        {
            _selectedKinling.Nickname = _selectedKinling.Race.GetRandomNickname(_selectedKinling.Gender);
            RefreshDisplayedKinlings();
        }

        public void OnRefreshNamePressed()
        {
            bool hasNameBasedNickname = _selectedKinling.Nickname == _selectedKinling.Firstname ||
                                     _selectedKinling.Nickname == _selectedKinling.Lastname;
            
            var firstname = _selectedKinling.Race.GetRandomFirstName(_selectedKinling.Gender);
            var lastname = _selectedKinling.Race.GetRandomLastName();
            
            _selectedKinling.Firstname = firstname;
            _selectedKinling.Lastname = lastname;

            if (hasNameBasedNickname)
            {
                var nickname = _selectedKinling.GenerateNickname();
                _selectedKinling.Nickname = nickname;
            }
            
            RefreshDisplayedKinlings();
        }

        #endregion
    }
}
