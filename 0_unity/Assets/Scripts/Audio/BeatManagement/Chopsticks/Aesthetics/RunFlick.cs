using UnityEngine;

namespace Audio.BeatManagement.Chopsticks.Aesthetics
{
    public class RunFlick : MonoBehaviour
    {
        private Animator _animator;

        // Start is called before the first frame update
        private void Start()
        {
            _animator = GetComponent<Animator>();
        }

        // Update is called once per frame
        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.F))
            {
                _animator.Play("flick", 0, 0);
            }
        }
    }
}