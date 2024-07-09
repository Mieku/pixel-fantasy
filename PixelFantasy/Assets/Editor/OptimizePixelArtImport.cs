using UnityEditor;
using UnityEngine;

public class OptimizePixelArtImport : AssetPostprocessor
{
    void OnPreprocessTexture()
    {
        TextureImporter textureImporter = (TextureImporter)assetImporter;

        // Apply pixel art-friendly settings
        textureImporter.filterMode = FilterMode.Point;
        textureImporter.mipmapEnabled = false;
        textureImporter.textureCompression = TextureImporterCompression.Uncompressed;
    }

    void OnPreprocessModel()
    {
        ModelImporter modelImporter = (ModelImporter)assetImporter;
        modelImporter.importBlendShapes = false;
        modelImporter.importAnimation = false;
    }

    void OnPreprocessAudio()
    {
        AudioImporter audioImporter = (AudioImporter)assetImporter;
        audioImporter.loadInBackground = true;
    }
}