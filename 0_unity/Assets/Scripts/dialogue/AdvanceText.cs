using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AdvanceText : MonoBehaviour
{
    public TextMeshProUGUI textElement;
    public GameObject advanceIndicator;
    public AudioSource advanceSoundSource;
    public AudioClip advanceSoundClip;
    public string[] statements = new string[] {};

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
        Advance();
    }

    // Update is called once per frame
    void Advance()
    {
        allowAdvance = false;
        advanceIndicator.SetActive(false);
        advanceSoundSource.PlayOneShot(advanceSoundClip);
        textElement.text = "";
        StartCoroutine(QueueNextText());
    }

    IEnumerator QueueNextText()
    {
        yield return _textDisplayDelay;
        if (currentIndex >= statements.Length)
        {
            TextDone();
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
        if (Input.touchCount > 0)
        {
            Advance();
        }
    }

    void TextDone()
    {
        Application.Quit();
    }
}
