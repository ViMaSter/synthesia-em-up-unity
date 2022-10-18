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

    public bool[] test  = Array.Empty<bool>();
    public bool[] test2 = Array.Empty<bool>();

    void OnEnable()
    {
        BPM = serializedObject.FindProperty(nameof(BeatMap.BPM));
        Precision = serializedObject.FindProperty(nameof(BeatMap.Precision));
        BeatsUntilFirstBar = serializedObject.FindProperty(nameof(BeatMap.BeatsUntilFirstBar));
        Track = serializedObject.FindProperty(nameof(BeatMap.Track));
        Beats = serializedObject.FindProperty(nameof(BeatMap.Beats));
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        EditorGUILayout.PropertyField(BPM);
        EditorGUILayout.PropertyField(Precision);
        EditorGUILayout.PropertyField(BeatsUntilFirstBar);
        EditorGUILayout.PropertyField(Track);
        EditorGUILayout.PropertyField(Beats);
        test = new bool[Beats.arraySize];
        test2 = new bool[Beats.arraySize]; 
                
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Tap");
        EditorGUILayout.LabelField("Hold");
        EditorGUILayout.EndHorizontal();

        List<int> a = new List<int>(0);
        for (var i = 0; i < Beats.arraySize; ++i)
        {
            a.Add(Beats.GetArrayElementAtIndex(i).intValue);
        }

        List<bool> b = new List<bool>();
        for (var i = 0; i <= a.Max(); ++i)
        {
            b.Add(a.Contains(i));
        }

        for (var i = 0; i < b.Count; ++i)
        {
            EditorGUILayout.BeginHorizontal();
            b[i] = EditorGUILayout.Toggle(b[i]);
            EditorGUILayout.EndHorizontal();
        }

        List<int> indices = new List<int>();
        for (var i = 0; i < b.Count; i++)
        {
            if (b[i])
            {
                indices.Add(i);
            }
        }
        
        Beats.ClearArray();
        for (var i = 0; i < indices.Count; i++)
        {
            Beats.InsertArrayElementAtIndex(i);
            Beats.GetArrayElementAtIndex(i).intValue = indices[i];
        }
        
        serializedObject.ApplyModifiedProperties();
    }
}