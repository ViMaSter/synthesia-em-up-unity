using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Events;
using UnityEngine.Serialization;

public class PlaybackSlave : MonoBehaviour
{
    public AudioMixer mixer;
    public AudioSource master;
    public UnityEvent onBarStart;
    public UnityEvent onBeatStart;

    private int PrecisionNths = 0;
    private float BPM = 0;
    private float BPSWithPrecision => 60 / (BPM * PrecisionNths);

    private float beatOneOneOffset = 0;
    private float SecondsToNextBar => (BPSWithPrecision * 4) - ((master.time + (BPSWithPrecision * beatOneOneOffset)) % (BPSWithPrecision * 4));
    public int NextBarIndex => (int)Math.Floor((master.time - (BPSWithPrecision * beatOneOneOffset)) / (BPSWithPrecision * 4)) + 1;

    private float _offset;
    public float DebugStartAtSeconds = 0;

    private void Awake()
    {
        var timeBehindInSeconds = BPSWithPrecision * beatOneOneOffset;
        _offset = timeBehindInSeconds;

        SetupQueueTimes();
    }

    public void Start()
    {
        mixer.SetFloat("GameVolume", 0.0f);
        mixer.SetFloat("MenuVolume", -80);
        BPM = beatmap.BPM;
        PrecisionNths = beatmap.Precision;
        beatOneOneOffset = beatmap.BeatsUntilFirstBar * PrecisionNths;
        master.clip = beatmap.Track;
        master.Play();
        master.time = DebugStartAtSeconds;
    }

    private bool isGameActive = true;

    private int RestoreAtBar = -1;
    private bool isQueued = false;

    private IEnumerator QueueBarStartAt(int beatIndex, double absoluteTime)
    {
        if (AudioSettings.dspTime > absoluteTime)
        {
            Debug.LogWarning($"Attempting to queue beat {beatIndex} was {AudioSettings.dspTime - absoluteTime}");
            yield break;
        }
        Debug.Log("Beat: " + beatIndex);
        yield return new WaitForSeconds((float)(AudioSettings.dspTime - absoluteTime));
        onBarStart.Invoke();
    }

    private IEnumerator QueueBeatStartAt(int bpmIndex, double absoluteTime)
    {
        if (AudioSettings.dspTime > absoluteTime)
        {
            Debug.LogWarning($"Attempting to queue beat {bpmIndex} was {AudioSettings.dspTime - absoluteTime} too late");
            yield break;
        }
        Debug.Log("BPM: " + bpmIndex);
        yield return new WaitForSeconds((float)(AudioSettings.dspTime - absoluteTime));
        onBeatStart.Invoke();
    }

    private List<int> queuedBeats = new List<int>(1000);

    private void Update()
    {
        if (!queuedBeats.Contains(NextBarIndex))
        {
            StartCoroutine(QueueBeatStartAt(NextBarIndex, AudioSettings.dspTime + SecondsToNextBar));
            if ((NextBarIndex % 4) == 0)
            {
                StartCoroutine(QueueBarStartAt(NextBarIndex, AudioSettings.dspTime + SecondsToNextBar));
            }
            queuedBeats.Add(NextBarIndex);
        }

        if (Input.GetKeyDown(KeyCode.R))
        {
            Record();
        }

        if (queueTimes.Contains(NextBarIndex))
        {
            shootSFX.PlayScheduled(SecondsToNextBar + AudioSettings.dspTime);
            //hitSFX.PlayScheduled(SecondsToNextBar + (BPSWithPrecision*2 * PrecisionNths) + AudioSettings.dspTime);
            queueTimes.Remove(NextBarIndex);
        }

        if (Input.GetKeyDown(KeyCode.T))
        {
            /*
             * @TODO VM:
             * 1. Determine perfect response timing
             * 2. Check if within threshold
             * 3. On success play hit
             * 4. On failure play whiff
             */
            
            hitSFX.Play();
        }
    }

    public AudioSource[] shootSFXPool = new AudioSource[0];
    public AudioSource[] hitSFXPool = new AudioSource[0];
    public int shootSFXIndex = -1;
    public int hitSFXIndex = -1;

    public AudioSource shootSFX
    {
        get
        {
            ++shootSFXIndex;
            if (shootSFXIndex >= shootSFXPool.Length)
            {
                shootSFXIndex = 0;
            }

            return shootSFXPool[shootSFXIndex];
        }
    }
    public AudioSource hitSFX
    {
        get
        {
            ++hitSFXIndex;
            if (hitSFXIndex >= hitSFXPool.Length)
            {
                hitSFXIndex = 0;
            }

            return hitSFXPool[hitSFXIndex];
        }
    }

    private List<int> queueTimes = new List<int>();
    public BeatMap beatmap;

    private void SetupQueueTimes()
    {
        queueTimes = beatmap.Beats.Select(i => i - 1).ToList();
    }
    
    private List<int> pressTimes = new List<int>();
    private void Record()
    {
        pressTimes.Add(NextBarIndex);
    }

    int previousCount = 0;
    private void OnGUI()
    {
        if (previousCount == 0 && Input.touchCount == 1)
        {
            hitSFX.Play();
        }
        previousCount = Input.touchCount;
        GUI.Label(new Rect(0, 0, 200, 20), "Time until next bar: ");
        GUI.HorizontalSlider(new Rect(200, 0, 500, 20), SecondsToNextBar, 0f, BPSWithPrecision*4);
        GUI.Label(new Rect(700, 0, 200, 20), SecondsToNextBar.ToString());
        GUI.Label(new Rect(0, 40, 500, 20), "Next bar index: " + (NextBarIndex));

        GUI.Label(new Rect(0, 60, 500, 20), "Recs: " + string.Join(", ", pressTimes));
        GUI.Label(new Rect(0, 60, 500, 20),"IsRec: " + (queueTimes.Contains(NextBarIndex-1) ? "AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA": ""));
    }
}
