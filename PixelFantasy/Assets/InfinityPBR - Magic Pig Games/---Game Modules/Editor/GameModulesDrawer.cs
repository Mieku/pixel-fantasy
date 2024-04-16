using UnityEditor;
using UnityEngine;
using static InfinityPBR.Modules.GameModuleUtilities;

namespace InfinityPBR.Modules
{
    public class GameModulesDrawer : InfinityEditor
    {
        
        public bool _copiedUid;
        public int countUids = -1;
        public bool cachedCount;
        
        public static void UpdateProperties()
        {
            if (!EditorPrefs.GetBool("Auto Export Property Code"))
                return;

            PropertyCodeUtility.CreatePropertyCode();
        }

        protected bool ThisIsDuplicateUid(string uid, ModulesScriptableObject thisObject) => CountUids(uid) > 1;

        protected bool CheckUid(string uid, ModulesScriptableObject thisObject, bool value)
        {
            if (!value)
            {
                StartRow();
                BackgroundColor(_copiedUid ? Color.green : Color.white);
                if (Button(_copiedUid ? $"{symbolCheck} Copied" : "Copy Uid", 80))
                {
                    EditorGUIUtility.systemCopyBuffer = uid;
                    _copiedUid = true;
                }
                
                ResetColor();
                Label(symbolInfo, "The uid should never change, regardless of the name or other values associated " +
                                  "with this object. It is the link used to serialized objects at runtime. Be careful to " +
                                  "never change this value.\n\nThis must be unique. If you copy an object, this section " +
                                  "will appear red and you will be prompted to create a new uid. Remember to do this on " +
                                  "the NEW object, rather than the original!", 25);
                LabelGrey("uid: " + uid);

                EndRow();
                Space();
                return false;
            }
            
            Label($"Uid is {uid}, count is {countUids}");
            StartRow();
            BackgroundColor(Color.red);
            if (Button("Create new uid"))
            {
                thisObject.ResetUid();
                CacheCount(thisObject.Uid());
                return true;
            }
            EndRow();
            StartRow();
            ShowMultipleUidsMessage(thisObject);
            BackgroundColor(Color.white);
            
            EndRow();
            Space();
            return false;
        }

        private void ShowMultipleUidsMessage(ModulesScriptableObject thisObject)
        {
            MessageBox($"There are {countUids} objects with this unique id: {thisObject.Uid()}! This is not good! " +
                       "If you recently copied an object, that is probably why. Click the " +
                       "\"Create new uid\" button to give THE NEW object a new uid. (Don't click this on " +
                       "the original object!!!)", MessageType.Error);
        }

        public virtual void BeginChangeCheck() => EditorGUI.BeginChangeCheck();

        protected virtual void EndChangeCheck(Object obj, bool setDirty = true)
        {
            if (!EditorGUI.EndChangeCheck()) return;
            if (!setDirty) return;
            EditorUtility.SetDirty(obj);
        }

        public bool ButtonCode(string label, bool current)
        {
            BackgroundColor(current ? Color.green : Color.black);
            if (Button(label))
                current = !current;
            return current;
        }
        
        private void CacheCount(string uid)
        {
            if (cachedCount) return;
            
            countUids = CountUids(uid);
            cachedCount = true;
        }
        
        public static void ShowFooter(ModulesScriptableObject Script)
        {
            Space();
        }
        
        public static bool ShowFullInspector(string scriptName)
        {
            Space();
            SetBool("Show full inspector " + scriptName,
                LeftCheck("Show full inspector", GetBool("Show full inspector " + scriptName)));
            return GetBool("Show full inspector " + scriptName);
        }
        
        protected virtual void CheckName(ModulesScriptableObject obj)
        {
            obj.objectName = obj.Name;
            obj.objectName = obj.objectName.Replace(",", "");
        }
    }
}