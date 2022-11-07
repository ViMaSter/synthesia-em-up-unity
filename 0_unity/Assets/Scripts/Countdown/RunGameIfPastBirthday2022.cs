using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;

namespace Countdown
{
    public class RunGameIfPastBirthday2022 : MonoBehaviour
    {
        private void Start()
        {
            StartCoroutine(CheckTime());
        }

        private void Update()
        {
            SkipToGameWorkaround();
        }

        private static void SkipToGameWorkaround()
        {
            if (Input.touchCount >= 6)
            {
                SceneManager.LoadScene("Scenes/Game");
            }

            if (Input.GetKeyDown(KeyCode.R))
            {
                SceneManager.LoadScene("Scenes/Game");
            }
        }

        private readonly DateTime _startAt = new(2022, 11, 6, 0, 0, 0);

        private static DateTime UnixTimeStampToDateTime( double unixTimeStamp )
        {
            var dateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            dateTime = dateTime.AddSeconds( unixTimeStamp ).ToLocalTime();
            return dateTime;
        }

        private IEnumerator CheckTime()
        {
            var currentTimeWebRequest = UnityWebRequest.Get("https://vincent.mahn.ke/prj/2022_xx_ben-b-2022/time.php");
            var currentTimeWebRequestOperation = currentTimeWebRequest.SendWebRequest();
            yield return currentTimeWebRequestOperation;
            switch(currentTimeWebRequest.result)
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

            var now = UnixTimeStampToDateTime(double.Parse(currentTimeWebRequest.downloadHandler.text));
            if (now < _startAt)
            {
                SceneManager.LoadScene("isNotTime");
                yield break;
            }
        
            SceneManager.LoadScene("isTime");
        }
    }
}
