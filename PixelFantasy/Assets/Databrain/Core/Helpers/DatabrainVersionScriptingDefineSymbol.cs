#if UNITY_EDITOR
using System.Collections.Generic;
using System.Linq;
using UnityEditor;

namespace Databrain.Inventory
{
    /// <summary>
    /// Adds the given define symbols to PlayerSettings define symbols.
    /// </summary>
    [InitializeOnLoad]
    public class DatabrainVersionScriptingDefineSymbol
    {

        /// <summary>
        /// Symbols that will be added to the editor
        /// </summary>
        public static readonly string[] Symbols = new string[] {
            "DATABRAIN_1_2","DATABRAIN_1_3",
        };

        public static readonly string[] SymbolsToRemove = new string[] {
            // "DATABRAIN_1_2",
        };

        /// <summary>
        /// Add define symbols as soon as Unity gets done compiling.
        /// </summary>
        static DatabrainVersionScriptingDefineSymbol()
        {
            #if UNITY_6000_0_OR_NEWER
            string definesString = PlayerSettings.GetScriptingDefineSymbols(UnityEditor.Build.NamedBuildTarget.FromBuildTargetGroup(EditorUserBuildSettings.selectedBuildTargetGroup));
            #else
            string definesString = PlayerSettings.GetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup);
            #endif

            List<string> allDefines = definesString.Split(';').ToList();
            allDefines.AddRange(Symbols.Except(allDefines));

            for (int i = 0; i < allDefines.Count; i ++)
            {
                if (SymbolsToRemove.Contains(allDefines[i]))
                {
                    allDefines.RemoveAt(i);
                }
            }

            #if UNITY_6000_0_OR_NEWER
            PlayerSettings.SetScriptingDefineSymbols(UnityEditor.Build.NamedBuildTarget.FromBuildTargetGroup(EditorUserBuildSettings.selectedBuildTargetGroup),  definesString);
            #else
            PlayerSettings.SetScriptingDefineSymbolsForGroup(
                EditorUserBuildSettings.selectedBuildTargetGroup,
                string.Join(";", allDefines.ToArray()));
            #endif

        }

    }
}
#endif