using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Events;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class PlaybackSlave : MonoBehaviour
{
    public AudioMixer mixer;
    public AudioSource master;
    public UnityEvent onBarStart;
    public UnityEvent onBeatStart;

    private float BPM = 0;
    private float BPSWithPrecision => 60 / (BPM * beatmap.Precision);
    private float BeatOneOneOffset => beatmap.BeatsUntilFirstBar * beatmap.Precision;
    private float SecondsToNextBar => (BPSWithPrecision * 4) - ((master.time + (BPSWithPrecision * BeatOneOneOffset)) % (BPSWithPrecision * 4));
    private float BeatOffset => SecondsToNextBar - (BPSWithPrecision * 2);
    public int NextBarIndex => (int)Math.Floor((master.time - (BPSWithPrecision * BeatOneOneOffset)) / (BPSWithPrecision * 4)) + 1;

    private float _offset;
    public float DebugStartAtSeconds = 0;

    private void Awake()
    {
        var timeBehindInSeconds = BPSWithPrecision * BeatOneOneOffset;
        _offset = timeBehindInSeconds;
        
        SetupQueueTimes();
    }

    public void Start()
    {
        Debug.Log(BeatOneOneOffset);
        mixer.SetFloat("GameVolume", 0.0f);
        mixer.SetFloat("MenuVolume", -80);
        BPM = beatmap.BPM;
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
        yield return new WaitForSeconds((float)(AudioSettings.dspTime - absoluteTime));
        onBeatStart.Invoke();
    }

    private List<int> queuedBeats = new List<int>(1000);

    private void Update()
    {
        if (!queuedBeats.Contains(NextBarIndex))
        {
            StartCoroutine(QueueBeatStartAt(NextBarIndex, AudioSettings.dspTime + SecondsToNextBar));
            if ((NextBarIndex % 4) == 1)
            {
                StartCoroutine(QueueBarStartAt(NextBarIndex, AudioSettings.dspTime + SecondsToNextBar));
            }
            queuedBeats.Add(NextBarIndex);
        }

        if (Input.GetKeyDown(KeyCode.R))
        {
            Record();
        }

        if (_pokeTimes.Contains(NextBarIndex))
        {
            PlayShot();
        }

        if (_eatTimes.Contains(NextBarIndex))
        {
            PlayEat();
        }
        
        if ((Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began) || Input.GetKeyDown(KeyCode.T))
        {
            if (_lastPokeAt + pokePause >= Time.time)
            {
                return;
            }
            _lastPokeAt = Time.time;
            PlayPoke();
        }
    }

    public Animator flickAnimator;
    private void PlayEat()
    {
        if (_peaCounter <= 0)
        {
            return;
        }
        _eatTimes.Remove(NextBarIndex);
        chopAnimator.Play("eat", 0, 0);
        StartCoroutine(ClearPeas());
    }
    private void PlayShot()
    {
        shootSFX.PlayScheduled(SecondsToNextBar + AudioSettings.dspTime);
        _pokeTimes.Remove(NextBarIndex);
        flickAnimator.Play("flick", 0, 0);
        peaManager.Fire(SecondsToNextBar + BPSWithPrecision * beatmap.Precision * 2);
    }

    public Animator chopAnimator;
    private void PlayPoke()
    {
        bool hasHitAny = false;
        foreach (var manager in peaManagerPool)
        {
            if (manager.AttemptHit())
            {
                hasHitAny = true;
                IncreasePeaCounter();
            }
        }

        if (hasHitAny)
        {
            hitSFX.Play();
            chopAnimator.Play("poke", 0, 0);
        }
        else
        {
            whiffSFX.Play();
            chopAnimator.Play("whiff", 0, 0);
        }
    }

    public Image[] peaSlots;
    private void IncreasePeaCounter()
    {
        ++_peaCounter;
        for (var i = 0; i < _peaCounter; ++i)
        {
            peaSlots[i].color = Color.white;
        }
    }

    public IEnumerator ClearPeas()
    {
        yield return new WaitForSeconds(0.33f);
        _peaCounter = 0;
        foreach (var peaSlot in peaSlots)
        {
            peaSlot.color = new Color(0, 0, 0, 0);
        }
    }

    public AudioSource[] shootSFXPool = new AudioSource[0];
    public int shootSFXIndex = -1;

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

    public PeaManager[] peaManagerPool = new PeaManager[0];
    public int peaManagerIndex = -1;

    public PeaManager peaManager
    {
        get
        {
            ++peaManagerIndex;
            if (peaManagerIndex >= peaManagerPool.Length)
            {
                peaManagerIndex = 0;
            }

            return peaManagerPool[peaManagerIndex];
        }
    }
    
    public AudioSource[] hitSFXPool = new AudioSource[0];
    public int hitSFXIndex = -1;
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
    
    public AudioSource[] whiffSFXPool = new AudioSource[0];
    public int whiffSFXIndex = -1;
    public AudioSource whiffSFX
    {
        get
        {
            ++whiffSFXIndex;
            if (whiffSFXIndex >= whiffSFXPool.Length)
            {
                whiffSFXIndex = 0;
            }

            return whiffSFXPool[whiffSFXIndex];
        }
    }

    private List<int> _pokeTimes = new List<int>();
    private List<int> _eatTimes;
    public BeatMap beatmap;

    private void SetupQueueTimes()
    {
        _pokeTimes = beatmap.Beats.Select(i => i - 1).ToList();
        _eatTimes = beatmap.FlickBeats.Select(i => i - 1).ToList();
    }
    
    private List<int> pressTimes = new List<int>();
    private void Record()
    {
        pressTimes.Add(NextBarIndex);
    }

    private float _lastPokeAt = -1f;
    private float pokePause = 0.110f;
    private int _peaCounter;

    private void OnGUI()
    {
        GUI.Label(new Rect(0, 0, 200, 20), "Time until next bar: ");
        GUI.HorizontalSlider(new Rect(200, 0, 500, 20), SecondsToNextBar, 0f, BPSWithPrecision*4);
        GUI.Label(new Rect(700, 0, 200, 20), SecondsToNextBar.ToString());
        GUI.HorizontalSlider(new Rect(200, 40, 1500, 20), BeatOffset, -BPSWithPrecision*2, BPSWithPrecision*2);
        GUI.Box(new Rect(200+750, 30, 1, 40), Texture2D.redTexture);
        GUI.Label(new Rect(700, 40, 800, 20), BeatOffset.ToString());
        GUI.Label(new Rect(0, 60, 500, 20), "Next bar index: " + (NextBarIndex));

        GUI.Label(new Rect(0, 100, 500, 20), "Recs: " + string.Join(", ", pressTimes));
        GUI.Label(new Rect(0, 100, 500, 20),"IsRec: " + (_pokeTimes.Contains(NextBarIndex-1) ? "AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA": ""));
    }
}
