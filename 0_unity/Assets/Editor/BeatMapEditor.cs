using System;
using System.Collections.Generic;
using System.Linq;
using Audio;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(BeatMap))]
[CanEditMultipleObjects]
public class BeatMapEditor : Editor
{
    private SerializedProperty _bpm;
    private SerializedProperty _precision;
    private SerializedProperty _beatsUntilFirstBar;
    private SerializedProperty _track;
    private SerializedProperty _beats;
    private SerializedProperty _flickBeats;
    private PlaybackSlave _activeSlave;

    void OnEnable()
    {
        _bpm = serializedObject.FindProperty(nameof(BeatMap.bpm));
        _precision = serializedObject.FindProperty(nameof(BeatMap.precision));
        _beatsUntilFirstBar = serializedObject.FindProperty(nameof(BeatMap.beatsUntilFirstBar));
        _track = serializedObject.FindProperty(nameof(BeatMap.track));
        _beats = serializedObject.FindProperty(nameof(BeatMap.beats));
        _flickBeats = serializedObject.FindProperty(nameof(BeatMap.flickBeats));

        _activeSlave = GameObject.FindObjectOfType<PlaybackSlave>();
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        EditorGUILayout.PropertyField(_bpm);
        EditorGUILayout.PropertyField(_precision);
        EditorGUILayout.PropertyField(_beatsUntilFirstBar);
        EditorGUILayout.PropertyField(_track);
        // EditorGUILayout.PropertyField(Beats);
        // EditorGUILayout.PropertyField(FlickBeats);
                
        var buttonAndLabel = GUILayout.Width(40);
        var boxes = GUILayout.Width(80);
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Jump", buttonAndLabel);
        EditorGUILayout.LabelField("Beat", buttonAndLabel);
        EditorGUILayout.LabelField("Flick", boxes);
        EditorGUILayout.LabelField("Player", boxes);
        EditorGUILayout.LabelField("Eat", boxes);
        EditorGUILayout.EndHorizontal();

        var a = new List<int>(0);
        var a2 = new List<int>(0);
        if (_flickBeats.arraySize != _beats.arraySize)
        {
            _flickBeats.arraySize = _beats.arraySize;
        }
        for (var i = 0; i < _beats.arraySize; ++i)
        {
            a.Add(_beats.GetArrayElementAtIndex(i).intValue);
            a2.Add(_flickBeats.GetArrayElementAtIndex(i).intValue);
        }

        var b = new List<bool>();
        var b2 = new List<bool>();
        var mostEntries = Math.Max(a.Max(), a2.Max());
        var clip = (AudioClip)_track.objectReferenceValue;
        var beatsInSong = Math.Ceiling(clip.length * (_bpm.floatValue / 60.0f)) * 2;

        for (var i = 0; i <= Math.Max(mostEntries, beatsInSong); ++i)
        {
            b.Add(a.Contains(i));
            b2.Add(a2.Contains(i));
        }
        
        for (var i = 1; i < Math.Max(mostEntries, beatsInSong); ++i)
        {
            if (b.Count < i)
            {
                b.Add(false);
            }
            if (b2.Count < i)
            {
                b2.Add(false);
            }
            var borderstyle = new GUIStyle() {normal = new GUIStyleState(){background = Texture2D.grayTexture}};
            var style = new GUIStyle();
            
            EditorGUILayout.BeginHorizontal(_activeSlave?.NextBarIndex == i ? borderstyle : style);
            if (GUILayout.Button("▶️", buttonAndLabel))
            {
                if (_activeSlave)
                {
                    _activeSlave.JumpTo(i);
                }
            }
            EditorGUILayout.LabelField(i.ToString(), buttonAndLabel);
            b[i] = EditorGUILayout.Toggle(b[i], boxes);
            if (i < 4)
            {
                EditorGUILayout.Toggle(false, boxes);
            }
            else
            {
                b[i-4] = EditorGUILayout.Toggle(b[i-4], boxes);
            }
            b2[i] = EditorGUILayout.Toggle(b2[i], boxes);
            EditorGUILayout.EndHorizontal();
            if (i % 4 == 0)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.Separator();
                EditorGUILayout.EndHorizontal();
            }
        }

        var indices = new List<int>();
        var indices2 = new List<int>();
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
        
        _beats.ClearArray();
        _flickBeats.ClearArray();
        for (var i = 0; i < indices.Count; i++)
        {
            _beats.InsertArrayElementAtIndex(i);
            _beats.GetArrayElementAtIndex(i).intValue = indices[i];
        }
        for (var i = 0; i < indices2.Count; i++)
        {
            _flickBeats.InsertArrayElementAtIndex(i);
            _flickBeats.GetArrayElementAtIndex(i).intValue = indices2[i];
        }
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Add beat"))
        {
            var lastCount = indices.Count;
            _beats.InsertArrayElementAtIndex(lastCount);
            _beats.GetArrayElementAtIndex(lastCount).intValue = _beats.GetArrayElementAtIndex(lastCount - 1).intValue+1;
        }
        EditorGUILayout.EndHorizontal();
        
        
        serializedObject.ApplyModifiedProperties();
    }
}