using System.Linq;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using static InfinityPBR.InfinityEditor;
using static InfinityPBR.Modules.GameModuleUtilities;

namespace InfinityPBR.Modules
{
    public class ConditionDrawerEditor : ConditionDrawer
    {
        private int fieldWidth;
        private Vector2 _scrollPosition;
        //private int addSourceIndex = -1;
        //private int sourceCalculation = 0;
        private LookupTable[] _availableLookupTables;
        private string[] _availableLookupTablesNames;

        public ConditionDrawerEditor(int fieldWidth)
        {
            this.fieldWidth = fieldWidth;
        }

        private bool _doCache;
        private bool _duplicateUid;

        private void Cache(Condition _modulesObject)
        {
            if (!_doCache) return;
            
            _modulesObject.Caching();
            _availableLookupTables = GameModuleObjects<LookupTable>();
            _availableLookupTablesNames = _availableLookupTables.Select(x => x.name).ToArray();
            _duplicateUid = ThisIsDuplicateUid(_modulesObject.Uid(), _modulesObject);
            _doCache = false;
            
            Debug.Log("Cached Condition Drawer");
        }
        
        protected bool ThisIsDuplicateUid(string uid, ModulesScriptableObject thisObject) => CountUids(uid) > 1;

        public void Draw(Condition _modulesObject)
        {
            Cache(_modulesObject);
            
            // If this is a duplicate uid and the button is clicked to reset it, redo Initial Setup
            if (CheckUid(_modulesObject.Uid(), _modulesObject, _duplicateUid))
                _doCache = true;
            
            ShowNameAndDescription(_modulesObject);
            
            ShowTime(_modulesObject);
            Space();
            ShowPoints(_modulesObject);
        }

        private bool CheckUid(string uid, Condition thisCondition, bool value)
        {
            if (!value)
                return false;
            
            StartRow();
            BackgroundColor(Color.red);
            if (Button("Create new uid"))
            {
                thisCondition.ResetUid();
                return true;
            }
            EndRow();
            StartRow();
            MessageBox($"There are more than one objects with this unique id! This is not good! " +
                       "If you recently copied an object, that is probably why. Click the " +
                       "\"Create new uid\" button to give THE NEW object a new uid. (Don't click this on " +
                       "the original object!!!", MessageType.Error);
            BackgroundColor(Color.white);
            
            EndRow();
            Space();
            return false;
        }

        private void ShowNameAndDescription(Condition thisCondition)
        {
            StartRow();
            StartVertical();
            if (string.IsNullOrWhiteSpace(thisCondition.DisplayName))
                thisCondition.SetDisplayName(thisCondition.objectName);
            StartRow();
            Label($"Display Name {symbolInfo}",
                "This is intended to be the name that is displayed. Multiple conditions " +
                "with different levels may all have the same Display Name, which will keep them " +
                "from stacking if \"Stacked\" is set false.", 140);
            thisCondition.SetDisplayName(TextField("", thisCondition.DisplayName, 100));
            EndRow();

            StartRow();
            Label($"Stack {symbolInfo}",
                $"If true, objects can obtain multiple copies of Conditions named {thisCondition.DisplayName}. " +
                "Otherwise, only one can be held at a time. If another instance is added, the end time will be " +
                "adjusted to match the new instance, and the Condition details will only be replaced if the new " +
                "instance is a greater level than the current.", 140);
            thisCondition.SetStack(Check(thisCondition.Stack, 100));
            EndRow();

            StartRow();
            Label($"Level {symbolInfo}",
                $"Higher level Conditions will replace active Conditions of the same Display Name ({thisCondition.DisplayName}) " +
                "if Stack = false.", 140);
            thisCondition.SetLevel(Int(thisCondition.Level, 100));
            EndRow();

            EndVerticalBox();

            StartVertical();
            Label("Description");
            thisCondition.description = TextArea(thisCondition.description);
            EndVerticalBox();
            EndRow();
            
            StartRow();
            Label($"Expiration Condition {symbolInfo}",
                "Select a condition to add automatically when this condition expires. If empty, nothing will happen.\n\nNote: Stat based " +
                "criteria will not be included, as there is no \"caster\" associated with the new GameCondition.", 140);
            thisCondition.SetExpirationCondition(Object(thisCondition.ExpirationCondition, typeof(Condition), 100) as Condition);
            if (thisCondition.ExpirationCondition != null)
            {
                Gap();
                Label($"Handler {symbolInfo}",
                    "If populated, the handler will be called when the expiration condition is added allowing for custom handling of the " +
                    "expiration condition. Example: Randomly decide whether the condition is added.",
                    70);
                thisCondition.SetExpirationConditionHandler(Object(thisCondition.ExpirationConditionHandler, typeof(ExpirationConditionHandler), 100) as ExpirationConditionHandler);
                LabelGrey("OPTIONAL");
            }
            EndRow();
        }

        private void ShowTime(Condition thisCondition)
        {
            Space();
            //StartVerticalBox();
            
            StartRow();




            thisCondition._showConditionTimes = OnOffButton(thisCondition._showConditionTimes);
            LabelBig($"Condition Time {symbolInfo}",
                "The total time this condition is active can be based on a variety of variable " +
                "factors which you can set up here using the Stats module. The system will add 0 time for " +
                "any setting that is unavailable, as in cases where the owner does not have the Stat required " +
                "to compute the time.\n\n" +
                "The total time will be the sum of values from all of the Condition Times added here, computed at " +
                "runtime.", 200, 14, true);

            EndRow();
            
            /*
            thisCondition._showConditionTimes = StartVerticalBoxSection(thisCondition._showConditionTimes, 
                $"Condition Time {symbolInfo}",
                14, true,
                "The total time this condition is active can be based on a variety of variable " +
                "factors which you can set up here using the Stats module. The system will add 0 time for " +
                "any setting that is unavailable, as in cases where the owner does not have the Stat required " +
                "to compute the time.\n\n" +
                "The total time will be the sum of values from all of the Condition Times added here, computed at " +
                "runtime.");
                */
            
            if (!thisCondition._showConditionTimes)
            {
                //EndVerticalBox();
                return;
            }

            // Ensure that only one is toggled on
            if (thisCondition.Infinite && thisCondition.Instant) thisCondition.SetInfinite(false);

            // Instant toggle
            if (!thisCondition.Infinite)
            {
                thisCondition.SetInstant(LeftCheck($"Instant {symbolInfo}",
                    "If true, the condition is intended to be instantly applied and " +
                    "removed. This is useful for conditions like \"Healing\" or \"Damage\", which do " +
                    "not have lasting effects; rather for conditions which have semi-permanent effects.",
                    thisCondition.Instant));
            }
            
            // Infinite Toggle
            if (!thisCondition.Instant)
            {
                thisCondition.SetInfinite(LeftCheck($"Infinite (Does Not Expire) {symbolInfo}",
                    "If true, the condition is will not be removed by the automatic processes. It can still be " +
                    "removed manually via your code. Periodic effects will trigger while this is active.",
                    thisCondition.Infinite));
            }
            
            if (thisCondition.Instant || thisCondition.Infinite) return; // Don't show the time options if one of these is true

            EnsureAtLeastOneConditionTime(thisCondition);

            for (int i = 0; i < thisCondition.conditionTimes.Count; i++)
            {
                StartVerticalBox();
                StartRow();
                StartVertical();
                DisplayConditionTime(thisCondition, i);
                EndVerticalBox();
                BackgroundColor(Color.red);
                if (Button($"{symbolX}", 25))
                {
                    thisCondition.conditionTimes.RemoveAt(i);
                    ExitGUI();
                }

                BackgroundColor(Color.white);
                EndRow();
                EndVerticalBox();
            }

            BackgroundColor(Color.yellow);
            if (Button("Add new Condition Time", 200))
                thisCondition.conditionTimes.Add(new ConditionTime());
            BackgroundColor(Color.white);
            
            //EndVerticalBox();
        }

        private void ShowPoints(Condition thisCondition)
        {
            StartRow();
            thisCondition._showPointEffects = OnOffButton(thisCondition._showPointEffects);
            LabelBig($"Effects on Points {symbolInfo}",
                "The \"Point\" value of Stat objects can not be modified by other Stats, but a condition can have " +
                "an effect. This is useful for effects on Stat values that utilize the Point value as a tracker, such as " +
                "\"Healing\", \"Damage\", and \"Get Experience\" etc.\n\nWhen this condition is added to a target, the method " +
                "\"AddPoints()\" will be called based on the settings here.", 200, 14, true);
            EndRow();
            if (!thisCondition._showPointEffects) return;

            EnsureAtLeastOnePointEffect(thisCondition);

            ShowPeriodic(thisCondition);

            for (int i = 0; i < thisCondition.pointEffects.Count; i++)
            {
                StartVerticalBox();
                StartRow();
                StartVertical();
                DisplayPointEffect(thisCondition, i);
                EndVerticalBox();
                BackgroundColor(Color.red);
                if (Button($"{symbolX}", 25))
                {
                    thisCondition.pointEffects.RemoveAt(i);
                    ExitGUI();
                }

                BackgroundColor(Color.white);
                EndRow();
                EndVerticalBox();
            }

            BackgroundColor(Color.yellow);
            if (Button("Add new Point Effect", 200))
                thisCondition.pointEffects.Add(new ConditionPointEffect());
            BackgroundColor(Color.white);
        }

        private void EnsureAtLeastOneConditionTime(Condition thisCondition)
        {
            if (thisCondition.conditionTimes.Count > 0) return;

            thisCondition.conditionTimes.Add(new ConditionTime());
        }

        private void EnsureAtLeastOnePointEffect(Condition thisCondition)
        {
            if (thisCondition.pointEffects.Count > 0) return;

            thisCondition.pointEffects.Add(new ConditionPointEffect());
        }

        private void ShowPeriodic(Condition thisCondition)
        {
            StartRow();
            if (thisCondition.Instant)
            {
                BackgroundColor(Color.grey);
                ContentColor(Color.grey);
            }

            thisCondition.SetPeriodic(LeftCheck($"Periodic {symbolInfo}",
                "The effects will occur periodically, starting instantly, and repeat " +
                "until the Condition Time has expired, and/or the condition is removed.", thisCondition.Periodic,
                130));
            ContentColor(Color.white);
            BackgroundColor(Color.white);
            if (!thisCondition.Periodic || thisCondition.Instant)
            {
                EndRow();
                return;
            }

            Label("", 20);
            if (thisCondition.Period == 0)
            {
                BackgroundColor(Color.red);
                ContentColor(Color.red);
            }

            var tempPeriod = thisCondition.Period;
            Label("Repeat every", 78);
            thisCondition.SetPeriod(Float(thisCondition.Period, 30));
            Label($"in-game minutes {symbolInfo}",
                $"The effects on points will occur every {thisCondition.Period} game minutes. Fractions of " +
                "minutes will be converted into subtime.");
            if (thisCondition.Period < 0)
                thisCondition.SetPeriod(tempPeriod < 0 ? 0 : tempPeriod);
            BackgroundColor(Color.white);
            ContentColor(Color.white);
            EndRow();
        }

        private void DisplayPointEffect(Condition thisCondition, int i)
        {
            var pointEffect = thisCondition.pointEffects[i];

            // TARGET OPTIONS
            StartRow();
            Label($"Target {symbolInfo}", "The target is the IHaveConditions object which receives this condition. The Stat Affected will have it's Point value " +
                                          "modified in this section. You can select additional stats and values which modify the final effect on the Stat Affected. Note: " +
                                          "If the target does not have the stats listed, this Condition will have no effect.", 110, true);
            Label("", 110);
            Label("", 20);
            Label($"{(pointEffect.statAffected == null ? "" : "Min")}", 35);
            Label($"{(pointEffect.statAffected == null ? "" : "Max")}", 35);
            Label("", 80);
            Label("", 35);
            Label("", 35);
            Label("", 100);
            EndRow();
            
            StartRow();
            Label($"Stat Affected {symbolInfo}",
                "This is the stat which will be affected. Only the Point value will be affected by the settings here.",
                110);
            pointEffect.statAffected = Object(pointEffect.statAffected, typeof(Stat), 110) as Stat;

            if (pointEffect.statAffected == null)
            {
                EndRow();
                return;
            }

            Label("", 20);
            pointEffect.minPoints = Float(pointEffect.minPoints, 35);
            pointEffect.maxPoints = Float(pointEffect.maxPoints, 35);
            Label($"Points {symbolInfo}",
                $"A random value between {pointEffect.minPoints} and {pointEffect.maxPoints} will be added to {pointEffect.statAffected.objectName}.", 110);
            EndRow();

            // SOURCE OPTIONS
            Space();
            StartRow();
            Label($"Source {symbolInfo}","This is the \"Caster\" of the condition, the IHaveStats object which gives the condition to the target. There may not be " +
                                         "a source object, in which case this is ignored. Note: The source stats are captured upon transfer of the Condition. Changes to the " +
                                         "source stat values will not change the Condition effects.", 110, true);
            Label("", 110);
            Label("", 20);
            Label($"{(pointEffect.stat == null ? "" : "Min")}", 35);
            Label($"{(pointEffect.stat == null ? "" : "Max")}", 35);
            Label("", 80);
            Label($"{(pointEffect.stat == null ? "" : "Min")}", 35);
            Label($"{(pointEffect.stat == null ? "" : "Max")}", 35);
            Label("", 100);
            EndRow();
            StartRow();
            Label($"Stat Required {symbolInfo}",
                "Optional. If populated, additional options will be available for point modifications based on the " +
                "Stat selected.", 110);
            pointEffect.stat = Object(pointEffect.stat, typeof(Stat), 110) as Stat;

            if (pointEffect.stat == null)
            {
                EndRow();
                return;
            }

            Label("", 20);
            pointEffect.minPerPoint = Float(pointEffect.minPerPoint, 35);
            pointEffect.maxPerPoint = Float(pointEffect.maxPerPoint, 35);
            Label($"Per Point {symbolInfo}",
                $"A random value between {pointEffect.minPerPoint} and {pointEffect.maxPerPoint} multiplied by the owners Points in {pointEffect.stat.objectName} will be included in the total effect on {pointEffect.statAffected.objectName}.",
                80);
            pointEffect.minPerFinalStat = Float(pointEffect.minPerFinalStat, 35);
            pointEffect.maxPerFinalStat = Float(pointEffect.maxPerFinalStat, 35);
            Label($"Per Final Stat {symbolInfo}",
                $"A random value between {pointEffect.minPerFinalStat} and {pointEffect.maxPerFinalStat} multiplied by the owners Final Stat in {pointEffect.stat.objectName} will be included in the total effect on {pointEffect.statAffected.objectName}",
                100);
            EndRow();

            StartRow();
            Label($"Lookup Table {symbolInfo}",
                "Optional. If populated, you can add a value multiplied by the Lookup Table output of any Stat you choose.",
                110);
            pointEffect.lookupTable = Object(pointEffect.lookupTable, typeof(LookupTable), 110) as LookupTable;

            if (pointEffect.lookupTable == null)
            {
                EndRow();
                return;
            }

            Label("", 20);
            pointEffect.minPerOutput = Float(pointEffect.minPerOutput, 35);
            pointEffect.maxPerOutput = Float(pointEffect.maxPerOutput, 35);
            Label("* value of:", 80);
            if (pointEffect.perOutputStat == null) pointEffect.perOutputStat = pointEffect.stat;
            pointEffect.perOutputStat = Object(pointEffect.perOutputStat, typeof(Stat), 110) as Stat;
            EndRow();
        }

        private void DisplayConditionTime(Condition thisCondition, int i)
        {
            var conditionTime = thisCondition.conditionTimes[i];

            //Header
            StartRow();
            Label("", 110);
            Label("", 110);
            Label("", 20);
            Label("Min", 35);
            Label("Max", 35);
            Label("", 80);
            Label("Min", 35);
            Label("Max", 35);
            Label("", 100);
            EndRow();
            
            StartRow();
            Label($"Minutes {symbolInfo}",
                $"The base in-game minutes the condition will be active for is a random value between {conditionTime.minMinutes} and {conditionTime.maxMinutes}. " +
                "If \"Stat Required\" is populated, " +
                "if the owner (i.e. source of the condition, not target) does not have the Stat then this will be the only contribution " +
                "to the total time from this Condition Time entry.", 110);
            Label("", 110);
            Label("", 20);
            if (conditionTime.minMinutes == 0 && conditionTime.maxMinutes == 0)
            {
                conditionTime.maxMinutes = 1;
            }
            conditionTime.minMinutes = Float(conditionTime.minMinutes, 35);
            conditionTime.maxMinutes = Float(conditionTime.maxMinutes, 35);
            EndRow();

            StartRow();
            Label($"Stat Required {symbolInfo}",
                "Optional. If populated, the owner (source, not target) of the Condition will need to have the Stat " +
                "or there will be no time added from this Condition Time beyond the base \"minutes\" value.", 110);
            conditionTime.stat = Object(conditionTime.stat, typeof(Stat), 110) as Stat;

            if (conditionTime.stat == null)
            {
                EndRow();
                return;
            }

            Label("", 20);
            conditionTime.minPerPoint = Float(conditionTime.minPerPoint, 35);
            conditionTime.maxPerPoint = Float(conditionTime.maxPerPoint, 35);
            Label($"Per Point {symbolInfo}",
                $"A value between {conditionTime.minPerPoint} and {conditionTime.maxPerPoint} multiplied by the owners Points in {conditionTime.stat.objectName} will be added to the total.",
                80);
            conditionTime.minPerFinalValue = Float(conditionTime.minPerFinalValue, 35);
            conditionTime.maxPerFinalValue = Float(conditionTime.maxPerFinalValue, 35);
            Label($"Per Final Stat {symbolInfo}",
                $"A value between {conditionTime.maxPerFinalValue} and {conditionTime.maxPerFinalValue}  multiplied by the owners Final Stat in {conditionTime.stat.objectName} will be added to the total.",
                100);
            EndRow();

            StartRow();
            Label($"Lookup Table {symbolInfo}",
                "Optional. If populated, you can add a value multiplied by the Lookup Table output of any Stat you choose.",
                110);
            conditionTime.lookupTable = Object(conditionTime.lookupTable, typeof(LookupTable), 110) as LookupTable;

            if (conditionTime.lookupTable == null)
            {
                EndRow();
                return;
            }

            Label("", 20);
            conditionTime.minPerOutput = Float(conditionTime.minPerOutput, 35);
            conditionTime.maxPerOutput = Float(conditionTime.maxPerOutput, 35);
            Label("* value of:", 80);
            if (conditionTime.perOutputStat == null) conditionTime.perOutputStat = conditionTime.stat;
            conditionTime.perOutputStat = Object(conditionTime.perOutputStat, typeof(Stat), 110) as Stat;
            EndRow();
        }

        public static void LabelBig(string label, int fontSize = 18, bool bold = false)
        {
            GUIStyle style = new GUIStyle(bold ? EditorStyles.boldLabel : EditorStyles.label);
            style.fontSize = fontSize;
            EditorGUILayout.LabelField(label, style, GUILayout.Height(fontSize));
        }

        public static void LabelBig(string label, int width, int fontSize = 18, bool bold = false)
        {
            GUIStyle style = new GUIStyle(bold ? EditorStyles.boldLabel : EditorStyles.label);
            style.fontSize = fontSize;
            EditorGUILayout.LabelField(label, style, GUILayout.Width(width), GUILayout.Height(fontSize));
        }

        public static void LabelBig(string label, string tooltip, int width, int fontSize = 18, bool bold = false)
        {
            GUIStyle style = new GUIStyle(bold ? EditorStyles.boldLabel : EditorStyles.label);
            style.fontSize = fontSize;
            EditorGUILayout.LabelField(new GUIContent(label, tooltip), style, GUILayout.Width(width),
                GUILayout.Height(fontSize));
        }
    }
}