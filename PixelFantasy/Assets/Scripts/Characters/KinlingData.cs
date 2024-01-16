using System;
using System.Collections.Generic;
using Items;
using ScriptableObjects;
using Sirenix.OdinInspector;
using Systems.Traits.Scripts;
using UnityEngine;
using UnityEngine.Serialization;

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
        [BoxGroup("Traits")] public List<Trait> Traits;
        [FormerlySerializedAs("Abilities")] [BoxGroup("Abilities")] public Stats Stats;
        [BoxGroup("Family")] public string Partner;
        [BoxGroup("Family")] public List<string> Children;
        
        [BoxGroup("General")] [Button("Create UID")]
        private void GenerateUID()
        {
            _uid = $"Kinling_{Firstname}_{Lastname}_{Guid.NewGuid()}";
        }
    }
}
