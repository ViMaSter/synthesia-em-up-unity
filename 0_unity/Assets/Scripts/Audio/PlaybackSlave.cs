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

    private const int PrecisionNths = 8;
    private int BPM = 130 * PrecisionNths;
    private float BPS => 60 / (float)BPM;

    private int beatOneOneOffset = 4 * PrecisionNths;
    private float SecondsToNextBar => (BPS * 4) - ((master.time + (BPS * beatOneOneOffset)) % (BPS * 4));
    private int NextBarIndex => (int)Math.Floor((master.time - (BPS * beatOneOneOffset)) / (BPS * 4)) + 1;

    private float _offset;

    private void Awake()
    {
        var timeBehindInSeconds = BPS * beatOneOneOffset;
        _offset = timeBehindInSeconds;
        fadeDuration = BPS/2;

        SetupQueueTimes();
    }

    public void Start()
    {
        mixer.SetFloat("GameVolume", 0.0f);
        mixer.SetFloat("MenuVolume", -80);

        master.Play();
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
            master.time = ((RestoreAtBar * (BPS * 4)) + (beatOneOneOffset * BPS)) - SecondsToNextBar;
        }
        var timeUntilFade = SecondsToNextBar - fadeDuration;
        if (timeUntilFade < 0)
        {
            timeUntilFade += BPS * 4;
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
            hitSFX.PlayScheduled(SecondsToNextBar + (BPS*2 * PrecisionNths) + AudioSettings.dspTime);
            queueTimes.Remove(NextBarIndex);
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

    private List<int> pressedTiems = new List<int> { 
        63, 64, 65, 
        71, 72, 73,
        79, 81, 
        95, 96, 97,
        103, 104, 105, 
        113, 115, 
        129, 131,
        137, 138, 139,
        205, 207,
        213, 214, 215 
    };
    private List<int> queueTimes = new List<int>();

    private void SetupQueueTimes()
    {
        queueTimes = pressedTiems.Select(i => i - 1).ToList();
       // master.time = 45;
    }
    
    private List<int> pressTimes = new List<int>();
    private void Record()
    {
        pressTimes.Add(NextBarIndex);
    }

    private void OnGUI()
    {
        GUI.Label(new Rect(0, 0, 200, 20), "Time until next bar: ");
        GUI.HorizontalSlider(new Rect(200, 0, 500, 20), SecondsToNextBar, 0f, BPS*4);
        GUI.Label(new Rect(700, 0, 200, 20), SecondsToNextBar.ToString());
        GUI.Label(new Rect(0, 20, 500, 20), "Enough time to transition this bar: " + !(SecondsToNextBar - fadeDuration <= 0));
        GUI.Label(new Rect(0, 40, 500, 20), "Next bar index: " + (NextBarIndex));

        GUI.Label(new Rect(0, 60, 500, 20), "Recs: " + string.Join(", ", pressTimes));
        GUI.Label(new Rect(0, 60, 500, 20), "IsRec: " + (pressedTiems.Contains(NextBarIndex) ? "AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA": ""));

        if (!isQueued && GUI.Button(new Rect(0, 100, Screen.width, 300), "Toggle low-pass filter to end of next bar"))
        {
            StartCoroutine(ToggleMenu());
        }
    }
}
