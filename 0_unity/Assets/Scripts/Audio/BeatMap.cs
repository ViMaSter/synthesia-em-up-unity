using UnityEngine;

[CreateAssetMenu(fileName = "New Beat Map", menuName = "Beat Map")]
public class BeatMap : ScriptableObject
{
    public float BPM;
    public int Precision;
    public int BeatsUntilFirstBar;
    public AudioClip track;
    public int[] beats;
}