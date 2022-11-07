using UnityEngine;

public class RunPoke : MonoBehaviour
{
    private Animator _animator;

    // Start is called before the first frame update
    void Start()
    {
        _animator = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.A))
        {
            Debug.Log(_animator);

            _animator.Play("poke", 0, 0);
        }
        if (Input.GetKeyDown(KeyCode.E))
        {
            _animator.Play("eat", 0, 0);
        }
    }
}
