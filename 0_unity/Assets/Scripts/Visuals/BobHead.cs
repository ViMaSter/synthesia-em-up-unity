using UnityEngine;

namespace Visuals
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
