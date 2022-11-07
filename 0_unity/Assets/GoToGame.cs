using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;

public class GoToGame : MonoBehaviour
{
    // Start is called before the first frame update
    private void Start()
    {
        StartCoroutine(CheckTime());
    }

    // Update is called once per frame
    private void Update()
    {
        if (Input.touchCount >= 6)
        {
            SceneManager.LoadScene("Scenes/Game");
        }
    }

    private readonly DateTime _startAt = new(2022, 11, 6, 0, 0, 0);

    private static DateTime UnixTimeStampToDateTime( double unixTimeStamp )
    {
        // Unix timestamp is seconds past epoch
        var dateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
        dateTime = dateTime.AddSeconds( unixTimeStamp ).ToLocalTime();
        return dateTime;
    }

    private IEnumerator CheckTime()
    {
        var uwr = UnityWebRequest.Get("https://vincent.mahn.ke/prj/2022_xx_ben-b-2022/time.php");
        var a = uwr.SendWebRequest();
        yield return a;
        switch(uwr.result)
        {
            case UnityWebRequest.Result.InProgress:
                break;
            case UnityWebRequest.Result.Success:
                break;
            case UnityWebRequest.Result.ConnectionError:
                Application.Quit(2);
                break;
            case UnityWebRequest.Result.ProtocolError:
                Application.Quit(3);
                break;
            case UnityWebRequest.Result.DataProcessingError:
                Application.Quit(4);
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }

        var now = UnixTimeStampToDateTime(double.Parse(uwr.downloadHandler.text));
        if (now <= _startAt)
        {
            SceneManager.LoadScene("isNotTime");
            yield break;
        }
        
        SceneManager.LoadScene("isTime");
    }
}
