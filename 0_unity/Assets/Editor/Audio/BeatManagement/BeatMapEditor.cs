using System;
using System.Collections.Generic;
using System.Linq;
using Audio.BeatManagement;
using UnityEditor;
using UnityEngine;

namespace Editor.Audio.BeatManagement
{
    [CustomEditor(typeof(BeatMap))]
    [CanEditMultipleObjects]
    public class BeatMapEditor : UnityEditor.Editor
    {
        private SerializedProperty _bpm;
        private SerializedProperty _precision;
        private SerializedProperty _beatsUntilFirstBar;
        private SerializedProperty _track;
        private SerializedProperty _beats;
        private SerializedProperty _flickBeats;
        
        private BeatMapPlayer _activeSlave;
        private bool _gameIsRunning;

        public BeatMapEditor()
        {
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
        }

        private void OnPlayModeStateChanged(PlayModeStateChange obj)
        {
            _gameIsRunning = obj switch
            {
                PlayModeStateChange.EnteredPlayMode => true,
                PlayModeStateChange.ExitingPlayMode => false,
                _ => _gameIsRunning
            };
            _activeSlave = obj switch
            {
                PlayModeStateChange.EnteredPlayMode => FindObjectOfType<BeatMapPlayer>(),
                PlayModeStateChange.ExitingPlayMode => null,
                _ => _activeSlave
            };
        }

        private void OnEnable()
        {
            _bpm = serializedObject.FindProperty(nameof(BeatMap.bpm));
            _precision = serializedObject.FindProperty(nameof(BeatMap.precision));
            _beatsUntilFirstBar = serializedObject.FindProperty(nameof(BeatMap.beatsUntilFirstBar));
            _track = serializedObject.FindProperty(nameof(BeatMap.track));
            _beats = serializedObject.FindProperty(nameof(BeatMap.beats));
            _flickBeats = serializedObject.FindProperty(nameof(BeatMap.flickBeats));

            _activeSlave = FindObjectOfType<BeatMapPlayer>();
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            EditorGUILayout.PropertyField(_bpm);
            EditorGUILayout.PropertyField(_precision);
            EditorGUILayout.PropertyField(_beatsUntilFirstBar);
            EditorGUILayout.PropertyField(_track);
                
            var buttonAndLabel = GUILayout.Width(40);
            var boxes = GUILayout.Width(80);
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Jump", buttonAndLabel);
            EditorGUILayout.LabelField("Beat", buttonAndLabel);
            EditorGUILayout.LabelField("Flick", boxes);
            EditorGUILayout.LabelField("Player", boxes);
            EditorGUILayout.LabelField("Eat", boxes);
            EditorGUILayout.EndHorizontal();

            var activeBeatsIndices = new List<int>(0);
            var activeFlickBeatsIndices = new List<int>(0);
            if (_flickBeats.arraySize != _beats.arraySize)
            {
                _flickBeats.arraySize = _beats.arraySize;
            }
            for (var i = 0; i < _beats.arraySize; ++i)
            {
                activeBeatsIndices.Add(_beats.GetArrayElementAtIndex(i).intValue);
                activeFlickBeatsIndices.Add(_flickBeats.GetArrayElementAtIndex(i).intValue);
            }

            var activeBeatsByIndex = new List<bool>();
            var activeFlickBeatsByIndex = new List<bool>();
            var mostEntries = Math.Max(activeBeatsIndices.Max(), activeFlickBeatsIndices.Max());
            var clip = (AudioClip)_track.objectReferenceValue;
            var beatsInSong = Math.Ceiling(clip.length * (_bpm.floatValue / 60.0f)) * 2;

            for (var i = 0; i <= Math.Max(mostEntries, beatsInSong); ++i)
            {
                activeBeatsByIndex.Add(activeBeatsIndices.Contains(i));
                activeFlickBeatsByIndex.Add(activeFlickBeatsIndices.Contains(i));
            }
        
            for (var i = 1; i < Math.Max(mostEntries, beatsInSong); ++i)
            {
                if (activeBeatsByIndex.Count < i)
                {
                    activeBeatsByIndex.Add(false);
                }
                if (activeFlickBeatsByIndex.Count < i)
                {
                    activeFlickBeatsByIndex.Add(false);
                }
                var highlightedRow = new GUIStyle {normal = new GUIStyleState {background = Texture2D.grayTexture}};
                var regularRow = new GUIStyle();

                // ReSharper disable once Unity.PerformanceCriticalCodeNullComparison needed to update when game is running
                EditorGUI.BeginDisabledGroup(!_gameIsRunning);
                EditorGUILayout.BeginHorizontal(_gameIsRunning && _activeSlave.NextBarIndex == i ? highlightedRow : regularRow);
                if (GUILayout.Button("▶️", buttonAndLabel))
                {
                    if (_gameIsRunning)
                    {
                        _activeSlave.JumpTo(i);
                    }
                }
                EditorGUI.EndDisabledGroup();
                EditorGUILayout.LabelField(i.ToString(), buttonAndLabel);
                activeBeatsByIndex[i] = EditorGUILayout.Toggle(activeBeatsByIndex[i], boxes);
                if (i < 4)
                {
                    EditorGUILayout.Toggle(false, boxes);
                }
                else
                {
                    activeBeatsByIndex[i-4] = EditorGUILayout.Toggle(activeBeatsByIndex[i-4], boxes);
                }
                activeFlickBeatsByIndex[i] = EditorGUILayout.Toggle(activeFlickBeatsByIndex[i], boxes);
                EditorGUILayout.EndHorizontal();
                if (i % 4 != 0)
                {
                    continue;
                }
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.Separator();
                EditorGUILayout.EndHorizontal();
            }

            var indices = new List<int>();
            var indices2 = new List<int>();
            for (var i = 0; i < activeBeatsByIndex.Count; i++)
            {
                if (activeBeatsByIndex[i])
                {
                    indices.Add(i);
                }
                if (activeFlickBeatsByIndex[i])
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
}