using UnityEngine;

public class Movement : MonoBehaviour
{
    private Vector2 _lastFrameMovement = new Vector2();
    [Range(0.5f, 1.0f)]
    [SerializeField] private float FALLOFF = 0.9f;
    [SerializeField]private float speed = 10;

    void FixedUpdate()
    {
        _lastFrameMovement *= FALLOFF;
    }

    void Update()
    {
        Vector2 thisFrameMovement = Vector2.zero;
        if (Input.GetKey(KeyCode.W))
        {
            thisFrameMovement += new Vector2(0, 1);
        }
        if (Input.GetKey(KeyCode.S))
        {
            thisFrameMovement += new Vector2(0, -1);
        }
        if (Input.GetKey(KeyCode.A))
        {
            thisFrameMovement += new Vector2(-1, 0);
        }
        if (Input.GetKey(KeyCode.D))
        {
            thisFrameMovement += new Vector2(1, 0);
        }

        thisFrameMovement *= speed;
        _lastFrameMovement += thisFrameMovement;

        transform.position += new Vector3(_lastFrameMovement.x, _lastFrameMovement.y) * Time.deltaTime;
    }
}
