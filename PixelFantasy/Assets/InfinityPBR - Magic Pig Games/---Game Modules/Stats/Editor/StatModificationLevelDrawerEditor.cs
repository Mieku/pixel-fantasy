using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using static InfinityPBR.InfinityEditor;
using static InfinityPBR.Modules.InfinityEditorGameModules;
using static InfinityPBR.Modules.GameModuleUtilities;

namespace InfinityPBR.Modules
{
    public class StatModificationLevelDrawerEditor : StatModificationLevelDrawer
    {
        private int fieldWidth;
        private Vector2 _scrollPosition;
        //private int addSourceIndex = -1;
        //private int sourceCalculation = 0;
        private LookupTable[] availableLookupTables;
        private string[] availableLookupTablesNames;

        private int AddSourceIndex
        {
            get => SessionState.GetInt("Add Source Index", 0);
            set => SessionState.SetInt("Add Source Index", value);
        }
        private int SourceCalculation
        {
            get => SessionState.GetInt("Stat Modifier Source Calculation", 0);
            set => SessionState.SetInt("Stat Modifier Source Calculation", value);
        }
        
        public StatModificationLevelDrawerEditor(int fieldWidth) => this.fieldWidth = fieldWidth;

        private bool MadeCache
        {
            get => SessionState.GetBool("Stat Modifier Made Cache", false);
            set => SessionState.SetBool("Stat Modifier Made Cache", value);
        }

        public void Draw(ModificationLevel modificationLevel, Stat thisObject)
        {
            if (!MadeCache || GetBool("statModificationLevelDrawer Force Cache"))
                Cache(modificationLevel, thisObject);
            
            DrawCopyPaste(modificationLevel, thisObject);
            DrawAddStatPopup(modificationLevel, thisObject);
            DrawModifications(modificationLevel, thisObject);
            
        }

        public void Cache(ModificationLevel modificationLevel, Stat thisObject, bool force = false)
        {
            UpdateListStats(modificationLevel);
            modificationLevel.CacheModifiableStats(thisObject);
            modificationLevel.CacheSourceStats(GameModuleObjects<Stat>(true), thisObject);
            availableLookupTables = GameModuleObjects<LookupTable>();
            availableLookupTablesNames = availableLookupTables.Select(x => x.name).ToArray();
            MadeCache = true;
            SetBool("statModificationLevelDrawer Force Cache", false);
            DebugConsoleMessage("Cached Modification Level Drawer");
        }

        public void DrawSimple(ModificationLevel modificationLevel, Stat thisObject)
        {
            if (!MadeCache)
            {
                modificationLevel.CacheModifiableStats(thisObject);
                modificationLevel.CacheSourceStats(GameModuleObjects<Stat>(true), thisObject);
                availableLookupTables = GameModuleObjects<LookupTable>();
                availableLookupTablesNames = availableLookupTables.Select(x => x.name).ToArray();
                MadeCache = true;
            }
            DrawModifications(modificationLevel, thisObject);
        }

        private void DrawModifications(ModificationLevel modificationLevel, Stat thisObject)
        {
            if (modificationLevel?.targets == null) return;
            if (modificationLevel.targets.Count == 0) return;
            
            
            StartRow();
            StartVertical();
            ShowLeftColumn(modificationLevel, thisObject);
            EndVerticalBox();
            
            StartVertical();
            StartRow();
            ShowModColumn(modificationLevel, null, "Base", thisObject);
            if (thisObject != null)
                ShowModColumn(modificationLevel, null, "Per Skill Point", thisObject);
            foreach (Stat stat in modificationLevel.sources)
            {
                if (stat == null) continue;
                ShowModColumn(modificationLevel, stat, stat.objectName, thisObject);
            }

            DrawAddSource(modificationLevel, thisObject);
            
            EndRow();
            EndVerticalBox();
            EndRow();
        }

        private void DrawAddSource(ModificationLevel modificationLevel, Stat thisObject)
        {
            if (modificationLevel == null) return;
            
            BackgroundColor(Color.yellow);
            StartVertical();

            if (modificationLevel.availableSources.Length == 0)
            {
                Label("There are no Stats available.", 150, false, true);
                EndVerticalBox();
                BackgroundColor(Color.white);
                return;
            }
            
            if (AddSourceIndex >= modificationLevel.availableSources.Length)
                AddSourceIndex = modificationLevel.availableSources.Length - 1;
            if (modificationLevel.availableSources.Length > 0 && AddSourceIndex < 0)
                AddSourceIndex = 0;
            
            Label("Add a source from another Stat");
            
            AddSourceIndex = Popup(AddSourceIndex, modificationLevel.availableSourcesNames, 150);
            if (Button("Add", 150))
            {
                modificationLevel.AddModification(modificationLevel.availableSources[AddSourceIndex], modificationLevel.targets[0], 0f, 0f);
                
                BuildAffectsLists();
                
                modificationLevel.CacheModifiableStats(thisObject);
                modificationLevel.CacheSourceStats(GameModuleObjects<Stat>(true), thisObject);
            }
          
            EndVerticalBox();

            BackgroundColor(Color.white);
        }

        private void ShowModColumn(ModificationLevel modificationLevel, Stat source, string sourceName, Stat thisObject)
        {
            var isBase = sourceName == "Base";
            var isPerSkillPoint = sourceName == "Per Skill Point";

            var headerTooltip = "";
            if (isBase)
            {
                headerTooltip =
                    "The target Stats will be modified by the specific values set in this column.";
            }
            if (isPerSkillPoint)
            {
                headerTooltip =
                    "The target Stats will be modified by the specific values multiplied by the \"Points\" value " + 
                    $"of the {thisObject.objectName} Stat. Note, this is not the \"Final Stat\". The Point value " +
                    "is intended to be used as something a player would modify during a game, and contributes to the " +
                    "final stat.";
            }

            StartVerticalBox(120);
            StartRow();
            if (!isBase && !isPerSkillPoint)
            {
                BackgroundColor(Color.red);
                if (Button(symbolX, 25))
                {
                    modificationLevel.RemoveSource(source);
                    BuildAffectsLists();
                    modificationLevel.CacheModifiableStats(thisObject);
                    modificationLevel.CacheSourceStats(GameModuleObjects<Stat>(true), thisObject);
                    ExitGUI();
                }
                BackgroundColor(Color.white);
            }
            Label($"{sourceName} {symbolInfo}",headerTooltip, 120, true);
            EndRow();
            StartRow();
            if (!isBase && !isPerSkillPoint) LabelSized("", 20, 10);
            LabelSized("Value", 60, 10);
            LabelSized("Proficiency", 60, 10);
            EndRow();
            
            foreach (Stat target in modificationLevel.targets)
            {
                if (!isBase && !isPerSkillPoint)
                {
                    if (CheckForLookupTableConflict(modificationLevel, source, target, sourceName, thisObject))
                        continue;
                }
                float value;
                float proficiency;
                (value, proficiency) = modificationLevel.GetValueAndProficiency(source, target, isBase, isPerSkillPoint);
                
                StartRow();
                if (!isBase && !isPerSkillPoint)
                {
                    if (source == target)
                    {
                        DebugConsoleMessage($"Conflict {source.objectName}");
                    }
                    modificationLevel.SetShowInInspector(source, target,
                        Check(modificationLevel.GetShowInInspector(source, target), 20));
                    if (!modificationLevel.GetShowInInspector(source, target))
                    {
                        modificationLevel.SetComputationIndex(source, target, 5);
                        EndRow();
                        continue;
                    }
                }

                if (modificationLevel.GetComputationIndex(source, target) != 6)
                {
                    modificationLevel.SetValue(source, target, Float(value, 60), isBase, isPerSkillPoint);
                    modificationLevel.SetProficiency(source, target, Float(proficiency, 60), isBase, isPerSkillPoint);
                }
                if (!isBase && !isPerSkillPoint)
                    ShowSourceSelectors(modificationLevel, source, sourceName, thisObject, target);
                EndRow();
            }
            EndVerticalBox();
        }

        private void ShowSourceSelectors(ModificationLevel modificationLevel, Stat source, string sourceName, Stat thisObject, Stat target)
        {

            if (!modificationLevel.GetShowInInspector(source, target)) return;
            
            modificationLevel.SetComputationIndex(source, target,
                Popup(modificationLevel.GetComputationIndex(source, target),
                    new[] {"Raw", "Points", "Proficiency", "Final Stat", "X * Value of", "X * Value", "No effect"}, 125));

            // If there are no lookuptable avilable, then SetComputationIndex(source, target) to 4
            if ((availableLookupTables == null 
                 || availableLookupTables.Length == 0)
                && (modificationLevel.GetComputationIndex(source, target) == 4 
                    || modificationLevel.GetComputationIndex(source, target) == 5))
            {
                DebugConsoleMessage($"No Lookup Tables are available, changing the computation method.");
                modificationLevel.SetComputationIndex(source, target, 3);
            }
            
            // Show selector for Lookup Table multiplier
            if (modificationLevel.GetComputationIndex(source, target) == 4)
            {
                //LabelSized("of", 15, 10);
                int multiplierIndex = modificationLevel.MultiplierIndex(source, target);
                int newMultiplierIndex = Popup(multiplierIndex, modificationLevel.availableModifiersNames, 105);

                if (newMultiplierIndex != multiplierIndex)
                {
                    if (modificationLevel.availableModifiers[newMultiplierIndex] == target)
                    {
                        Debug.LogWarning($"You can't affect {target.objectName} with {modificationLevel.availableModifiers[newMultiplierIndex].objectName} (itself)!");
                    }
                    else
                    {
                        modificationLevel.SetLookupTableMultiplier(source, target,
                            modificationLevel.availableModifiers[newMultiplierIndex].Uid());
                    }
                }

                LabelSized("in", 15, 10);
                int lookupTableIndex = modificationLevel.GetLookupTableIndex(source, target, availableLookupTables);
                int newLookupTableIndex = Popup(lookupTableIndex, availableLookupTablesNames, 105);

                modificationLevel.SetLookupTableUid(source, target, availableLookupTables[newLookupTableIndex].Uid());
                
            }

            if (modificationLevel.GetComputationIndex(source, target) == 5)
            {
                LabelSized("in", 15, 10);
                int lookupTableIndex = modificationLevel.GetLookupTableIndex(source, target, availableLookupTables);
                int newLookupTableIndex = Popup(lookupTableIndex, availableLookupTablesNames, 105);

                
                if (availableLookupTables == null)
                    return;
                
                modificationLevel.SetLookupTableUid(source, target, availableLookupTables[newLookupTableIndex].Uid());
            }
        }

        private bool CheckForLookupTableConflict(ModificationLevel modificationLevel, Stat source, Stat target, string sourceName, Stat thisObject)
        {
            if (source == target)
            {
                modificationLevel.RemoveMods(source, target);
                LabelGrey("Circular Logic", 120);
                return true;
            }
            if ((modificationLevel.GetComputationIndex(source, target) == 4 && modificationLevel.GetLookupTableMultiplier(source, target) == target.Uid()) || 
                modificationLevel.GetComputationIndex(source, target) == 5 && source.Uid() == target.Uid())
            {
                modificationLevel.RemoveMods(source, target);
                LabelGrey("Circular Logic", 120);
                ShowSourceSelectors(modificationLevel, source, sourceName, thisObject, target);
                return true;
            }

            return false;
        }

        private void ShowLeftColumn(ModificationLevel modificationLevel, Stat thisObject)
        {
            StartVerticalBox(125);
            LabelSized("", 125, 8);
            Label($"TARGETS {symbolInfo}","These Stats will have their final stat values modified by the values set " +
                              "in the table to the right.", 125, true);
            foreach (var target in modificationLevel.targets)
            {
                StartRow();
                BackgroundColor(Color.red);
                if (Button(symbolX, 20, 17))
                {
                    modificationLevel.RemoveTarget(target);
                    BuildAffectsLists();
                    modificationLevel.CacheModifiableStats(thisObject);
                    modificationLevel.CacheSourceStats(GameModuleObjects<Stat>(true), thisObject);
                    ExitGUI();
                }
                BackgroundColor(Color.white);
                Label(target.objectName,target.objectName, 100, true);
                EndRow();
            }

            Label("",125); // Vertical space
            EndVerticalBox();
        }

        private void Paste(ModificationLevel modificationLevel, Stat thisObject)
        {
            ModificationLevel fromJson = JsonUtility.FromJson<ModificationLevel>(GetString("Mod Copy"));

            // Clear existing
            modificationLevel.modifications.Clear();
            modificationLevel.sources.Clear();
            modificationLevel.targets.Clear();
                
            // Try adding modifications one at a time
            foreach (StatModification mod in fromJson.modifications)
            {
                if (!modificationLevel.TryAddModification(mod, thisObject))
                    DebugConsoleMessage("Could not add a modification to avoid circular logic conflict: " +
                              (mod.source == null ? "Null Source" : mod.source.objectName) + " and " +
                              mod.target.objectName);
            }
                
            BuildAffectsLists();
            modificationLevel.CacheModifiableStats(thisObject);
            modificationLevel.CacheSourceStats(GameModuleObjects<Stat>(true), thisObject);
            PasteStructure(modificationLevel, thisObject);
        }

        private void PasteStructure(ModificationLevel modificationLevel, Stat thisObject)
        {
            ModificationLevel fromJson = JsonUtility.FromJson<ModificationLevel>(GetString("Mod Copy"));

                // Add sources / targets
                var copySources = fromJson.sources
                    .Distinct()
                    .ToList();
                
                var copyTargets = fromJson.targets
                    .Distinct()
                    .ToList();

                foreach (Stat copyTarget in copyTargets)
                {
                    //DebugConsoleMessage($"Trying target {copyTarget}");
                    if (modificationLevel.targets.FirstOrDefault(x => x == copyTarget) == null)
                    {
                       // DebugConsoleMessage("It was not found, do add");
                        StatModification mod =
                            new StatModification(null, copyTarget, 0, 0, 0, true);
                        if (!modificationLevel.TryAddModification(mod, thisObject))
                            DebugConsoleMessage("Could not add a modification to avoid circular logic conflict: " +
                                      (mod.source == null ? "Null Source" : mod.source.objectName) + " and " +
                                      mod.target.objectName);
                    }
                }

                foreach (Stat copySource in copySources)
                {
                    //DebugConsoleMessage($"Trying source {copySource}");
                    if (modificationLevel.sources.FirstOrDefault(x => x == copySource) == null)
                    {
                        DebugConsoleMessage("It was not found, do add");
                        StatModification mod = new StatModification(copySource, modificationLevel.targets[0], 0,0);
                        if (!modificationLevel.TryAddModification(mod, thisObject))
                            DebugConsoleMessage("Could not add a modification to avoid circular logic conflict: " +
                                      (mod.source == null ? "Null Source" : mod.source.objectName) + " and " +
                                      mod.target.objectName);
                    }
                    
                    // Computation Source Etc
                    foreach (Stat source in modificationLevel.sources)
                    {
                        if (source != copySource) continue;
                        foreach (Stat copyTarget in copyTargets)
                        {
                            DebugConsoleMessage($"Checking source {source.objectName} and target {copyTarget.objectName} against copy {copySource.objectName}");
                            modificationLevel.SetComputationIndex(source, copyTarget, fromJson.GetComputationIndex(copySource, copyTarget));
                            modificationLevel.SetLookupTableUid(source, copyTarget,
                                availableLookupTables[fromJson.GetLookupTableIndex(copySource, copyTarget, availableLookupTables)].Uid());
                            modificationLevel.SetLookupTableMultiplier(source, copyTarget,
                                fromJson.GetLookupTableMultiplier(copySource, copyTarget));
                            modificationLevel.SetShowInInspector(source, copyTarget,
                                fromJson.GetShowInInspector(copySource, copyTarget));
                        }
                    }
                }

                BuildAffectsLists();
                modificationLevel.CacheModifiableStats(thisObject);
                modificationLevel.CacheSourceStats(GameModuleObjects<Stat>(true), thisObject);
        }

        private void DrawCopyPaste(ModificationLevel modificationLevel, Stat thisObject)
        {
            StartRow();
            BackgroundColor(Color.cyan);
            if (Button("Copy", 100))
            {
                var jsonString = JsonUtility.ToJson(modificationLevel);
                SetString("Mod Copy", jsonString);
            }

            if (HasKey("Mod Copy") && Button("Paste Modification List","Replace this entire list and data " +
                                                                       "with the copied data.", 200))
            {
                Paste(modificationLevel, thisObject);
            }
            
            if (HasKey("Mod Copy") && Button("Append Structure", "This will add any sources or targets that are " +
                                                                 "present in the copy but not in this. It will not affect data inserted, " +
                                                                 "and will not paste new data.", 200))
            {
                PasteStructure(modificationLevel, thisObject);
            }
            
            BackgroundColor(Color.white);
            if (HasKey("Mod Copy") && Button("Clear", 100))
            {
                DeleteKey("Mod Copy");
            }
            EndRow();
            Space();
        }
        
        private int SearchTypeIndex
        {
            get => SessionState.GetInt("Stat Modification Search Type Index", -1);
            set => SessionState.SetInt("Stat Modification Search Type Index", value);
        }
        
        private string SearchString
        {
            get => SessionState.GetString("Stat Modification Search String", "");
            set => SessionState.SetString("Stat Modification Search String", value);
        }
        
        private void DrawAddStatPopup(ModificationLevel modificationLevel, Stat thisObject)
        {
            if (modificationLevel == null) return;
            Space();

            BackgroundColor(Color.yellow);
            StartVerticalBox();

            if (modificationLevel.modifiableStats == null)
            {
                ModifiableStats(true);
                modificationLevel.CacheModifiableStats(thisObject);
                modificationLevel.CacheSourceStats(GameModuleObjects<Stat>(true), thisObject);
                return;
            }

            if (modificationLevel.modifiableStats.Length == 0)
            {
                Label("There are no modifiable Stats available.");
                EndVerticalBox();
                BackgroundColor(Color.white);
                return;
            }

            Label("Select a Stat to be modified by this object");

            var searchResults = SearchBoxWithType<Stat>(SearchTypeIndex, GameModuleObjectTypes<Stat>(), " Modification");
            SearchTypeIndex = searchResults.Item1;
            SearchString = searchResults.Item2;

            StartRow();
            _resultsStats = SearchResults().ToArray();
            
            var statNames = _resultsStats.Select(x => $"[{x.objectType}] {x.objectName}").ToArray();
            var stats = _resultsStats;
            if (modificationLevel.addStatIndex > statNames.Length)
                modificationLevel.addStatIndex = statNames.Length - 1;
            if (modificationLevel.addStatIndex < 0 && statNames.Length > 0)
                modificationLevel.addStatIndex = 0;

            modificationLevel.addStatIndex = Popup(modificationLevel.addStatIndex, statNames, 300);
            if (stats.Length == 0)
                Colors(Color.black, Color.grey);
            if (Button("Add", 50) && stats.Length > 0)
            {
                var target = stats[modificationLevel.addStatIndex];
                
                modificationLevel.AddModification(null, target, 0f, 0f, true);
                BuildAffectsLists();

                modificationLevel.CacheModifiableStats(thisObject);
                UpdateListStats(modificationLevel);

            }
            EndRow();
            EndVerticalBox();
            ResetColor();
            Space();
        }
        
        private List<Stat> SearchResults()
        {
            var results = new List<Stat>();
            var stats = GameModuleObjects<Stat>();
            if (SearchTypeIndex < GameModuleObjectTypes<Stat>().Length)
                stats = stats.Where(x => x.ObjectType == GameModuleObjectTypes<Stat>()[SearchTypeIndex]).ToArray();

            foreach (var stat in stats)
            {
                if (!stat.canBeModified) continue;
                
                if (stat.ObjectName.ToLower().Contains(SearchString.ToLower()) 
                    || stat.ObjectType.ToLower().Contains(SearchString.ToLower()))
                    results.Add(stat);
            }

            return results;
        }

        private Stat[] _resultsStats;
        private void UpdateListStats(ModificationLevel modificationLevel)
        {
            /*
            // If no search is provided, return
            if (string.IsNullOrWhiteSpace(SearchStringStat))
            {
                _resultsStats = modificationLevel.modifiableStats;
                return;
            }
            
            // Do search string
            _resultsStats = modificationLevel.modifiableStats
                .Where(x => !string.IsNullOrWhiteSpace(x.objectName))
                .Where(x => x.objectName
                    .IndexOf(SearchStringStat, StringComparison.OrdinalIgnoreCase) >= 0)
                .OrderBy(x => x.objectName)
                .ToArray();
                */
        }
        
        /// <summary>
        /// This is called to build all of the Stat lists and compute orders. Very important to call this.
        /// </summary>
        public void BuildAffectsLists()
        {
            DebugConsoleMessage("Build Connections & Populate Lists");

            var statsArray = GameModuleObjects<Stat>();
            var itemObjectArray = GameModuleObjects<ItemObject>();
            var itemAttributeArray = GameModuleObjects<ItemAttribute>();

            foreach (var stat in statsArray)
                stat.Cache(false);
            
            // Remove Missing (as when a user deletes an object, update the references to it)
            foreach (var stat in statsArray)
                stat.RemoveMissingStats();
            
            // Build all the connections between the Stats
            foreach (var stat in statsArray)
                stat.BuildTreeConnections();

            // Build the actual computation list & correct order
            foreach (var stat in statsArray)
            {
                stat.BuildComputeList();
                EditorUtility.SetDirty(stat); // Also make sure the objects get saved
            }
        }
    }
}