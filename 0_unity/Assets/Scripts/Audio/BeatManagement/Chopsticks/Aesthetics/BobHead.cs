using UnityEngine;

namespace Audio.BeatManagement.Chopsticks.Aesthetics
{
    public class BobHead : MonoBehaviour
    {
        public Animator animator;
    
        public void Bob()
        {
            animator.Play("head", -1, 0.0f);
        }
    }
}