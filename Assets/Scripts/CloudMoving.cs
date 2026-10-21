using UnityEngine;

public class CloudMoving : MonoBehaviour
{
    /// <summary>
    /// Raised each time the cloud reaches the right edge and is teleported back to
    /// the left one. GlitchCloud listens for this to roll for a glitched pass.
    /// </summary>
    public event System.Action Wrapped;

    [SerializeField] private float _speed = 5f; // Cloud Seed
    private const float OffscreenMargin = 1f;   // Travel pass the screen
    private const float MinRandomY = 2f;        // Min restriction
    private const float MaxRandomY = 4.5f;        // Max restriction

    private float _leftEdge;    // Starting X
    private float _rightEdge;   // Ending X

    private void Start()
    {

        Camera cam = Camera.main;
        float reach = cam.orthographicSize * cam.aspect
                    + GetComponent<SpriteRenderer>().bounds.extents.x
                    + OffscreenMargin;

        _rightEdge = cam.transform.position.x + reach;
        _leftEdge = cam.transform.position.x - reach;
    }

    private void Update()
    {

        transform.position += Vector3.right * (_speed * Time.deltaTime);

        if (transform.position.x > _rightEdge)
        {
            Vector3 position = transform.position;
            position.x = _leftEdge;
            position.y = Random.Range(MinRandomY, MaxRandomY);
            transform.position = position;

            Wrapped?.Invoke();
        }
    }
}
