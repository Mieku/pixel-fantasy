using UnityEditor;
using UnityEngine;
using static InfinityPBR.Modules.GameModuleUtilities;

namespace InfinityPBR.Modules
{
    public class GameModulesAssetModificationProcessor : AssetModificationProcessor
    {
        public static AssetDeleteResult OnWillDeleteAsset(string assetPath, RemoveAssetOptions options)
        {
            var obj = AssetDatabase.LoadAssetAtPath<Object>(assetPath);
            if (obj == null)
                return AssetDeleteResult.DidNotDelete;
            
            if (obj is ItemAttribute) DeleteItemAttribute(AssetDatabase.LoadAssetAtPath<ItemAttribute>(assetPath));
            
            return AssetDeleteResult.DidNotDelete;
        }

        private static void DeleteItemAttribute(ItemAttribute obj)
        {
            Debug.Log($"Running Delete actions for {obj.objectName}");
            
            // ItemObjects
            foreach (var itemObject in GameModuleObjects<ItemObject>())
            {
                // Remove this from all startingItemAttributes for itemObjects
                itemObject.startingItemAttributes.Remove(obj);
            }
            
            // ItemAttributes
            foreach (var itemAttribute in GameModuleObjects<ItemAttribute>())
            {
                // Remove this from all Requisite attributes for other ItemAttributes
                itemAttribute.incompatibleAttributes.Remove(obj);
                itemAttribute.requiredAttributes.RemoveAll(x => x.itemAttribute == obj);
            }
        }
    }
}
