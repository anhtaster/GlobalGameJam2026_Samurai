using UnityEngine;
using UnityEditor;

public class ForceRefreshAssets : MonoBehaviour
{
    [MenuItem("Tools/Refresh Assets")]
    static void RefreshAssets()
    {
        AssetDatabase.Refresh();
        Debug.Log("Assets refreshed!");
    }
}
