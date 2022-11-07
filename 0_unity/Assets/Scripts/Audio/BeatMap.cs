using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(fileName = "New Beat Map", menuName = "Beat Map")]
public class BeatMap : ScriptableObject
{
    public float bpm;
    public int precision;
    public int beatsUntilFirstBar;
    public AudioClip track;
    public int[] beats;
    public int[] flickBeats;
}