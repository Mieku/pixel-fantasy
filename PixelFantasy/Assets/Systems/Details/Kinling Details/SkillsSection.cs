using System.Collections.Generic;
using Characters;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Systems.Details.Kinling_Details
{
    public class SkillsSection : MonoBehaviour
    {
        [SerializeField, BoxGroup("Skills")] private SkillDisplay _meleeDisplay;
        [SerializeField, BoxGroup("Skills")] private SkillDisplay _rangedDisplay;
        [SerializeField, BoxGroup("Skills")] private SkillDisplay _constructionDisplay;
        [SerializeField, BoxGroup("Skills")] private SkillDisplay _miningDisplay;
        [SerializeField, BoxGroup("Skills")] private SkillDisplay _botanyDisplay;
        [SerializeField, BoxGroup("Skills")] private SkillDisplay _cookingDisplay;
        [SerializeField, BoxGroup("Skills")] private SkillDisplay _craftingDisplay;
        [SerializeField, BoxGroup("Skills")] private SkillDisplay _beastMasteryDisplay;
        [SerializeField, BoxGroup("Skills")] private SkillDisplay _medicalDisplay;
        [SerializeField, BoxGroup("Skills")] private SkillDisplay _socialDisplay;
        [SerializeField, BoxGroup("Skills")] private SkillDisplay _intelligenceDisplay;

        [SerializeField, BoxGroup("Traits")] private TraitDisplay _historyTrait;
        [SerializeField, BoxGroup("Traits")] private Transform _traitsLayout;

        private KinlingData _kinlingData;
        private List<TraitDisplay> _displayedTraits = new List<TraitDisplay>();

        public void ShowSection(KinlingData kinlingData)
        {
            gameObject.SetActive(true);
            _kinlingData = kinlingData;

            _meleeDisplay.Init(_kinlingData.StatsData.MeleeSkill, "Melee");
            _rangedDisplay.Init(_kinlingData.StatsData.RangedSkill, "Ranged");
            _constructionDisplay.Init(_kinlingData.StatsData.ConstructionSkill, "Construction");
            _miningDisplay.Init(_kinlingData.StatsData.MiningSkill, "Mining");
            _botanyDisplay.Init(_kinlingData.StatsData.BotanySkill, "Botany");
            _cookingDisplay.Init(_kinlingData.StatsData.CookingSkill, "Cooking");
            _craftingDisplay.Init(_kinlingData.StatsData.CraftingSkill, "Crafting");
            _beastMasteryDisplay.Init(_kinlingData.StatsData.BeastMasterySkill, "Beast Mastery");
            _medicalDisplay.Init(_kinlingData.StatsData.MedicalSkill, "Medical");
            _socialDisplay.Init(_kinlingData.StatsData.SocialSkill, "Social");
            _intelligenceDisplay.Init(_kinlingData.StatsData.IntelligenceSkill, "Intelligence");
            
            RefreshTraits();
        }

        public void Hide()
        {
            gameObject.SetActive(false);
            _kinlingData = null;
        }

        private void RefreshTraits()
        {
            foreach (var displayedTrait in _displayedTraits)
            {
                Destroy(displayedTrait.gameObject);
            }
            _displayedTraits.Clear();
            
            _historyTrait.Init(_kinlingData.StatsData.History.HistoryName, _kinlingData.StatsData.History.DescriptionString(_kinlingData.GetNickname()));

            foreach (var trait in _kinlingData.StatsData.Traits)
            {
                var traitDisplay = Instantiate(_historyTrait, _traitsLayout);
                traitDisplay.Init(trait.TraitName, trait.DescriptionString(_kinlingData.GetNickname()));
                _displayedTraits.Add(traitDisplay);
            }
        }
    }
}
