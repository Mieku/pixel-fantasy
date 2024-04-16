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
            "DATABRAIN_1_2",
     };

        /// <summary>
        /// Add define symbols as soon as Unity gets done compiling.
        /// </summary>
        static DatabrainVersionScriptingDefineSymbol()
        {
            string definesString = PlayerSettings.GetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup);
            List<string> allDefines = definesString.Split(';').ToList();
            allDefines.AddRange(Symbols.Except(allDefines));
            PlayerSettings.SetScriptingDefineSymbolsForGroup(
                EditorUserBuildSettings.selectedBuildTargetGroup,
                string.Join(";", allDefines.ToArray()));
        }

    }
}
#endif