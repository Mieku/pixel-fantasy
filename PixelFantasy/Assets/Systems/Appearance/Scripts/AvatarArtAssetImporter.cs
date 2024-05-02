using UnityEngine;
using UnityEditor;
using System.IO;
using UnityEditor.U2D.Sprites;

public class AvatarArtAssetImporter : AssetPostprocessor
{
    void OnPreprocessTexture()
    {
        // Check if the asset path contains the "Avatar Art" folder and ends with ".png"
        if (assetPath.Contains("/Avatar Art/") && assetPath.EndsWith(".png"))
        {
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



