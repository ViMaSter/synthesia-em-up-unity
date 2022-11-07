using UnityEngine;
using UnityEngine.UI;

namespace Audio.BeatManagement.Chopsticks
{
    public class PeaManager : MonoBehaviour
    {
        private Animator _peaAnimator;
        private Image _peaImage;
        private const float HitTolerance = 0.1f;
        public float correctHitAt = float.NaN;

        private void Start()
        {
            _peaAnimator = GetComponent<Animator>();
            _peaImage = GetComponent<Image>();
        }

        public bool AttemptHit()
        {
            if (float.IsNaN(correctHitAt))
            {
                return false;
            }
            var now = Time.time;
            if (correctHitAt - HitTolerance < now && now < correctHitAt + HitTolerance)
            {
                IsHit();
                return true;
            }

            if (correctHitAt + HitTolerance < now)
            {
                Reset();
            }
            return false;
        }

        private void IsHit()
        {
            Reset();
        }

        private void Reset()
        {
            _peaImage.color = new Color(0, 0, 0, 0);
            correctHitAt = -1;
        }

        public void Fire(float correctTimingAfterSeconds)
        {
            correctHitAt = Time.time + correctTimingAfterSeconds;

            _peaImage.color = new Color(1, 1, 1, 1);
            _peaAnimator.speed = 1 / correctTimingAfterSeconds;
            _peaAnimator.Play("rolling", 0, 0);
        }
    }
}
