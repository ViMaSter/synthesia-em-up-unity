using System;
using System.Collections;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;

public class GoToGame : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(checkTime());
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.touchCount >= 6)
        {
            SceneManager.LoadScene("Scenes/AudioTest");
        }
    }

    private readonly DateTime startAt = new DateTime(2022, 11, 6, 0, 0, 0);
    
    public static DateTime UnixTimeStampToDateTime( double unixTimeStamp )
    {
        // Unix timestamp is seconds past epoch
        DateTime dateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
        dateTime = dateTime.AddSeconds( unixTimeStamp ).ToLocalTime();
        return dateTime;
    }

    IEnumerator checkTime()
    {
        UnityWebRequest uwr = UnityWebRequest.Get("https://vincent.mahn.ke/prj/2022_xx_ben-b-2022/time.php");
        var a = uwr.SendWebRequest();
        yield return a;
        if (uwr.isNetworkError)
        {
            Application.Quit(2);
        }

        var now = UnixTimeStampToDateTime(double.Parse(uwr.downloadHandler.text));
        Debug.Log("Now: " + now.ToString("o"));
        if (now > startAt)
        {
            Debug.Log("isTime");
            SceneManager.LoadScene("isTime");
        }
        else
        {
            Debug.Log("isNotTime");
            SceneManager.LoadScene("isNotTime");
        }
    }
}
