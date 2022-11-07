using UnityEngine;

namespace Audio.BeatManagement.Chopsticks.Aesthetics
{
    public class RunFlick : MonoBehaviour
    {
        private Animator _animator;

        private void Start()
        {
            _animator = GetComponent<Animator>();
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.F))
            {
                _animator.Play("flick", 0, 0);
            }
        }
    }
}