using UnityEngine;

/// <summary>
/// Turns the cloud into the ransom trap on some passes.
///
/// CloudMoving raises Wrapped every time the cloud reaches the right edge and is
/// moved back to the left one. On that event we roll the configured chance: a hit
/// makes the cloud glitch, wear the stop sign and become clickable. Clicking it
/// starts the ransom; leaving it alone means it simply rides to the right edge,
/// turns normal again and refreshes as usual.
/// </summary>
[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(CloudMoving))]
public class GlitchCloud : MonoBehaviour
{
    [Tooltip("Optional. When empty, RansomEvent.Instance is used, which owns the chance value.")]
    [SerializeField] private RansomEvent _ransom;

    [Tooltip("How wide the stop sign sits on the cloud, in world units.")]
    [SerializeField] private float _stopSignSize = 0.62f;

    private SpriteRenderer _renderer;
    private CloudMoving _moving;
    private Color _originalColor;
    private Vector3 _originalScale;

    private GameObject _stopSignObject;
    private BoxCollider2D _hitbox;
    private float _seed;
    private bool _isGlitched;

    public bool IsGlitched => _isGlitched;

    private void Awake()
    {
        _renderer = GetComponent<SpriteRenderer>();
        _moving = GetComponent<CloudMoving>();
        _originalColor = _renderer.color;
        _originalScale = transform.localScale;
    }

    private void OnEnable()
    {
        _moving.Wrapped += OnWrapped;
    }

    private void OnDisable()
    {
        _moving.Wrapped -= OnWrapped;
    }

    /// <summary>One pass finished: clear any glitch, then roll for a new one.</summary>
    private void OnWrapped()
    {
        if (_isGlitched)
        {
            ClearGlitch();
        }

        RansomEvent evt = ResolveRansom();
        if (evt == null)
        {
            return;
        }

        if (Random.value * 100f < evt.GlitchChancePercent)
        {
            BecomeGlitched();
        }
    }

    private void Update()
    {
        if (!_isGlitched)
        {
            return;
        }

        // Colour flicker, flip bursts and a rocking sign read as "corrupted"
        // without fighting CloudMoving for the transform position.
        float t = Time.time * 20f + _seed;
        _renderer.color = Color.Lerp(
            _originalColor,
            new Color(0.75f, 0.05f, 0.05f, 1f),
            Mathf.PingPong(t * 0.35f, 1f));
        _renderer.flipX = Mathf.Repeat(t * 0.15f, 1f) > 0.85f;

        if (_stopSignObject != null)
        {
            _stopSignObject.transform.localRotation = Quaternion.Euler(0f, 0f, Mathf.Sin(t * 0.7f) * 18f);
        }

        if (Input.GetMouseButtonDown(0) && PointerOverHitbox())
        {
            Trigger();
        }
    }

    private void BecomeGlitched()
    {
        _isGlitched = true;
        _seed = Random.value * 97f;

        Sprite stopSign = LoadStopSign();
        if (stopSign != null)
        {
            _stopSignObject = new GameObject("StopSign");
            _stopSignObject.transform.SetParent(transform, false);
            _stopSignObject.transform.localPosition = Vector3.zero;

            SpriteRenderer signRenderer = _stopSignObject.AddComponent<SpriteRenderer>();
            signRenderer.sprite = stopSign;
            signRenderer.sortingOrder = _renderer.sortingOrder + 1;

            float scale = ScaleForChild(stopSign, _stopSignSize);
            _stopSignObject.transform.localScale = new Vector3(scale, scale, 1f);
        }

        // Only the glitched cloud is clickable, so the hitbox is added and removed live.
        _hitbox = gameObject.AddComponent<BoxCollider2D>();
        if (_renderer.sprite != null)
        {
            _hitbox.size = _renderer.sprite.bounds.size;
        }
    }

    private void ClearGlitch()
    {
        _isGlitched = false;
        _renderer.color = _originalColor;
        _renderer.flipX = false;
        transform.localScale = _originalScale;

        if (_stopSignObject != null)
        {
            Destroy(_stopSignObject);
            _stopSignObject = null;
        }

        if (_hitbox != null)
        {
            Destroy(_hitbox);
            _hitbox = null;
        }
    }

    private void Trigger()
    {
        RansomEvent evt = ResolveRansom();
        ClearGlitch();

        if (evt != null)
        {
            evt.BeginFromGlitchCloud();
        }
    }

    private bool PointerOverHitbox()
    {
        if (_hitbox == null)
        {
            return false;
        }

        Camera cam = Camera.main;
        if (cam == null)
        {
            return false;
        }

        Vector2 pointer = cam.ScreenToWorldPoint(Input.mousePosition);
        Collider2D hit = Physics2D.OverlapPoint(pointer);
        return hit != null && hit == _hitbox;
    }

    private RansomEvent ResolveRansom()
    {
        if (_ransom == null)
        {
            _ransom = RansomEvent.Instance;
        }

        return _ransom;
    }

    private static Sprite LoadStopSign()
    {
        Sprite[] sprites = Resources.LoadAll<Sprite>("Ransom/stop_sign");
        if (sprites != null && sprites.Length > 0)
        {
            return sprites[0];
        }

        Debug.LogWarning("GlitchCloud: no Sprite at 'Resources/Ransom/stop_sign'.");
        return null;
    }

    /// <summary>Local scale that makes a child sprite <paramref name="worldSize"/> units wide.</summary>
    private float ScaleForChild(Sprite sprite, float worldSize)
    {
        if (sprite == null)
        {
            return 1f;
        }

        float parentScale = Mathf.Max(Mathf.Abs(transform.localScale.x), 0.0001f);
        float width = Mathf.Max(sprite.bounds.size.x, 0.0001f);
        return worldSize / (width * parentScale);
    }
}
