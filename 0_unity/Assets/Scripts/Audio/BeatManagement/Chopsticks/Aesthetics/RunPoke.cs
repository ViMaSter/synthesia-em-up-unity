using UnityEngine;

namespace Audio.BeatManagement.Chopsticks.Aesthetics
{
    public class RunPoke : MonoBehaviour
    {
        private Animator _animator;

        private void Start()
        {
            _animator = GetComponent<Animator>();
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.A))
            {
                _animator.Play("poke", 0, 0);
            }
            if (Input.GetKeyDown(KeyCode.E))
            {
                _animator.Play("eat", 0, 0);
            }
        }
    }
}
