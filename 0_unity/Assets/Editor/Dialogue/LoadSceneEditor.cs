using System.Collections.Generic;
using UnityEditor;
using UnityEngine.SceneManagement;

namespace Dialogue
{
    [CustomEditor(typeof(LoadScene))]
    [CanEditMultipleObjects]
    public class LoadSceneEditor : UnityEditor.Editor
    {
        private SerializedProperty _sceneToLoad;
        private List<string> _scenes;

        private void OnEnable()
        {
            _sceneToLoad = serializedObject.FindProperty("sceneToLoad");

            _scenes = new List<string> { "NONE" };
            for (var i = 0; i < SceneManager.sceneCountInBuildSettings; ++i)
            {
                _scenes.Add(System.IO.Path.GetFileNameWithoutExtension(SceneUtility.GetScenePathByBuildIndex(i)));
            }
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Scene name:");

            var selectedIndex = _scenes.FindIndex(sceneName => sceneName == _sceneToLoad.stringValue);
            if (selectedIndex == -1)
            {
                selectedIndex = 0;
            }
            var sceneIndex = EditorGUILayout.Popup(selectedIndex, _scenes.ToArray());
            _sceneToLoad.stringValue = _scenes[sceneIndex];
 
            if (EditorGUI.EndChangeCheck())
            {
                serializedObject.ApplyModifiedProperties();
            }
            
            EditorGUILayout.EndHorizontal();
        }

    }
}