using UnityEngine;
using UnityEngine.SceneManagement;

namespace Dialogue
{
    public class StartGame : MonoBehaviour
    {
        public void LoadGameScene()
        {
            SceneManager.LoadScene("Game");
        }
    }
}
