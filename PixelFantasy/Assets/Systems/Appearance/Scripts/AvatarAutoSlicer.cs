using UnityEditor;
using UnityEngine;
using System.IO;
using UnityEditor.U2D.Sprites;

public class AvatarAutoSlicer
{
    [MenuItem("Tools/Gobbo/Begin Avatar Auto Slice")]
    public static void BeginAutoSlice()
    {
        Debug.Log("Avatar Auto-Slicing Started");
        
        const string examplePath = "Assets/Systems/Appearance/Examples/ExampleAvatarLayout.png";
        const string artPath = "Assets/Systems/Appearance/Avatar Art/";
        
        string[] fileEntries = Directory.GetFiles(artPath, "*.png", SearchOption.AllDirectories);
        
        foreach (string fileName in fileEntries)
        {
            AutoSlice(fileName, examplePath);
        }

        Debug.Log("Done Avatar Auto-Slicing!");
    }
    
    private static void AutoSlice(string targetPath, string examplePath)
    {
        TextureImporter target = AssetImporter.GetAtPath(targetPath) as TextureImporter;
        TextureImporter example = AssetImporter.GetAtPath(examplePath) as TextureImporter;

        if (target != null && example != null)
        {
            target.GetSourceTextureWidthAndHeight(out int width, out int height);
            example.GetSourceTextureWidthAndHeight(out int exampleWidth, out int exampleHeight);
            bool isSameSize = exampleWidth == width && exampleHeight == height;
            
            if (isSameSize)
            {
                var settings = new TextureImporterSettings();
                example.ReadTextureSettings(settings);
                target.SetTextureSettings(settings);

                var factory = new SpriteDataProviderFactories();
                factory.Init();

                var exampleData = factory.GetSpriteEditorDataProviderFromObject(example);
                var targetData = factory.GetSpriteEditorDataProviderFromObject(target);

                exampleData.InitSpriteEditorDataProvider();
                targetData.InitSpriteEditorDataProvider();

                var spriteRects = exampleData.GetSpriteRects();

                targetData.SetSpriteRects(spriteRects);
                targetData.Apply();
                target.SaveAndReimport();
            }
            else
            {
                Debug.LogWarning("Skipped due to size mismatch: " + targetPath);
            }
        }
        else
        {
            Debug.LogError("Failed to load texture importers for: " + targetPath);
        }
    }
}
