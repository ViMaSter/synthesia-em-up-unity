using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Profiling;
using UnityEngine.Serialization;

public class PlaybackSlave : MonoBehaviour
{
    public AudioMixer mixer;
    public AudioSource master;

    private int PrecisionNths = 0;
    private float BPM = 0;
    private float BPSWithPrecision => 60 / (BPM * PrecisionNths);

    private float beatOneOneOffset = 0;
    private float SecondsToNextBar => (BPSWithPrecision * 4) - ((master.time + (BPSWithPrecision * beatOneOneOffset)) % (BPSWithPrecision * 4));
    private int NextBarIndex => (int)Math.Floor((master.time - (BPSWithPrecision * beatOneOneOffset)) / (BPSWithPrecision * 4)) + 1;

    private float _offset;
    public float DebugStartAtSeconds = 0;

    private void Awake()
    {
        var timeBehindInSeconds = BPSWithPrecision * beatOneOneOffset;
        _offset = timeBehindInSeconds;
        fadeDuration = BPSWithPrecision/2;

        SetupQueueTimes();
    }

    public void Start()
    {
        mixer.SetFloat("GameVolume", 0.0f);
        mixer.SetFloat("MenuVolume", -80);
        BPM = beatmap.BPM;
        PrecisionNths = beatmap.Precision;
        beatOneOneOffset = beatmap.BeatsUntilFirstBar * PrecisionNths;
        master.clip = beatmap.track;
        master.Play();
        master.time = DebugStartAtSeconds;
    }

    private bool isGameActive = true;
    public float fadeDuration = 0f;

    public float[] crossFade(float t)
    {
        return new[]
        {
            Mathf.Lerp(1,0, Mathf.Sqrt(0.5f * (1f + t))),
            Mathf.Lerp(1,0, Mathf.Sqrt(0.5f * (1f - t)))
        };
    }

    private float LinearToDecibel(float linear)
    {
        float dB;
        if (linear != 0)
            dB = 20.0f * Mathf.Log10(linear);
        else
            dB = -144.0f;
        return dB;
    }

    private int RestoreAtBar = -1;
    private bool isQueued = false;
    private IEnumerator ToggleMenu()
    {
        if (isQueued)
        {
            yield break;
        }

        isQueued = true;
        if (isGameActive)
        {
            RestoreAtBar = NextBarIndex;
        }
        else
        {
            master.time = ((RestoreAtBar * (BPSWithPrecision * 4)) + (beatOneOneOffset * BPSWithPrecision)) - SecondsToNextBar;
        }
        var timeUntilFade = SecondsToNextBar - fadeDuration;
        if (timeUntilFade < 0)
        {
            timeUntilFade += BPSWithPrecision * 4;
        }
        yield return new WaitForSeconds(timeUntilFade);
        float progress = -1.0f;
        string from = isGameActive ? "GameVolume" : "MenuVolume";
        string to = isGameActive ? "MenuVolume" : "GameVolume";
        while (progress < 1.0f)
        {
            float[] values = crossFade(progress);
            mixer.SetFloat(from, LinearToDecibel(values[0]));
            mixer.SetFloat(to, LinearToDecibel(values[1]));
            progress += Time.deltaTime / (fadeDuration/2);
            yield return null;
        }

        mixer.SetFloat(from, -80);
        mixer.SetFloat(to, 0);
        isGameActive = !isGameActive;
        isQueued = false;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Return))
        {
            StartCoroutine(ToggleMenu());
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
        queueTimes = beatmap.beats.Select(i => i - 1).ToList();
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
        GUI.Label(new Rect(0, 20, 500, 20), "Enough time to transition this bar: " + !(SecondsToNextBar - fadeDuration <= 0));
        GUI.Label(new Rect(0, 40, 500, 20), "Next bar index: " + (NextBarIndex));

        GUI.Label(new Rect(0, 60, 500, 20), "Recs: " + string.Join(", ", pressTimes));
        GUI.Label(new Rect(0, 60, 500, 20), "IsRec: " + (queueTimes.Contains(NextBarIndex-1) ? "AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA": ""));

        if (!isQueued && GUI.Button(new Rect(0, 100, Screen.width, 300), "Toggle low-pass filter to end of next bar"))
        {
            StartCoroutine(ToggleMenu());
        }
    }
}
