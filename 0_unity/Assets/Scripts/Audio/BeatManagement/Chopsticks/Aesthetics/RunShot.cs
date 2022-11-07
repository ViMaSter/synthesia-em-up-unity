using UnityEngine;

namespace Audio.BeatManagement.Chopsticks.Aesthetics
{
    public class RunShot : MonoBehaviour
    {
        private Animator _animator;

        private void Start()
        {
            _animator = GetComponent<Animator>();
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.R))
            {
                _animator.Play("rolling", 0, 0);
            }
        }
    }
}