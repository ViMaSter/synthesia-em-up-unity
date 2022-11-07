using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

public class AdvanceText : MonoBehaviour
{
    public TextMeshProUGUI textElement;
    public GameObject advanceIndicator;
    public AudioSource advanceSoundSource;
    public AudioClip advanceHoldSoundClip;
    public AudioClip advanceReleaseSoundClip;
    public string[] statements = new string[] {};
    public UnityEvent onDone;

    private int _currentIndex = -1;
    private bool _allowAdvance;
    private readonly float _textDisplayDelay = 15.0f / 30.0f;
    private readonly float _allowAdvanceAfter = 40.0f / 30.0f;
    private WaitForSeconds _textDisplayDelayEnumerator;
    private WaitForSeconds _allowAdvanceAfterEnumerator;


    void Awake()
    {
        _textDisplayDelayEnumerator = new WaitForSeconds(_textDisplayDelay);
        _allowAdvanceAfterEnumerator = new WaitForSeconds(_allowAdvanceAfter);
        advanceIndicator.SetActive(false);
        textElement.text = "";
    }

    void Start()
    {
        StartCoroutine(InitialAdvance());
    }

    IEnumerator InitialAdvance()
    {
        yield return new WaitForSeconds(0.5f);
        advanceSoundSource.PlayOneShot(advanceHoldSoundClip);
        yield return new WaitForSeconds(0.05f);
        Advance();
    }

    // Update is called once per frame
    void Advance()
    {
        _allowAdvance = false;
        advanceIndicator.SetActive(false);
        advanceSoundSource.PlayOneShot(advanceReleaseSoundClip);
        textElement.text = "";
        StartCoroutine(QueueNextText());
    }

    IEnumerator QueueNextText()
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

    IEnumerator AllowAdvance()
    {
        yield return _allowAdvanceAfterEnumerator;
        advanceIndicator.SetActive(true);
        _allowAdvance = true;
    }

    void Update()
    {
        if (!_allowAdvance)
        {
            return;
        }
        WaitForAdvance();
    }

    void WaitForAdvance()
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
        }
    }

    void TextDone()
    {
        onDone.Invoke();
    }
}
