using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using static InfinityPBR.Modules.EditorUtilities;
using static InfinityPBR.Modules.GameModuleUtilities;

namespace InfinityPBR.Modules
{
    public class EditorWindowPropertyCode : EditorWindow
    {
        private EditorWindow thisEditorWindow;
        
        [MenuItem("Window/Game Modules/Property Code")]
        static void Init()
        {
            // Get existing open window or if none, make a new one:
            EditorWindowPropertyCode window = (EditorWindowPropertyCode)GetWindow(typeof(EditorWindowPropertyCode));
            window.Show();
        }
        
        private void Awake()
        {
            thisEditorWindow = this;
            titleContent = new GUIContent("Property Code");
        }

        void OnGUI()
        {
            ShowPropertyCode();
            ShowAutoOptions();
        }
        
        void OnEnable()
        {
            AssemblyReloadEvents.beforeAssemblyReload += OnBeforeAssemblyReload;
            AssemblyReloadEvents.afterAssemblyReload += OnAfterAssemblyReload;
        }

        void OnDisable()
        {
            AssemblyReloadEvents.beforeAssemblyReload -= OnBeforeAssemblyReload;
            AssemblyReloadEvents.afterAssemblyReload -= OnAfterAssemblyReload;
        }

        public void OnBeforeAssemblyReload()
        {
            PropertyCodeUtility.CreatePropertyCode();
            ExportEnumCode();
            //Debug.Log("Before Assembly Reload");
        }

        public void OnAfterAssemblyReload()
        {
            //Debug.Log("After Assembly Reload");
        }

        private void ShowAutoOptions()
        {
            if (!EditorPrefs.HasKey("Auto Export Property Code"))
                EditorPrefs.SetBool("Auto Export Property Code", true);
            Label($"Auto Update Property Code {symbolInfo}", "If true, the property code will automatically update when play mode is entered, and whenever " +
                                                             "a Game Module object has its objectName or objectType changed. " +
                                                             "NOTE: This window must be open in the Editor for this feature " +
                                                             "to work.", true);
            LeftCheckSetBool("Auto Export Property Code", "Yes, auto export");
            Space();
        }
        
        private void ShowPropertyCode()
        {

            LabelSized("Property Code");

            Space();
            MessageBox("This will create a Properties.cs script in the InfinityPBR.Modules namespace. It makes it " +
                       "easier to access UID and Scriptable Objects for Item Objects, Item Attributes, Stats, and Conditions.\n\n" +
                       "For even easier use, add this to the top of any of your scripts:\n\n" +
                       "using static InfinityPBR.Modules.Properties;");
            
            Space();
            MessageBox("IMPORTANT: This code utilizes the object and type names that you set. If either of these changes, you will need " +
                       "to visit this panel to export the script again. This script is not included in the main Game Modules download " +
                       "from Asset Store / Package Manager, so updating Game Modules will not overwrite your exported script.", MessageType.Warning);

            Space();
            
            if (Button("Export Now", 200, 50))
            {
                if (Dialog("Exporting Property Code", "This will overwrite the existing script. We advise that you do not modify the exported script to avoid accidental " +
                                                      "over-writes. Would you like to continue?"))
                {
                    PropertyCodeUtility.CreatePropertyCode();
                    ExportEnumCode();
                }
            }

            Space();
        }

        public void ExportEnumCode()
        {
            
        }
        
        /*
        public void ExportPropertyCode()
        {
            PropertyCodeUtility.CreatePropertyCode();
            return;
            
        }
        */
    }
}