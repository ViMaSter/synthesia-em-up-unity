using UnityEditor;
using UnityEngine;

namespace Editor
{
    public class AssetDatabaseExamples : MonoBehaviour
    {
        [MenuItem("AssetDatabase/Force Reserialize Assets Example")]
        private static void UpdateGroundMaterials()
        {
            AssetDatabase.ForceReserializeAssets();
        }
    }
}