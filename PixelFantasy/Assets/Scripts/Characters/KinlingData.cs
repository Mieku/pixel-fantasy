using System;
using Items;
using ScriptableObjects;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Characters
{
    [Serializable]
    public class KinlingData
    {
        private string _uid;
        public string UID
        {
            get
            {
                if (string.IsNullOrEmpty(_uid))
                {
                    _uid = $"Kinling_{Firstname}_{Lastname}_{Guid.NewGuid()}";
                }
                return _uid;
            }
            set => _uid = value;
        }
        
        [BoxGroup("General")] public string Firstname;
        [BoxGroup("General")] public string Lastname;
        [BoxGroup("General")] public EMaturityStage MaturityStage;
        [BoxGroup("General")] public Gender Gender;
        [BoxGroup("General")] public ESexualPreference SexualPreference;
        
        [BoxGroup("Appearance")] public AppearanceState Appearance;
        [BoxGroup("Gear")] public KinlingGearData Gear;
    }
}
