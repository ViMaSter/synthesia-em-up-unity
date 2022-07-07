using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Audio;

public class PlaybackSlave : MonoBehaviour
{
    public AudioMixer mixer;
    public AudioSource master;
    public AudioSource slave;

    private int BPM = 96;
    private float BPS => 60 / (float)BPM;

    private int beatOneOneOffset = 2;
    private float SecondsToNextBar => (BPS * 4) - ((master.time + (BPS * beatOneOneOffset)) % (BPS * 4));
    private int NextBarIndex => (int)Math.Floor((master.time - (BPS * beatOneOneOffset)) / (BPS * 4)) + 1;

    private float _offset;

    private void Awake()
    {
        var timeBehindInSeconds = BPS * beatOneOneOffset;
        _offset = timeBehindInSeconds;
        fadeDuration = BPS/2;
    }

    public void Start()
    {
        mixer.SetFloat("GameVolume", 0.0f);
        mixer.SetFloat("MenuVolume", -80);

        master.Play();
        slave.PlayDelayed(_offset);
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
    private IEnumerator ToggleMenu()
    {
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

        var a = master.time;
        mixer.SetFloat(from, -80);
        mixer.SetFloat(to, 0);
        isGameActive = !isGameActive;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Return))
        {
            StartCoroutine(ToggleMenu());
        }
        if (Input.GetKeyDown(KeyCode.LeftControl))
        {
            if (master.isPlaying)
            {
                master.Pause();
                slave.Pause();
            }
            else
            {
                master.UnPause();
                slave.UnPause();
            }
        }
    }

    private void OnGUI()
    {
        GUI.Label(new Rect(0, 0, 200, 20), "Time until next bar: ");
        GUI.HorizontalSlider(new Rect(200, 0, 500, 20), SecondsToNextBar, 0f, BPS*4);
        GUI.Label(new Rect(700, 0, 200, 20), SecondsToNextBar.ToString());
        GUI.Label(new Rect(0, 20, 500, 20), "Enough time to transition this bar: " + !(SecondsToNextBar - fadeDuration <= 0));
        GUI.Label(new Rect(0, 40, 500, 20), "Next bar index: " + (NextBarIndex));
    }
}
