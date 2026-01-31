using UnityEditor;

public static class ForceRefreshScene
{
    [MenuItem("Tools/Force Refresh ChuckieHand")]
    public static void RefreshChuckieHand()
    {
        AssetDatabase.Refresh();
        AssetDatabase.ImportAsset("Assets/Scenes/ChuckieHand.unity", ImportAssetOptions.ForceUpdate);
        AssetDatabase.Refresh();
    }
}
