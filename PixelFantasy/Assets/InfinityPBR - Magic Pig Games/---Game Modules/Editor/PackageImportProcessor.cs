using UnityEditor;
using UnityEngine;

namespace InfinityPBR.Modules
{
    public class PackageImportProcessor : AssetPostprocessor
    {
        // This method is called after all assets are imported, deleted, or moved.
        private static void OnPostprocessAllAssets(
            string[] importedAssets,
            string[] deletedAssets,
            string[] movedAssets,
            string[] movedFromAssetPaths)
        {
            foreach (var asset in importedAssets)
                ProcessAsset(asset, "Imported");

            foreach (var asset in deletedAssets)
                ProcessAsset(asset, "Deleted");
        }

        private static void ProcessAsset(string asset, string imported)
        {
            //Debug.Log($"processing: {asset} {imported}");
            var obj = AssetDatabase.LoadAssetAtPath<Object>(asset);
            
            if (obj == null) return;
            var sessionStateString = "";
            
            if (obj.GetType() == typeof(Stat))
                sessionStateString = GameModuleUtilities.SessionStateString<Stat>();
            if (obj.GetType() == typeof(ItemObject))
                sessionStateString = GameModuleUtilities.SessionStateString<ItemObject>();
            if (obj.GetType() == typeof(ItemAttribute))
                sessionStateString = GameModuleUtilities.SessionStateString<ItemAttribute>();
            if (obj.GetType() == typeof(Quest))
                sessionStateString = GameModuleUtilities.SessionStateString<Quest>();
            if (obj.GetType() == typeof(QuestCondition))
                sessionStateString = GameModuleUtilities.SessionStateString<QuestCondition>();
            if (obj.GetType() == typeof(QuestReward))
                sessionStateString = GameModuleUtilities.SessionStateString<QuestReward>();
            if (obj.GetType() == typeof(LootBox))
                sessionStateString = GameModuleUtilities.SessionStateString<LootBox>();
            if (obj.GetType() == typeof(LootItems))
                sessionStateString = GameModuleUtilities.SessionStateString<LootItems>();
            if (obj.GetType() == typeof(Condition))
                sessionStateString = GameModuleUtilities.SessionStateString<Condition>();
            if (obj.GetType() == typeof(MasteryLevels))
                sessionStateString = GameModuleUtilities.SessionStateString<MasteryLevels>();
            
            SessionState.SetBool(sessionStateString, false);
        }
    }

}
