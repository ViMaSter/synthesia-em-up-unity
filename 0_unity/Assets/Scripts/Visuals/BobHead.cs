using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BobHead : MonoBehaviour
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
        if(Input.GetKeyDown(KeyCode.Space))
        {
            _animator.Play("cathead", -1, 0.0f);
        }
    }
}