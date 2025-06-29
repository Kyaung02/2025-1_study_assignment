using UnityEngine;
using UnityEditor;
using UnityEngine.Tilemaps;

public static class CreateBasicTile
{
    [MenuItem("Assets/Create/2D/Tiles/Basic Tile")]
    public static void CreateTileAsset()
    {
        var tile = ScriptableObject.CreateInstance<Tile>();
        string path = AssetDatabase.GenerateUniqueAssetPath("Assets/New Tile.asset");
        AssetDatabase.CreateAsset(tile, path);
        AssetDatabase.SaveAssets();
        EditorUtility.FocusProjectWindow();
        Selection.activeObject = tile;
    }
}