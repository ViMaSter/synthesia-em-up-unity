using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

public class AdvanceText : MonoBehaviour
{
    public TextMeshProUGUI textElement;
    public GameObject advanceIndicator;
    public AudioSource advanceSoundSource;
    public AudioClip advanceHoldSoundClip;
    public AudioClip advanceReleaseSoundClip;
    public string[] statements = new string[] {};
    public UnityEvent OnDone;

    private int currentIndex = -1;
    private bool allowAdvance = false;
    private float textDisplayDelay = 15.0f / 30.0f;
    private float allowAdvanceAfter = 40.0f / 30.0f;
    private WaitForSeconds _textDisplayDelay;
    private WaitForSeconds _allowAdvanceAfter;


    void Awake()
    {
        _textDisplayDelay = new WaitForSeconds(textDisplayDelay);
        _allowAdvanceAfter = new WaitForSeconds(allowAdvanceAfter);
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
        allowAdvance = false;
        advanceIndicator.SetActive(false);
        advanceSoundSource.PlayOneShot(advanceReleaseSoundClip);
        textElement.text = "";
        StartCoroutine(QueueNextText());
    }

    IEnumerator QueueNextText()
    {
        yield return _textDisplayDelay;
        if (currentIndex >= statements.Length-1)
        {
            TextDone();
            yield break;
        }
        textElement.text = statements[++currentIndex];
        StartCoroutine(AllowAdvance());
    }

    IEnumerator AllowAdvance()
    {
        yield return _allowAdvanceAfter;
        advanceIndicator.SetActive(true);
        allowAdvance = true;
    }

    void Update()
    {
        if (!allowAdvance)
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
        OnDone.Invoke();
    }
}
