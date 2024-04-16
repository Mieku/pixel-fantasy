using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace InfinityPBR.Modules
{
    public static class Utilities
    {
        private static ItemObject[] _itemObjectCached = { };
        private static ItemAttribute[] _itemAttributeCached = { };
        private static Stat[] _statsCached = { };
        private static Condition[] _conditionsCached = { };
        private static Quest[] _questsCached = { };
        private static QuestReward[] _questsRewardsCached = { };
        private static MasteryLevels[] _masteryLevelsCached = { };
        private static LootItems[] _lootBoxItemsCached = { };
        private static LootBox[] _lootBoxesCached = { };

        public enum Rounding
        {
            None,
            Floor,
            Ceiling,
            Round
        }

        public static readonly string[] RoundingOptions = Enum.GetNames(typeof(Rounding));

        /// <summary>
        /// Round any float to a specified number of decimals, with a specified rounding option.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="decimals"></param>
        /// <param name="roundingOption"></param>
        /// <returns></returns>
        public static float RoundThis(float value, int decimals, Rounding roundingOption = Rounding.Round) => roundingOption switch
        {
            Rounding.None => RoundToDecimal(value, decimals),
            Rounding.Floor => (float)Math.Floor(value * Mathf.Pow(10, decimals)) / Mathf.Pow(10, decimals),
            Rounding.Ceiling => (float)Math.Ceiling(value * Mathf.Pow(10, decimals)) / Mathf.Pow(10, decimals),
            Rounding.Round => (float)Math.Round(value, decimals, MidpointRounding.AwayFromZero),
            _ => RoundToDecimal(value, decimals)
        };

        /// <summary>
        /// Given a value and decimals, will round the value to the specified number of decimals.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="decimals"></param>
        /// <returns></returns>
        public static float RoundToDecimal(float value, int decimals = 2)
        {
            if (decimals < 0) return value;
            if (decimals == 0) return Mathf.Round(value);
            
            var multiplier = (int)Mathf.Pow(10, decimals);
            return Mathf.Round(value * multiplier) / multiplier;
        }
        
        /// <summary>
        /// Provides a return value between min and max, defaulting to 0f and 1f.
        /// </summary>
        /// <param name="min"></param>
        /// <param name="max"></param>
        /// <returns></returns>
        public static float RandomFloat(float min = 0f, float max = 1f) => UnityEngine.Random.Range(min, max);

        /// <summary>
        /// Returns a random value between min and max, with multiple attempts and a best-of option. Default is 2 rolls,
        /// and that a higher result is better.
        /// </summary>
        /// <param name="higherIsBetter"></param>
        /// <param name="rolls"></param>
        /// <param name="min"></param>
        /// <param name="max"></param>
        /// <returns></returns>
        public static float RandomFloatBestOf(bool higherIsBetter = true, int rolls = 2, float min = 0f, float max = 1f)
        {
            var value = RandomFloat(min, max);

            for(int i = 1; i < rolls; i++)
            {
                var newValue = RandomFloat(min, max);
                if ((newValue > value && higherIsBetter) || (newValue < value && !higherIsBetter))
                    value = newValue;
            }
            
            return value;
        }
        
        // Reruns a random integer between min and max.
        public static int RandomInt(int min = 0, int max = 100) => UnityEngine.Random.Range(min, max);
        
        // ------------------------------------------------------------------------------------
        // DO NOT USE THESE METHODS AT RUN TIME!!!
        // ------------------------------------------------------------------------------------

        public static IEnumerable<string> GetStatsAndSkillsKeyValuesOfType(string objectType)
        {
            return GetStatsArrayOfType(objectType).SelectMany(x => x.dictionaries.keyValues).Select(x => x.key).Distinct();
        }
        
        public static Stat[] GetStatsArrayOfType(string objectType)
        {
            return StatsArray().Where(x => x.objectType == objectType).ToArray();
        }
        
        public static Stat GetStatsAndSkills(string objectName)
        {
            return StatsArray().FirstOrDefault(x => x.objectName == objectName);
        }
        
        public static Stat GetStatByUid(string uid)
        {
            return StatsArray().FirstOrDefault(x => x.Uid() == uid);
        }
        
        public static ItemObject[] GetItemObjectArray()
        {
            return ItemObjectArray();
        }
        
        public static Quest[] GetQuestObjectArray()
        {
            return QuestObjectArray();
        }
        
        public static ItemAttribute[] GetItemAttributeArray()
        {
            return ItemAttributeArray();
        }
        
        public static ItemAttribute GetItemAttribute(string uid)
        {
            return GetItemAttributeArray().FirstOrDefault(x => x.Uid() == uid);
        }
        
        public static IEnumerable<Stat> GetTrainableStatsAndSkillsArray()
        {
            return StatsArray().Where(x => x.canBeTrained).ToArray();
        }
        
        public static IEnumerable<Stat> GetStatsArray()
        {
            return StatsArray();
        }
        
        public static Stat[] StatsArray()
        {
#if UNITY_EDITOR
            int i = 0;
            string[] guids1 = AssetDatabase.FindAssets("t:InfinityPBR.Modules.StatsAndSkills", null);
            var allThings = new Stat[guids1.Length];
            
            foreach (string guid1 in guids1)
            {
                allThings[i] = AssetDatabase.LoadAssetAtPath<Stat>(AssetDatabase.GUIDToAssetPath(guid1));
                i++;
            }

            return allThings;
#else
            Debug.LogError("The method called only runs in the editor.");
            return default;
#endif
        }
        
        public static Quest[] QuestObjectArray()
        {
#if UNITY_EDITOR
            int i = 0;
            string[] guids1 = AssetDatabase.FindAssets("t:InfinityPBR.Modules.Quest", null);
            var allThings = new Quest[guids1.Length];
            
            foreach (string guid1 in guids1)
            {
                allThings[i] = AssetDatabase.LoadAssetAtPath<Quest>(AssetDatabase.GUIDToAssetPath(guid1));
                i++;
            }

            return allThings;
#else
            Debug.LogError("The method called only runs in the editor.");
            return default;
#endif
        }
        
        public static ItemObject[] ItemObjectArray()
        {
#if UNITY_EDITOR
            int i = 0;
            string[] guids1 = AssetDatabase.FindAssets("t:InfinityPBR.Modules.ItemObject", null);
            var allThings = new ItemObject[guids1.Length];
            
            foreach (string guid1 in guids1)
            {
                allThings[i] = AssetDatabase.LoadAssetAtPath<ItemObject>(AssetDatabase.GUIDToAssetPath(guid1));
                i++;
            }

            return allThings;
#else
            Debug.LogError("The method called only runs in the editor.");
            return default;
#endif
        }
        
        public static ItemAttribute[] ItemAttributeArray()
        {
#if UNITY_EDITOR
            int i = 0;
            string[] guids1 = AssetDatabase.FindAssets("t:InfinityPBR.Modules.ItemAttribute", null);
            var allThings = new ItemAttribute[guids1.Length];
            
            foreach (string guid1 in guids1)
            {
                allThings[i] = AssetDatabase.LoadAssetAtPath<ItemAttribute>(AssetDatabase.GUIDToAssetPath(guid1));
                i++;
            }

            return allThings;
#else
            Debug.LogError("The method called only runs in the editor.");
            return default;
#endif
        }
        
        public static LootBox[] GetLootBoxArray()
        {
#if UNITY_EDITOR
            if (_lootBoxesCached.Length > 0) return _lootBoxesCached;
            
            int i = 0;
            string[] guids1 = AssetDatabase.FindAssets("t:InfinityPBR.Modules.LootBox", null);
            var allItems = new LootBox[guids1.Length];
            
            foreach (string guid1 in guids1)
            {
                allItems[i] = AssetDatabase.LoadAssetAtPath<LootBox>(AssetDatabase.GUIDToAssetPath(guid1));
                i++;
            }

            return allItems;
#else
            Debug.LogError("The method called only runs in the editor.");
            return default;
#endif
        }

        public static string StatHash()
        {
            var hashString = "";
            foreach (var stat in GetStatArray())
                hashString = $"{hashString}{stat.objectName}";
            
            foreach (var type in GetStatArray().Select(x => x.objectType).Distinct().ToArray())
                hashString = $"{hashString}{type}";
            
            return Hash128.Compute(hashString).ToString();
        }

        public static string ItemObjectHash()
        {
            var hashString = "";
            foreach (var itemObject in GetItemObjectArray())
                hashString = $"{hashString}{itemObject.objectName}";
            
            foreach (var type in GetItemObjectArray().Select(x => x.objectType).Distinct().ToArray())
                hashString = $"{hashString}{type}";
            
            return Hash128.Compute(hashString).ToString();
        }
        
        public static string ItemAttributeHash()
        {
            var hashString = "";
            foreach (var itemAttribute in GetItemAttributeArray())
                hashString = $"{hashString}{itemAttribute.objectName}";
            
            foreach (var type in GetItemAttributeArray().Select(x => x.objectType).Distinct().ToArray())
                hashString = $"{hashString}{type}";
            
            return Hash128.Compute(hashString).ToString();
        }

        public static string QuestHash()
        {
            var hashString = "";
            foreach (var itemObject in GetQuestArray())
                hashString = $"{hashString}{itemObject.objectName}";
            
            foreach (var type in GetQuestArray().Select(x => x.objectType).Distinct().ToArray())
                hashString = $"{hashString}{type}";
            
            return Hash128.Compute(hashString).ToString();
        }
        
        public static Stat[] GetStatArray()
        {
            return StatArray();
        }
        
        public static Stat[] StatArray()
        {
#if UNITY_EDITOR
            if (_statsCached.Length > 0) return _statsCached;
            
            int i = 0;
            string[] guids1 = AssetDatabase.FindAssets("t:InfinityPBR.Modules.Stat", null);
            var allThings = new Stat[guids1.Length];
            
            foreach (string guid1 in guids1)
            {
                allThings[i] = AssetDatabase.LoadAssetAtPath<Stat>(AssetDatabase.GUIDToAssetPath(guid1));
                i++;
            }

            return allThings;
#else
            Debug.LogError("The method called only runs in the editor.");
            return default;
#endif
        }
        
        public static Quest[] GetQuestArray() => QuestArray();
        public static Quest[] QuestArray()
        {
#if UNITY_EDITOR
            if (_questsCached.Length > 0) return _questsCached;
            
            int i = 0;
            string[] guids1 = AssetDatabase.FindAssets("t:InfinityPBR.Modules.Quest", null);
            var allThings = new Quest[guids1.Length];
            
            foreach (string guid1 in guids1)
            {
                allThings[i] = AssetDatabase.LoadAssetAtPath<Quest>(AssetDatabase.GUIDToAssetPath(guid1));
                i++;
            }

            return allThings;
#else
            Debug.LogError("The method called only runs in the editor.");
            return default;
#endif
        }
        
        public static QuestReward[] GetQuestRewardArray() => QuestRewardArray();
        public static QuestReward[] QuestRewardArray()
        {
#if UNITY_EDITOR
            if (_questsRewardsCached.Length > 0) return _questsRewardsCached;
            
            int i = 0;
            string[] guids1 = AssetDatabase.FindAssets("t:InfinityPBR.Modules.QuestReward", null);
            var allThings = new QuestReward[guids1.Length];
            
            foreach (string guid1 in guids1)
            {
                allThings[i] = AssetDatabase.LoadAssetAtPath<QuestReward>(AssetDatabase.GUIDToAssetPath(guid1));
                i++;
            }

            return allThings;
#else
            Debug.LogError("The method called only runs in the editor.");
            return default;
#endif
        }
        
        public static Condition[] GetConditionArray() => ConditionArray();
        public static Condition[] ConditionArray()
        {
#if UNITY_EDITOR
            if (_conditionsCached.Length > 0) return _conditionsCached;
            
            int i = 0;
            // Jan 7 2024 -- explicitly call "InfinityPBR.Modules." in cases of projects that have another "condition"
            string[] guids1 = AssetDatabase.FindAssets("t:InfinityPBR.Modules.Condition", null);
            var allThings = new Condition[guids1.Length];
            
            foreach (string guid1 in guids1)
            {
                allThings[i] = AssetDatabase.LoadAssetAtPath<Condition>(AssetDatabase.GUIDToAssetPath(guid1));
                i++;
            }

            return allThings;
#else
            Debug.LogError("The method called only runs in the editor.");
            return default;
#endif
        }
        
        public static LookupTable[] GetLookupTableArray()
        {
            return LookupTableArray();
        }
        
        public static LookupTable[] LookupTableArray()
        {
#if UNITY_EDITOR
            int i = 0;
            string[] guids1 = AssetDatabase.FindAssets("t:InfinityPBR.Modules.LookupTable", null);
            var allItems = new LookupTable[guids1.Length];
            
            foreach (string guid1 in guids1)
            {
                allItems[i] = AssetDatabase.LoadAssetAtPath<LookupTable>(AssetDatabase.GUIDToAssetPath(guid1));
                i++;
            }

            return allItems;
#else
            Debug.LogError("The method called only runs in the editor.");
            return default;
#endif
        }
        
        public static IEnumerable<int> GenerateIncrementalValues(int start, int increment, int count)
        {
            var result = new List<int>();

            for (var i = 0; i < count; i++)
                result.Add(start + i * increment);

            return result;
        }
        
        public static IEnumerable<float> GenerateEvenlySpacedValues(int min, int max, int numberOfPairs, int decimalPlaces = 0)
        {
            var step = (double)(max - min) / (numberOfPairs - 1); // Subtract 1 because we include both ends
            var result = new List<float>();

            for (var i = 0; i < numberOfPairs; i++)
            {
                var value = min + i * step;
                result.Add((float)Math.Round(value, decimalPlaces));
            }

            return result;
        }

        public static IEnumerable<float> GenerateEvenlySpacedValuesFromCurve(AnimationCurve curve, float min, float max, int numberOfPairs, int decimalPlaces = 0)
        {
            var step = 1.0 / (numberOfPairs - 1); // Subtract 1 because we include both ends
            var result = new List<float>();

            for (var i = 0; i < numberOfPairs; i++)
            {
                var t = i * step; // t ranges from 0 to 1
                var value = curve.Evaluate((float)t); // sample the curve at t
                value = min + (value * (max - min)); // remap the value from [0, 1] to [min, max]
                result.Add((float)Math.Round(value, decimalPlaces));
            }

            return result;
        }
    }
}