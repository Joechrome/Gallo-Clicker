using UnityEngine;

public class Particle : MonoBehaviour
{
    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    public void Initialize(Sprite image, float size, float lifetime)
    {

        spriteRenderer.sprite = image;
        transform.localScale = new Vector3(size, size, 1f);
        float randomX = Random.Range(-40, 40) / 10f;
        float randomY = Random.Range(50, 90) / 10f;
        rb.linearVelocity = new Vector2(randomX, randomY);

        float randomSpin = Random.Range(-120, 120);
        rb.angularVelocity = randomSpin;

        // Handle self-destruction
        Destroy(gameObject, lifetime);
    }
}