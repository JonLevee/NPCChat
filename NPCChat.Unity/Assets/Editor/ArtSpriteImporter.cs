using UnityEditor;

/// <summary>
/// Automatically sets Texture Type to Sprite for anything imported under Assets/Art/.
/// </summary>
public class ArtSpriteImporter : AssetPostprocessor
{
    void OnPreprocessTexture()
    {
        if (!assetPath.StartsWith("Assets/Art/"))
            return;

        var importer = (TextureImporter)assetImporter;
        if (importer.textureType == TextureImporterType.Sprite)
            return;

        importer.textureType = TextureImporterType.Sprite;
        importer.spriteImportMode = SpriteImportMode.Single;
        importer.mipmapEnabled = false;
        importer.filterMode = UnityEngine.FilterMode.Point;
    }
}
