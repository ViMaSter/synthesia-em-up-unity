using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class BobHead : MonoBehaviour
{
    public Animator animator;
    
    public void Bob()
    {
        animator.Play("cathead", -1, 0.0f);
    }
}
