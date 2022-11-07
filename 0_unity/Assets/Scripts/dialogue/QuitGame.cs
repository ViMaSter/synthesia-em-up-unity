using UnityEngine;

namespace Dialogue
{
    public class QuitGame : MonoBehaviour
    {
        public void Quit()
        {
            Application.Quit(0);
        }
    }
}
