using UnityEditor;
using UnityEngine;

public class AssetDatabaseExamples : MonoBehaviour
{
    [MenuItem("AssetDatabase/Force Reserialize Assets Example")]
    static void UpdateGroundMaterials()
    {
        AssetDatabase.ForceReserializeAssets();
    }
}