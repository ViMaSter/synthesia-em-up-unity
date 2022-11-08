using UnityEngine;
using UnityEngine.SceneManagement;

namespace Dialogue
{
    public class LoadScene : MonoBehaviour
    {
        [SerializeField] private string sceneToLoad;
        public void Execute()
        {
            SceneManager.LoadScene(sceneToLoad);
        }
    }
}
