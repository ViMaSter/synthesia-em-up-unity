using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class FadeOutOnSongEnd : MonoBehaviour
{
    public AudioSource music;
    public Image fadeImage;

    // Update is called once per frame
    void Update()
    {
        if (music.time < (music.clip.length - 1f))
        {
            return;
        }

        StartCoroutine(FadeOut());
    }
    
    public IEnumerator Fade(float targetOpacity, float time)
    {
        var completion = 0f;
        var startVolume = fadeImage.color.a;
        var startTime = Time.time;
        while (completion < 1)
        {
            var tmpColor = fadeImage.color;
            tmpColor.a = Mathf.Lerp(startVolume, targetOpacity, completion);
            fadeImage.color = tmpColor;
            completion = (Time.time - startTime) / time;
            yield return null;
        }

        var newColor = fadeImage.color;
        newColor.a = targetOpacity;
        fadeImage.color = newColor;
    }

    private IEnumerator FadeOut()
    {
        yield return Fade(1, 2f);
        yield return new WaitForSeconds(1f);
        SceneManager.LoadScene("isOver");
    }
}
