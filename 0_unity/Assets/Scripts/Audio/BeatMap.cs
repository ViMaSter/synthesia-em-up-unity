using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(fileName = "New Beat Map", menuName = "Beat Map")]
public class BeatMap : ScriptableObject
{
    public float BPM;
    public int Precision;
    public int BeatsUntilFirstBar;
    public AudioClip Track;
    public int[] Beats;
    public int[] FlickBeats;
}