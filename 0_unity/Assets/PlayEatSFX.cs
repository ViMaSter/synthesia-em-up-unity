using UnityEngine;

public class PlayEatSFX : MonoBehaviour
{
    private AudioSource _audioSource;

    private void Start()
    {
        _audioSource = GetComponent<AudioSource>();
    }

    // ReSharper disable once UnusedMember.Global Used by UnityEvent
    public void PlayEat()
    {
        _audioSource.Play();
    }
}
