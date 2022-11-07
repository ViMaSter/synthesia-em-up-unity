using UnityEngine;

public class PlayEatSFX : MonoBehaviour
{
    private AudioSource _audioSource;

    private void Start()
    {
        _audioSource = GetComponent<AudioSource>();
    }

    public void PlayEat()
    {
        _audioSource.Play();
    }
}
