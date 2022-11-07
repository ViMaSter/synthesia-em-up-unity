using UnityEngine;

public class PlayEatSfx : MonoBehaviour
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
