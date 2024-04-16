using System;
using UnityEngine;

namespace InfinityPBR.Modules
{
    [Serializable]
    public class GameConditionStatEffect
    {
        public string Uid => _uid;
        public Stat Stat => GetStat();
        public float PointEffect => GetPointEffect();
        public float ValueEffect => GetValueEffect();
        public float ProficiencyEffect => GetProficiencyEffect();
        
        [SerializeField] [HideInInspector] private string _uid;
        [SerializeField] [HideInInspector] private float _pointEffect;
        [SerializeField] [HideInInspector] private float _valueEffect;
        [SerializeField] [HideInInspector] private float _proficiencyEffect;
        private Stat _stat;

        public GameConditionStatEffect(ConditionPointEffect effect, IHaveStats owner)
        {
            _stat = effect.stat;
            _uid = effect.stat.Uid();
            _pointEffect = effect.ComputeEffect(owner);
        }
        
        public GameConditionStatEffect(ModificationLevel modificationLevel, Stat stat, IHaveStats owner)
        {
            float valueEffect;
            float proficiencyEffect;
            (valueEffect, proficiencyEffect) = modificationLevel.GetEffectOn(stat.Uid(), 0, 0, owner);
            
            _stat = stat;
            _uid = stat.Uid();
            _valueEffect = valueEffect;
            _proficiencyEffect = proficiencyEffect;
        }
        
        private Stat GetStat()
        {
            if (_stat != null) return _stat;

            _stat = GameModuleRepository.Instance.Get<Stat>(Uid);
            return _stat;
        }

        private float GetPointEffect() => _pointEffect;
        private float GetValueEffect() => _valueEffect;
        private float GetProficiencyEffect() => _proficiencyEffect;
    }
}