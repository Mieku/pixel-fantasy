using System;
using UnityEditor;
using UnityEngine;
using static InfinityPBR.Modules.GameModuleUtilities;

namespace InfinityPBR.Modules
{
    [Serializable]
    public abstract class ModulesScriptableObjectEditor : InfinityEditorModules<ModulesScriptableObject>
    {
        private static bool _copiedUid;

        private static int countUids = -1;
        private static bool cachedCount;

        protected virtual void CheckName(ModulesScriptableObject obj)
        {
            obj.objectName = obj.Name;
            obj.objectName = obj.objectName.Replace(",", "");
        }
        
        public static void ShowHeader(ModulesScriptableObject Script, string uid)
        {
            StartRow();
            BackgroundColor(_copiedUid ? Color.green : Color.white);
            if (Button(_copiedUid ? $"{symbolCheck} Copied" : "Copy Uid", 80))
            {
                EditorGUIUtility.systemCopyBuffer = uid;
                _copiedUid = true;
            }

            CacheCount(uid);
            
            BackgroundColor(Color.white);
            Label(symbolInfo, "The uid should never change, regardless of the name or other values associated " +
                              "with this object. It is the link used to serialized objects at runtime. Be careful to " +
                              "never change this value.\n\nThis must be unique. If you copy an object, this section " +
                              "will appear red and you will be prompted to create a new uid. Remember to do this on " +
                              "the NEW object, rather than the original!", 25);
            ContentColor(countUids > 1 ? Color.red : Color.grey);
            Label("uid: " + uid);
            ContentColor(Color.white);
            if (countUids > 1)
            {
                EndRow();
                StartRow();
                BackgroundColor(Color.red);
                if (Button("Create new uid"))
                {
                    Script.ResetUid();
                    cachedCount = false;
                }
                EndRow();
                StartRow();
                MessageBox($"There are {countUids} objects with this unique id! This is not good! " +
                           "If you recently copied an object, that is probably why. Click the " +
                           "\"Create new uid\" button to give THE NEW object a new uid. (Don't click this on " +
                           "the original object!!!", MessageType.Error);
                BackgroundColor(Color.white);
                
            }
            
            EndRow();
            StartRow();
            LeftCheckSetBool($"Show Default Inspector {Script.name}", "Show Default Inspector");
            EndRow();
            Space();
        }

        public static void UpdateProperties()
        {
            if (!EditorPrefs.GetBool("Auto Export Property Code"))
                return;
            
            PropertyCodeUtility.CreatePropertyCode();
            /*
            if (EditorWindow.HasOpenInstances<EditorWindowPropertyCode>())
            {
                EditorWindow.GetWindow<EditorWindowPropertyCode>(false, null, false).ExportPropertyCode();
                EditorWindow.GetWindow<EditorWindowPropertyCode>(false, null, false).ExportEnumCode();
            }
            else
            {
                Debug.LogWarning("Attempt to update Properties code failed. The Property Code window must be open " +
                                 "for this to work. Please open it with Window/Game Modules/Property Code. If you do not " +
                                 "want Properties to auto update, toggle off the option in the window.");
            }
            */
        }

        private static void CacheCount(string uid)
        {
            if (cachedCount) return;
            
            countUids = CountUids(uid);
            cachedCount = true;
        }

        public static void ShowFooter(ModulesScriptableObject Script)
        {
            Space();
            ShowFullInspector(Script.name);
        }
        
        protected override void OnEnable()
        {
            base.OnEnable();
            _copiedUid = false;
            cachedCount = false;
            //EditorApplication.projectWindowItemOnGUI += OnProjectWindowItemOnGUI;
        }

        protected virtual void OnDisable()
        {
            //EditorApplication.projectWindowItemOnGUI -= OnProjectWindowItemOnGUI;
        }
        
        public bool ButtonCode(string label, bool current)
        {
            BackgroundColor(current ? Color.green : Color.black);
            if (Button(label))
                current = !current;
            return current;
        }

        public void ShowObjectsWindowButton<T>(T thisObject) where T : ModulesScriptableObject
        {
            var typeName = CleanTypeName(typeof(T).ToString());
            if (ButtonBig($"Manage {typeName} Objects", 300))
                ClickButton(typeName, thisObject);
        }

        private void ClickButton<T>(string typeName, T thisObject) where T : ModulesScriptableObject
        {
            SetString($"{typeName} Type Selected", thisObject.objectType);
            //Debug.Log($"{typeName} Type Selected value is {thisObject.objectType} | {GetString($"{typeName} Type Selected")}");
            if (typeName == "Quest") EditorWindow.GetWindow(typeof(EditorWindowQuestObject)).Show();
            if (typeName == "Stat") EditorWindow.GetWindow(typeof(StatsManager)).Show();
            if (typeName == "ItemObject") EditorWindow.GetWindow(typeof(EditorWindowItemObject)).Show();
            if (typeName == "ItemAttribute") EditorWindow.GetWindow(typeof(EditorWindowItemAttribute)).Show();
        }

        protected virtual void OnProjectWindowItemOnGUI(string guid, Rect selectionRect) => Debug.Log("Did you forget to override this?");

        protected void DoOnProjectWindowItemOnGUI<T>(string guid, Rect selectionRect, T thisObject) where T : ModulesScriptableObject
        {
        }
    }
}
