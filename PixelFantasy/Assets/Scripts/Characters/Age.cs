using System;
using UnityEngine;

namespace Characters
{
    [Serializable]
    public class Age
    {
        public int CurrentAge;
        private RacialAgeData _racialAgeData;
        
        public Age(int currentAge, RacialAgeData racialAgeData)
        {
            CurrentAge = currentAge;
            _racialAgeData = racialAgeData;
        }

        public EMaturityStage MaturityStage
        {
            get
            {
                if (CurrentAge <= _racialAgeData.ChildMaxAge)
                {
                    return EMaturityStage.Child;
                }

                if (CurrentAge <= _racialAgeData.AdultMaxAge)
                {
                    return EMaturityStage.Adult;
                }

                return EMaturityStage.Senior;
            }
        }

        public void IncrementAge()
        {
            CurrentAge++;
        }
    }

    [Serializable]
    public class RacialAgeData
    {
        public int ChildMaxAge;
        public int AdultMaxAge;
        public int LifeExpectancy;
    }
    
    public enum EMaturityStage
    {
        Child = 0,
        Adult = 1,
        Senior = 2,
    }
}
