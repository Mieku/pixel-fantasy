using System;
using Newtonsoft.Json;
using ScriptableObjects;
using Random = UnityEngine.Random;

namespace Systems.Stats.Scripts
{
    public class SkillData
    {
        public int Level;
        public float TotalExp;
        public ESkillPassion Passion;
        public string SkillSettingsID;

        [JsonIgnore] public bool Incapable => Level == 0;
        [JsonIgnore] public bool IsMaxLevel => Level == 10;
        [JsonIgnore] public SkillSettings Settings => GameSettings.Instance.LoadSkillSettings(SkillSettingsID);
        
        public void Init(SkillSettings settings)
        {
            SkillSettingsID = settings.name;
        }

        public void RandomlyAssignPassion()
        {
            // Generate a random number between 1 and 100
            int diceRoll = Random.Range(1, 101);

            // Retrieve the probabilities from the game settings
            int minorChance = GameSettings.Instance.ExpSettings.ChanceForMinorPassion;
            int majorChance = GameSettings.Instance.ExpSettings.ChanceForMajorPassion;

            // Determine the passion level based on the dice roll
            if (diceRoll <= 100 - (minorChance + majorChance))
            {
                Passion = ESkillPassion.None; // Most common scenario with no passion
            }
            else if (diceRoll <= 100 - majorChance)
            {
                Passion = ESkillPassion.Minor; // Less common, minor passion
            }
            else
            {
                Passion = ESkillPassion.Major; // Least common, major passion
            }
        }

        [JsonIgnore]
        public float CurrentExp
        {
            get
            {
                if (Incapable) return 0;

                var minExpForLvl = GameSettings.Instance.ExpSettings.GetMinExpForLevel(Level);
                float curExp = TotalExp - minExpForLvl;
                return curExp;
            }
        }
        
        public float GetPercentExp()
        {
            if (IsMaxLevel)
            {
                return 1;
            }

            if (Incapable)
            {
                return 0;
            }

            var expNextLevel = GameSettings.Instance.ExpSettings.GetMinExpForLevel(Level + 1);
            return CurrentExp / expNextLevel;
        }

        [JsonIgnore]
        public string PercentString
        {
            get
            {
                int percent = (int)(GetPercentExp() * 100f);
                return $"{percent}";
            }
        }

        [JsonIgnore]
        public string RankString
        {
            get
            {
                if (Incapable) return "Incapable";

                var rankName = GameSettings.Instance.ExpSettings.DetermineLevelName(Level);
                
                return $"Lv {Level} : {rankName}";
            }
        }

        [JsonIgnore]
        public string ExpString
        {
            get
            {
                if (IsMaxLevel) return "MAX";
                if (Incapable) return "";

                int curExp = (int) CurrentExp;
                var expNextLevel = GameSettings.Instance.ExpSettings.GetMinExpForLevel(Level + 1);

                return $"{FormatNumber(curExp)} / {FormatNumber(expNextLevel)}";
            }
        }

        private string FormatNumber(int number)
        {
            if (number >= 1000000)
                return (number / 1000000D).ToString("0.#M"); // Converts millions
            if (number >= 1000)
                return (number / 1000D).ToString("0.#k"); // Converts thousands

            return number.ToString(); // Returns the number as is if it's less than 1000
        }
    }

    public enum ESkillPassion
    {
        None = 0,
        Minor = 1,
        Major = 2
    }
}
