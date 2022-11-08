using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

namespace Dialogue
{
    public class AdvanceText : MonoBehaviour
    {
        public TextMeshProUGUI textElement;
        public GameObject advanceIndicator;
        public AudioSource advanceSoundSource;
        public AudioClip advanceHoldSoundClip;
        public AudioClip advanceReleaseSoundClip;
        public string[] statements = {};
        public UnityEvent onDone;

        private int _currentIndex = -1;
        private bool _allowAdvance;
        private const float TextDisplayDelay = 15.0f / 30.0f;
        private const float AllowAdvanceAfter = 40.0f / 30.0f;
        private WaitForSeconds _textDisplayDelayEnumerator;
        private WaitForSeconds _allowAdvanceAfterEnumerator;


        private void Awake()
        {
            _textDisplayDelayEnumerator = new WaitForSeconds(TextDisplayDelay);
            _allowAdvanceAfterEnumerator = new WaitForSeconds(AllowAdvanceAfter);
            advanceIndicator.SetActive(false);
            textElement.text = "";
        }

        private void Start()
        {
            StartCoroutine(InitialAdvance());
        }

        private IEnumerator InitialAdvance()
        {
            yield return new WaitForSeconds(0.5f);
            advanceSoundSource.PlayOneShot(advanceHoldSoundClip);
            yield return new WaitForSeconds(0.05f);
            Advance();
        }

        private void Advance()
        {
            _allowAdvance = false;
            advanceIndicator.SetActive(false);
            advanceSoundSource.PlayOneShot(advanceReleaseSoundClip);
            textElement.text = "";
            StartCoroutine(QueueNextText());
        }

        private IEnumerator QueueNextText()
        {
            yield return _textDisplayDelayEnumerator;
            if (_currentIndex >= statements.Length-1)
            {
                TextDone();
                yield break;
            }
            textElement.text = statements[++_currentIndex];
            StartCoroutine(AllowAdvance());
        }

        private IEnumerator AllowAdvance()
        {
            yield return _allowAdvanceAfterEnumerator;
            advanceIndicator.SetActive(true);
            _allowAdvance = true;
        }

        private void Update()
        {
            if (!_allowAdvance)
            {
                return;
            }
            WaitForAdvance();
        }

        private void WaitForAdvance()
        {
            if (Input.touchCount <= 0 && !Input.GetMouseButton(1))
            {
                return;
            }

            if (Input.GetMouseButton(1))
            {
                Advance();
                return;
            }
        
            switch (Input.touches[0].phase)
            {
                case TouchPhase.Began:
                    advanceSoundSource.PlayOneShot(advanceHoldSoundClip);
                    return;
                case TouchPhase.Ended:
                case TouchPhase.Canceled:
                    Advance();
                    return;
                case TouchPhase.Moved:
                case TouchPhase.Stationary:
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void TextDone()
        {
            onDone.Invoke();
        }
    }
}
