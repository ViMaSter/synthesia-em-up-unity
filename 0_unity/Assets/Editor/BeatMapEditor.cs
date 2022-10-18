using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(BeatMap))]
[CanEditMultipleObjects]
public class BeatMapEditor : Editor
{
    private SerializedProperty BPM;
    private SerializedProperty Precision;
    private SerializedProperty BeatsUntilFirstBar;
    private SerializedProperty Track;
    private SerializedProperty Beats;
    private SerializedProperty FlickBeats;
    private PlaybackSlave activeSlave;

    void OnEnable()
    {
        BPM = serializedObject.FindProperty(nameof(BeatMap.BPM));
        Precision = serializedObject.FindProperty(nameof(BeatMap.Precision));
        BeatsUntilFirstBar = serializedObject.FindProperty(nameof(BeatMap.BeatsUntilFirstBar));
        Track = serializedObject.FindProperty(nameof(BeatMap.Track));
        Beats = serializedObject.FindProperty(nameof(BeatMap.Beats));
        FlickBeats = serializedObject.FindProperty(nameof(BeatMap.FlickBeats));

        activeSlave = GameObject.FindObjectOfType<PlaybackSlave>();
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        EditorGUILayout.PropertyField(BPM);
        EditorGUILayout.PropertyField(Precision);
        EditorGUILayout.PropertyField(BeatsUntilFirstBar);
        EditorGUILayout.PropertyField(Track);
        // EditorGUILayout.PropertyField(Beats);
        // EditorGUILayout.PropertyField(FlickBeats);
                
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Tap");
        EditorGUILayout.LabelField("Hold");
        EditorGUILayout.EndHorizontal();

        List<int> a = new List<int>(0);
        List<int> a2 = new List<int>(0);
        if (FlickBeats.arraySize != Beats.arraySize)
        {
            FlickBeats.arraySize = Beats.arraySize;
        }
        for (var i = 0; i < Beats.arraySize; ++i)
        {
            a.Add(Beats.GetArrayElementAtIndex(i).intValue);
            a2.Add(FlickBeats.GetArrayElementAtIndex(i).intValue);
        }

        List<bool> b = new List<bool>();
        List<bool> b2 = new List<bool>();
        for (var i = 0; i <= a.Max(); ++i)
        {
            b.Add(a.Contains(i));
            b2.Add(a2.Contains(i));
        }

        for (var i = 1; i < b.Count; ++i)
        {
            var borderstyle = new GUIStyle() {normal = new GUIStyleState(){background = Texture2D.grayTexture}};
            var style = new GUIStyle();
            
            EditorGUILayout.BeginHorizontal(activeSlave?.NextBarIndex == i ? borderstyle : style);
            b[i] = EditorGUILayout.Toggle(b[i]);
            b2[i] = EditorGUILayout.Toggle(b2[i]);
            EditorGUILayout.EndHorizontal();
            if (i % 4 == 0)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.Separator();
                EditorGUILayout.EndHorizontal();
            }
        }

        List<int> indices = new List<int>();
        List<int> indices2 = new List<int>();
        for (var i = 0; i < b.Count; i++)
        {
            if (b[i])
            {
                indices.Add(i);
            }
            if (b2[i])
            {
                indices2.Add(i);
            }
        }
        
        Beats.ClearArray();
        FlickBeats.ClearArray();
        for (var i = 0; i < indices.Count; i++)
        {
            Beats.InsertArrayElementAtIndex(i);
            Beats.GetArrayElementAtIndex(i).intValue = indices[i];
        }
        for (var i = 0; i < indices2.Count; i++)
        {
            FlickBeats.InsertArrayElementAtIndex(i);
            FlickBeats.GetArrayElementAtIndex(i).intValue = indices2[i];
        }
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Add beat"))
        {
            var lastCount = indices.Count;
            Beats.InsertArrayElementAtIndex(lastCount);
            Beats.GetArrayElementAtIndex(lastCount).intValue = Beats.GetArrayElementAtIndex(lastCount - 1).intValue+1;
        }
        EditorGUILayout.EndHorizontal();
        
        
        serializedObject.ApplyModifiedProperties();
    }
}