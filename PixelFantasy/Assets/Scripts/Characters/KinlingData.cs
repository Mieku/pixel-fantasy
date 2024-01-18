using System;
using System.Collections.Generic;
using System.Linq;
using Items;
using ScriptableObjects;
using Sirenix.OdinInspector;
using Systems.Traits.Scripts;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

namespace Characters
{
    [Serializable]
    public class KinlingData
    {
        [BoxGroup("General")] [SerializeField] private string _uid;
        public string UID
        {
            get
            {
                if (string.IsNullOrEmpty(_uid))
                {
                    GenerateUID();
                }
                return _uid;
            }
            protected set => _uid = value;
        }
        
        [BoxGroup("General")] public string Firstname;
        [BoxGroup("General")] public string Lastname;
        [BoxGroup("General")] public EMaturityStage MaturityStage;
        [BoxGroup("General")] public Gender Gender;
        [BoxGroup("General")] public ESexualPreference SexualPreference;
        
        [BoxGroup("Appearance")] public AppearanceState Appearance;
        [BoxGroup("Gear")] public KinlingGearData Gear;
        [BoxGroup("Traits")] public List<Trait> Traits = new List<Trait>();
        [BoxGroup("Stats")] public Stats Stats;
        [BoxGroup("Family")] public string Partner;
        [BoxGroup("Family")] public List<string> Children = new List<string>();
        
        [BoxGroup("General")] [Button("Create UID")]
        private void GenerateUID()
        {
            _uid = $"Kinling_{Firstname}_{Lastname}_{Guid.NewGuid()}";
        }

        public KinlingData(KinlingData mother, KinlingData father)
        {
            MaturityStage = EMaturityStage.Child;
            if (Helper.RollDice(50))
            {
                Gender = Gender.Male;
            }
            else
            {
                Gender = Gender.Female;
            }

            SexualPreference = DetermineSexuality();
            Appearance = new AppearanceState(Gender, mother.Appearance, father.Appearance);
            Gear = new KinlingGearData();
            Stats = new Stats(mother.Stats, father.Stats);
            Traits = GetTraitsFromParents(mother.Traits, father.Traits);

            Firstname = Appearance.Race.GetRandomFirstName(Gender);
            Lastname = mother.Lastname;
            GenerateUID();
        }

        private ESexualPreference DetermineSexuality()
        {
            var sexualityRoll = Random.Range(0f, 100f);
            if (sexualityRoll <= 10f) // According to global statistics roughly 10% of population identifies as not heterosexual
            {
                if (sexualityRoll <= 6f) // According to global statistics roughly 60% of non-heterosexual individuals identify as bi-sexual
                {
                    // Bisexual
                    return ESexualPreference.Both;
                }
                else
                {
                    // Homosexual
                    if (Gender == Gender.Male)
                    {
                        return ESexualPreference.Male;
                    }
                    else
                    {
                        return ESexualPreference.Female;
                    }
                }
            }
            else
            {
                // Heterosexual
                if (Gender == Gender.Male)
                {
                    return ESexualPreference.Female;
                }
                else
                {
                    return ESexualPreference.Male;
                }
            }
        }

        private List<Trait> GetTraitsFromParents(List<Trait> motherTraits, List<Trait> fatherTraits)
        {
            List<Trait> childTraits = new List<Trait>();
            foreach (var motherTrait in motherTraits)
            {
                if (Helper.RollDice(50))
                {
                    childTraits.Add(motherTrait);
                }
            }
            
            foreach (var fatherTrait in fatherTraits)
            {
                if (Helper.RollDice(50))
                {
                    childTraits.Add(fatherTrait);
                }
            }

            childTraits = childTraits.Distinct().ToList();
            return childTraits;
        }
    }
}
