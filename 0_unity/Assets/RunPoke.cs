using System.Collections;
using System.Collections.Generic;
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
            _animator.Play("poke", 0, 0);
            _animator.playbackTime = 0;
        }
        if (Input.GetKeyDown(KeyCode.E))
        {
            _animator.Play("eat", 0, 0);
            _animator.playbackTime = 0;
        }
    }
}
