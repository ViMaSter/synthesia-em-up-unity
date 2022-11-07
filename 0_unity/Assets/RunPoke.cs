using UnityEngine;

public class RunPoke : MonoBehaviour
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
