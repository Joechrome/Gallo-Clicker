using UnityEngine;

/// <summary>
/// One ransomed coin. It drops from above the screen onto the ground and then
/// bobs there until the player clicks it. Picking is distance based, so the coin
/// needs no collider of its own and never interferes with the Galo click handler.
/// </summary>
[RequireComponent(typeof(SpriteRenderer))]
public class RansomCoin : MonoBehaviour
{
    private SpriteRenderer _renderer;
    private float _landY;
    private float _fallSpeed;
    private float _bobPhase;

    public bool Collected { get; private set; }

    public void Initialize(Sprite sprite, float worldSize, float landY, float fallSpeed)
    {
        _renderer = GetComponent<SpriteRenderer>();
        if (_renderer != null && sprite != null)
        {
            _renderer.sprite = sprite;

            float width = Mathf.Max(sprite.bounds.size.x, 0.0001f);
            float scale = worldSize / width;
            transform.localScale = new Vector3(scale, scale, 1f);
        }

        _landY = landY;
        _fallSpeed = Mathf.Max(0.1f, fallSpeed);
        _bobPhase = Random.value * Mathf.PI * 2f;
    }

    private void Update()
    {
        if (Collected)
        {
            return;
        }

        Vector3 position = transform.position;

        if (position.y > _landY)
        {
            position.y = Mathf.MoveTowards(position.y, _landY, _fallSpeed * Time.deltaTime);
        }
        else
        {
            _bobPhase += Time.deltaTime * 4f;
            position.y = _landY + Mathf.Abs(Mathf.Sin(_bobPhase)) * 0.08f;
        }

        transform.position = position;
    }

    /// <summary>Generous circular hit test so coins are easy to pick under time pressure.</summary>
    public bool ContainsPoint(Vector2 worldPoint)
    {
        float radius = 0.45f;
        if (_renderer != null)
        {
            radius = Mathf.Max(_renderer.bounds.extents.x * 0.9f, 0.35f);
        }

        return Vector2.Distance(worldPoint, transform.position) <= radius;
    }

    public void Collect()
    {
        Collected = true;
        Destroy(gameObject);
    }
}
