using UnityEngine;

/// <summary>
/// Fills this object's SpriteRenderer from a texture that lives in a Resources
/// folder, resolved by name at load time.
///
/// The Main scene's backdrop layers ("cloud", "cloud2") are plain sprites that
/// carry no reference of their own once imported, so a layer whose texture was
/// dropped in from another project needs one of these to give itself a picture.
///
/// <see cref="Resources.LoadAll{T}"/> is used rather than Resources.Load so the
/// result does not depend on whether the texture was imported as a Single sprite
/// or sliced into a Multiple sprite sheet.
/// </summary>
[RequireComponent(typeof(SpriteRenderer))]
public class SpriteFromResources : MonoBehaviour
{
    [Tooltip("Path under a Resources folder, without the file extension.")]
    [SerializeField] private string _resourcePath = "Starlight";

    private void Awake()
    {
        SpriteRenderer renderer = GetComponent<SpriteRenderer>();

        // A sprite dragged in through the Inspector always wins.
        if (renderer.sprite != null || string.IsNullOrWhiteSpace(_resourcePath))
        {
            return;
        }

        Sprite[] sprites = Resources.LoadAll<Sprite>(_resourcePath);
        if (sprites == null || sprites.Length == 0)
        {
            Debug.LogWarning(
                $"{name}: no Sprite found at 'Resources/{_resourcePath}'. Check that the " +
                "texture exists there and that its Import Settings use Texture Type 'Sprite'.",
                this);
            return;
        }

        renderer.sprite = sprites[0];
    }
}
