using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using InfinityPBR;
using UnityEditor;
using InfinityPBR.Modules;
using UnityEngine;
using static InfinityPBR.Modules.GameModuleUtilities;
using static InfinityPBR.Modules.InfinityEditorGameModules;

namespace InfinityPBR.Modules
{
    [CustomEditor(typeof(StartingStats))]
    [CanEditMultipleObjects]
    [Serializable]
    public class StartingStatsEditor : InfinityEditor
    {
        private Stat[] _availableStats;

        private int SelectedStatIndex
        {
            get => EditorPrefs.GetInt("SelectedStatIndex", 0);
            set => EditorPrefs.SetInt("SelectedStatIndex", value);
        }
        
        private string SearchString
        {
            get => SessionState.GetString("Stat Starting Search String", "");
            set => SessionState.SetString("Stat Starting Search String", value);
        }

        private int SearchTypeIndex
        {
            get => SessionState.GetInt("Stat Starting Search Type Index", -1);
            set => SessionState.SetInt("Stat Starting Search Type Index", value);
        }

        public override void OnInspectorGUI()
        {
            var startingStats = (StartingStats)target;

            Header1("Starting Stats");
            Label(
                $"<color=#999999>Add starting <color=#99ffff><b>Stat</b></color> values to any <color=#99ffff><b>IHaveStats</b></color> component on this GameObject. " +
                $"This will be done automatically when the game starts.</color> <color=#999999>There are <color=#99ffff><b>{startingStats.cachedComponents.Count} " +
                $"<b>IHaveStats</b></b></color> components on this game object, which will receive the Stats selected below.</color> \n<color=#555555><i>Components update automatically when this Inspector is viewed.</i></color>",false, true, true);
            for (var index = 0; index < startingStats.cachedComponents.Count; index++)
            {
                var component = startingStats.cachedComponents[index];
                var field = startingStats.cachedFieldNames[index];
                Label($"<color=#999999> - {component} {field}</color>", false, true, true);
            }

            BlackLine();
            ShowOptions();
            BlackLine();
            ShowAddStat();
            Space();
            ShowStats();
        }

        private void ShowOptions()
        {
            var startingStats = (StartingStats)target;
            
            Undo.RecordObject(startingStats, "Change Print to Console"); 
            startingStats.writeToConsole = LeftCheck("Write to Console", startingStats.writeToConsole);
        }

        private void OnEnable()
        {
            ResetCache();
            CacheStatsComponents();
        }

        private void ResetCache()
        {
            var startingStats = (StartingStats)target;

            startingStats.startingStatValues.RemoveAll(x => x.stat == null);
        }

        private void ShowStats()
        {
            var startingStats = (StartingStats)target;

            StartRow();
            Label("", 25);
            Label("", 25);
            Label("Stat", 150, true);
            Label("Min", 50, true);
            Label("Max", 50, true);
            Label("Round", 50, true);
            EndRow();

            for (int i = startingStats.startingStatValues.Count - 1;
                 i >= 0;
                 i--) // Reverse loop to safely remove items while iterating.
            {
                StartRow();
                if (XButton())
                {
                    Undo.RecordObject(startingStats,
                        "Remove Stat"); // Here you record the undo on the StartingStats object
                    startingStats.startingStatValues.RemoveAt(i);
                    ResetCache();
                    ExitGUI();
                }
                
                PingButton(startingStats.startingStatValues[i].stat);

                Label(startingStats.startingStatValues[i].stat.ObjectName, 150);

                // Record undo before changing minPoints
                Undo.RecordObject(startingStats.startingStatValues[i].stat,
                    "Change Min Points"); // Here you record the undo on the Stat object
                float newMinPoints = DelayedFloat(startingStats.startingStatValues[i].minPoints, 50);
                if (newMinPoints != startingStats.startingStatValues[i].minPoints)
                {
                    startingStats.startingStatValues[i].minPoints = newMinPoints;
                }

                if (startingStats.startingStatValues[i].minPoints > startingStats.startingStatValues[i].maxPoints)
                    startingStats.startingStatValues[i].maxPoints = startingStats.startingStatValues[i].minPoints;

                // Record undo before changing maxPoints
                Undo.RecordObject(startingStats.startingStatValues[i].stat,
                    "Change Max Points"); // Here you record the undo on the Stat object
                float newMaxPoints = DelayedFloat(startingStats.startingStatValues[i].maxPoints, 50);
                if (newMaxPoints != startingStats.startingStatValues[i].maxPoints)
                {
                    startingStats.startingStatValues[i].maxPoints = newMaxPoints;
                }

                if (startingStats.startingStatValues[i].maxPoints < startingStats.startingStatValues[i].minPoints)
                    startingStats.startingStatValues[i].minPoints = startingStats.startingStatValues[i].maxPoints;
                
                // Record undo before changing maxPoints
                Undo.RecordObject(startingStats.startingStatValues[i].stat,
                    "Change Rounding"); // Here you record the undo on the Stat object
                int newDecimals = Int(startingStats.startingStatValues[i].decimals, 50);
                if (newDecimals != startingStats.startingStatValues[i].decimals)
                {
                    startingStats.startingStatValues[i].decimals = newDecimals;
                }

                EndRow();
            }
        }


        private void ShowAddStat()
        {
            var startingStats = (StartingStats)target;

            BackgroundColor(Color.yellow);
            
            var searchResults = SearchBoxWithType<Stat>(SearchTypeIndex, GameModuleObjectTypes<Stat>(), " Modification");
            SearchTypeIndex = searchResults.Item1;
            SearchString = searchResults.Item2;

            StartRow();
            _availableStats = SearchResults().Where(x => !startingStats.Contains(x)).ToArray();
            
            var statNames = _availableStats.Select(x => $"[{x.objectType}] {x.objectName}").ToArray();
            var stats = _availableStats;
            if (SelectedStatIndex > statNames.Length)
                SelectedStatIndex = statNames.Length - 1;
            if (SelectedStatIndex < 0 && statNames.Length > 0)
                SelectedStatIndex = 0;

            SelectedStatIndex = Popup(SelectedStatIndex, statNames, 300);
            if (stats.Length == 0)
                Colors(Color.black, Color.grey);
            if (Button("Add", 50) && stats.Length > 0)
            {
                AddStat(_availableStats[SelectedStatIndex]);
                ResetCache();
                ExitGUI();
            }
            if (Button("Add All", 75) && stats.Length > 0)
            {
                foreach(var stat in _availableStats)
                    AddStat(stat);
                ResetCache();
                ExitGUI();
            }
            EndRow();
            
            ResetColor();
        }
        
        private List<Stat> SearchResults()
        {
            var results = new List<Stat>();
            var stats = GameModuleObjects<Stat>();
            if (SearchTypeIndex < GameModuleObjectTypes<Stat>().Length)
                stats = stats.Where(x => x.ObjectType == GameModuleObjectTypes<Stat>()[SearchTypeIndex]).ToArray();

            foreach (var stat in stats)
            {
                if (stat.ObjectName.ToLower().Contains(SearchString.ToLower()) 
                    || stat.ObjectType.ToLower().Contains(SearchString.ToLower()))
                    results.Add(stat);
            }

            return results;
        }
        
        private void AddStat(Stat newStat)
        {
            var startingStats = (StartingStats)target;
         
            if (startingStats.Contains(newStat))
            {
                Debug.LogError($"Already contains the {newStat.ObjectName} Stat.");
                return;
            }
    
            Undo.RecordObject(target, "Add Stat"); // Add this line to record the change.
            startingStats.startingStatValues.Add(new StartingStatValues(newStat));
        }
        
        /// <summary>
        /// Loops through all Monobehaviours on this object, and all fields, and will cache any fields that are IHaveStats.
        /// </summary>
        public void CacheStatsComponents()
        {
            var startingStats = (StartingStats)target;
            
            startingStats.cachedComponents.Clear();
            startingStats.cachedFieldNames.Clear();
            
            foreach (var component in startingStats.GetComponents<MonoBehaviour>())
            {
                var fields = component.GetType().GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                foreach (var field in fields)
                {
                    if (field.GetValue(component) is not IHaveStats) continue;
                    
                    startingStats.cachedComponents.Add(component);
                    startingStats.cachedFieldNames.Add(field.Name);
                }
            }
            
            EditorUtility.SetDirty(startingStats);
        }
    }
}