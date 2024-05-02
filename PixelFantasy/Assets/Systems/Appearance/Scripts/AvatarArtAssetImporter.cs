using UnityEngine;
using UnityEditor;
using System.IO;

public class AvatarArtAssetImporter : AssetPostprocessor 
{
    void OnPreprocessTexture() 
    {
        // Check if the asset path contains the "Avatar Art" folder and ends with ".png"
        if (assetPath.Contains("/Avatar Art/") && assetPath.EndsWith(".png"))
        {
            string fileName = Path.GetFileName(assetPath);  // Extracts the file name from the assetPath
            Debug.Log($"Preprocessing new file: {fileName}");
            TextureImporter importer = (TextureImporter)assetImporter;
            importer.spritePixelsPerUnit = 16; // Set Pixels Per Unit
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Multiple;
            importer.filterMode = FilterMode.Point; // Set filter mode to Point (no filter)
            importer.textureCompression = TextureImporterCompression.Uncompressed; // No compression
            importer.isReadable = true; // Set Read/Write Enabled to true
        }
    }
}

