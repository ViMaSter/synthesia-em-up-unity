using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.LowLevel;
using UnityEngine.SceneManagement;

public class Go : MonoBehaviour
{
    private void OnGUI()
    {
        GUI.Label(new Rect(0, 0, Screen.width, 200), "Did you know? Browsers prevent sound from playing without interaction!");
        if (GUI.Button(new Rect(0, 100, Screen.width, 200),
                "So click here! And ideally make sure your volume isn't on 12/10."))
        {
            SceneManager.LoadScene(1, LoadSceneMode.Single);
        }
    }
}
