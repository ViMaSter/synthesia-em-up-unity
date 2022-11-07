using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Audio
{
    public class PlaybackSlave : MonoBehaviour
    {
        public AudioMixer mixer;
        public AudioSource master;
        public UnityEvent<int> onBarStart;
        public UnityEvent<int> onBeatStart;

        private float _bpm;
        private float BPSWithPrecision => 60 / (_bpm * beatmap.precision);
        private float BeatOneOneOffset => beatmap.beatsUntilFirstBar * beatmap.precision;
        private float SecondsToNextBar => (BPSWithPrecision * 4) - ((master.time + (BPSWithPrecision * BeatOneOneOffset)) % (BPSWithPrecision * 4));
        private float BeatOffset => SecondsToNextBar - (BPSWithPrecision * 2);
        public int NextBarIndex => (int)Math.Floor((master.time - (BPSWithPrecision * BeatOneOneOffset)) / (BPSWithPrecision * 4)) + 1;

        public float debugStartAtSeconds;

        private void Awake()
        {
            SetupQueueTimes(0);
        }

        public void Start()
        {
            mixer.SetFloat("GameVolume", 0.0f);
            mixer.SetFloat("MenuVolume", -80);
            _bpm = beatmap.bpm;
            master.clip = beatmap.track;
            master.Play();
            if (Debug.isDebugBuild)
            {
                master.time = debugStartAtSeconds;
            }
        }

        public void JumpTo(int beat)
        {
            SetupQueueTimes(beat);

            var time = BPSWithPrecision * (beat - 1) * 4 + BPSWithPrecision * BeatOneOneOffset;
            master.time = time;
        }

        private IEnumerator QueueBarStartAt(int beatIndex, double absoluteTime)
        {
            if (AudioSettings.dspTime > absoluteTime)
            {
                yield break;
            }
            yield return new WaitForSeconds((float)(AudioSettings.dspTime - absoluteTime));
            onBarStart.Invoke(beatIndex);
        }

        private IEnumerator QueueBeatStartAt(int bpmIndex, double absoluteTime)
        {
            if (AudioSettings.dspTime > absoluteTime)
            {
                yield break;
            }
            yield return new WaitForSeconds((float)(AudioSettings.dspTime - absoluteTime));
            onBeatStart.Invoke(bpmIndex);
        }

        private readonly List<int> _queuedBeats = new List<int>(1000);

        private void Update()
        {
            if (!_queuedBeats.Contains(NextBarIndex))
            {
                StartCoroutine(QueueBeatStartAt(NextBarIndex, AudioSettings.dspTime + SecondsToNextBar));
                if ((NextBarIndex % 4) == 1)
                {
                    StartCoroutine(QueueBarStartAt(NextBarIndex, AudioSettings.dspTime + SecondsToNextBar));
                }
                _queuedBeats.Add(NextBarIndex);
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
                if (_lastPokeAt + _pokePause >= Time.time)
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
            ShootSFX.PlayScheduled(SecondsToNextBar + AudioSettings.dspTime);
            _pokeTimes.Remove(NextBarIndex);
            flickAnimator.Play("flick", 0, 0);
            PeaManager.Fire(SecondsToNextBar + BPSWithPrecision * beatmap.precision * 2);
        }

        public Animator chopAnimator;
        private void PlayPoke()
        {
            var hasHitAny = false;
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
                HitSFX.Play();
                chopAnimator.Play("poke", 0, 0);
            }
            else
            {
                WhiffSFX.Play();
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

        private IEnumerator ClearPeas()
        {
            yield return new WaitForSeconds(0.33f);
            _peaCounter = 0;
            foreach (var peaSlot in peaSlots)
            {
                peaSlot.color = new Color(0, 0, 0, 0);
            }
        }

        public AudioSource[] shootSFXPool = Array.Empty<AudioSource>();
        public int shootSFXIndex = -1;

        public AudioSource ShootSFX
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

        public PeaManager[] peaManagerPool = Array.Empty<PeaManager>();
        public int peaManagerIndex = -1;

        public PeaManager PeaManager
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
    
        public AudioSource[] hitSFXPool = Array.Empty<AudioSource>();
        public int hitSFXIndex = -1;
        public AudioSource HitSFX
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
    
        public AudioSource[] whiffSFXPool = Array.Empty<AudioSource>();
        public int whiffSFXIndex = -1;
        public AudioSource WhiffSFX
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

        private void SetupQueueTimes(int startingAt)
        {
            _pokeTimes = beatmap.beats.Where(i => i >= startingAt).Select(i => i - 1).ToList();
            _eatTimes = beatmap.flickBeats.Where(i => i >= startingAt).Select(i => i - 1).ToList();
        }
    
        private readonly List<int> _pressTimes = new List<int>();
        private void Record()
        {
            _pressTimes.Add(NextBarIndex);
        }

        private float _lastPokeAt = -1f;
        private readonly float _pokePause = 0.110f;
        private int _peaCounter;

        private void OnGUI()
        {
            if (!Debug.isDebugBuild)
            {
                return;
            }
        
            GUI.Label(new Rect(0, 0, 200, 20), "Time until next bar: ");
            GUI.HorizontalSlider(new Rect(200, 0, 500, 20), SecondsToNextBar, 0f, BPSWithPrecision*4);
            GUI.Label(new Rect(700, 0, 200, 20), SecondsToNextBar.ToString(CultureInfo.InvariantCulture));
            GUI.HorizontalSlider(new Rect(200, 40, 1500, 20), BeatOffset, -BPSWithPrecision*2, BPSWithPrecision*2);
            GUI.Box(new Rect(200+750, 30, 1, 40), Texture2D.redTexture);
            GUI.Label(new Rect(700, 40, 800, 20), BeatOffset.ToString(CultureInfo.InvariantCulture));
            GUI.Label(new Rect(0, 60, 500, 20), "Next bar index: " + (NextBarIndex));

            GUI.Label(new Rect(0, 100, 500, 20), "Recs: " + string.Join(", ", _pressTimes));
            GUI.Label(new Rect(0, 100, 500, 20),"IsRec: " + (_pokeTimes.Contains(NextBarIndex-1) ? "AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA": ""));
        }
    }
}
