using UnityEngine;
using UnityEngine.UI;

public class PeaManager : MonoBehaviour
{
    private Animator _peaAnimator;
    private Image _peaImage;
    private const float HitTolerance = 0.1f;
    public float correctHitAt = -1f;
    private float _startAt;

    private void Start()
    {
        _peaAnimator = GetComponent<Animator>();
        _peaImage = GetComponent<Image>();
    }

    public bool AttemptHit()
    {
        if (correctHitAt == -1)
        {
            Debug.Log(gameObject.name+": IGNORE!");
            return false;
        }
        float now = Time.time;
        if (correctHitAt - HitTolerance < now && now < correctHitAt + HitTolerance)
        {
            IsHit();
            Debug.Log(gameObject.name+": HIT!");
            return true;
        }

        if (correctHitAt + HitTolerance < now)
        {
            Reset();
        }
        Debug.Log(gameObject.name+": MISS!");
        return false;
    }

    private void IsHit()
    {
        // @TODO VM: Add pea to chopsticks
        Reset();
    }

    private void Reset()
    {
        _peaImage.color = new Color(0, 0, 0, 0);
        correctHitAt = -1;
        _startAt = -1;
    }

    public void Fire(float correctTimingAfterSeconds)
    {
        _startAt = Time.time;
        correctHitAt = Time.time + correctTimingAfterSeconds;

        _peaImage.color = new Color(1, 1, 1, 1);
        Debug.Log(correctTimingAfterSeconds);
        _peaAnimator.speed = 1 / correctTimingAfterSeconds;
        _peaAnimator.Play("rolling", 0, 0);
    }
}
