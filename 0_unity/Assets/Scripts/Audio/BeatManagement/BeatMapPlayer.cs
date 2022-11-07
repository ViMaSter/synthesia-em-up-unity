using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Audio.BeatManagement.Chopsticks;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Audio.BeatManagement
{
    public class BeatMapPlayer : MonoBehaviour
    {
        public AudioMixer mixer;
        public AudioSource master;
        public UnityEvent<int> onBarStart;
        public UnityEvent<int> onBeatStart;

        private float _bpm;
        private float BPSWithPrecision => 60 / (_bpm * beatMap.precision);
        private float BeatOneOneOffset => beatMap.beatsUntilFirstBar * beatMap.precision;
        private float SecondsToNextBar => BPSWithPrecision * 4 - (master.time + BPSWithPrecision * BeatOneOneOffset) % (BPSWithPrecision * 4);
        private float BeatOffset => SecondsToNextBar - BPSWithPrecision * 2;
        public int NextBarIndex => (int)Math.Floor((master.time - BPSWithPrecision * BeatOneOneOffset) / (BPSWithPrecision * 4)) + 1;

        public float debugStartAtSeconds;

        private void Awake()
        {
            SetupQueueTimes(0);
        }

        public void Start()
        {
            mixer.SetFloat("GameVolume", 0.0f);
            mixer.SetFloat("MenuVolume", -80);
            _bpm = beatMap.bpm;
            master.clip = beatMap.track;
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

        private readonly List<int> _queuedBeats = new(1000);

        private void Update()
        {
            if (!_queuedBeats.Contains(NextBarIndex))
            {
                StartCoroutine(QueueBeatStartAt(NextBarIndex, AudioSettings.dspTime + SecondsToNextBar));
                if (NextBarIndex % 4 == 1)
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
            
            var firstFingerJustTapped = Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began;
            var keyboardWasPressed = Input.GetKeyDown(KeyCode.T); 

            if (!firstFingerJustTapped && !keyboardWasPressed)
            {
                return;
            }
            if (_lastPokeAt + PokePause >= Time.time)
            {
                return;
            }
            _lastPokeAt = Time.time;
            PlayPoke();
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
            PeaManager.Fire(SecondsToNextBar + BPSWithPrecision * beatMap.precision * 2);
        }

        public Animator chopAnimator;
        private void PlayPoke()
        {
            var hasHitAny = false;
            foreach (var manager in peaManagerPool)
            {
                if (!manager.AttemptHit())
                {
                    continue;
                }
                hasHitAny = true;
                IncreasePeaCounter();
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

        private AudioSource ShootSFX
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

        private PeaManager PeaManager
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

        private AudioSource HitSFX
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

        private AudioSource WhiffSFX
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

        private List<int> _pokeTimes = new();
        private List<int> _eatTimes;
        public BeatMap beatMap;

        private void SetupQueueTimes(int startingAt)
        {
            _pokeTimes = beatMap.beats.Where(i => i >= startingAt).Select(i => i - 1).ToList();
            _eatTimes = beatMap.flickBeats.Where(i => i >= startingAt).Select(i => i - 1).ToList();
        }
    
        private readonly List<int> _pressTimes = new();
        private void Record()
        {
            _pressTimes.Add(NextBarIndex);
        }

        private float _lastPokeAt = -1f;
        private const float PokePause = 0.110f;
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
            GUI.Label(new Rect(0, 60, 500, 20), "Next bar index: " + NextBarIndex);

            GUI.Label(new Rect(0, 100, 500, 20), "Recs: " + string.Join(", ", _pressTimes));
            GUI.Label(new Rect(0, 100, 500, 20),"IsRec: " + (_pokeTimes.Contains(NextBarIndex-1) ? "!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!": ""));
        }
    }
}
