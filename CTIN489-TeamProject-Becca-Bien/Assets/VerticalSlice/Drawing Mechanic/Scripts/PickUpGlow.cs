using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A soft, pulsing outline for nearby 2D objects.
/// Attach beside the object's SpriteRenderer and existing Collider2D.
/// Uses readable, unpacked sprite textures; does not change pickup or physics.
/// Call IsInReach() from the object's interaction method to enforce the range.
/// </summary>
[DisallowMultipleComponent]
[RequireComponent(typeof(SpriteRenderer))]
public class PickupGlow : MonoBehaviour
{
    [Header("Reach")]
    [SerializeField, Min(0f)] private float pickupReach = 1.5f;
    [Tooltip("Optional. Otherwise uses PlayerPencil.Instance.")]
    [SerializeField] private Transform player;

    [Header("Glow")]
    [SerializeField] private Color glowColor = new Color(1f, 0.85f, 0.2f, 1f);
    [SerializeField, Range(1, 12)] private int outlinePixels = 2;
    [SerializeField, Range(1, 24)] private int glowPixels = 6;
    [SerializeField, Min(0f)] private float pulseSpeed = 3f;
    [Tooltip("Optional unlit sprite material. Otherwise uses the object's material.")]
    [SerializeField] private Material glowMaterial;

    private SpriteRenderer sourceRenderer;
    private SpriteRenderer glowRenderer;
    private Collider2D objectCollider;
    private readonly Dictionary<Sprite, Sprite> cachedGlows =
        new Dictionary<Sprite, Sprite>();

    private void Awake()
    {
        sourceRenderer = GetComponent<SpriteRenderer>();
        objectCollider = GetComponent<Collider2D>();

        if (objectCollider == null)
        {
            Debug.LogError("PickupGlow needs a Collider2D on the same object.", this);
            enabled = false;
            return;
        }

        GameObject glowObject = new GameObject("Pickup Glow");
        glowObject.layer = gameObject.layer;
        glowObject.transform.SetParent(transform, false);
        glowRenderer = glowObject.AddComponent<SpriteRenderer>();
        glowRenderer.enabled = false;
    }

    // Both the visual effect and the pickup code use this exact check.
    public bool IsInReach()
    {
        if (!isActiveAndEnabled || sourceRenderer == null ||
            !sourceRenderer.enabled || sourceRenderer.sprite == null ||
            objectCollider == null || !objectCollider.enabled)
        {
            return false;
        }

        if (player == null && PlayerPencil.Instance != null)
            player = PlayerPencil.Instance.transform;

        if (player == null || !player.gameObject.activeInHierarchy)
            return false;

        Vector2 playerPosition = player.position;
        Vector2 nearestPoint = objectCollider.ClosestPoint(playerPosition);
        float reach = Mathf.Max(0f, pickupReach);

        return (nearestPoint - playerPosition).sqrMagnitude <= reach * reach;
    }

    private void LateUpdate()
    {
        if (glowRenderer == null)
            return;

        // A held pencil has its renderer/collider disabled, so it cannot glow.
        if (!IsInReach())
        {
            glowRenderer.enabled = false;
            return;
        }

        Sprite source = sourceRenderer.sprite;
        if (!cachedGlows.TryGetValue(source, out Sprite glowSprite))
        {
            glowSprite = CreateGlow(source);
            // Cache failures too, so setup errors do not log every frame.
            cachedGlows.Add(source, glowSprite);
        }

        glowRenderer.sprite = glowSprite;
        glowRenderer.enabled = glowSprite != null;
        if (glowSprite == null)
            return;

        glowRenderer.sharedMaterial = glowMaterial != null
            ? glowMaterial : sourceRenderer.sharedMaterial;
        glowRenderer.flipX = sourceRenderer.flipX;
        glowRenderer.flipY = sourceRenderer.flipY;
        glowRenderer.maskInteraction = sourceRenderer.maskInteraction;
        glowRenderer.sortingLayerID = sourceRenderer.sortingLayerID;
        glowRenderer.sortingOrder = sourceRenderer.sortingOrder;
        glowRenderer.spriteSortPoint = sourceRenderer.spriteSortPoint;
        glowRenderer.gameObject.layer = gameObject.layer;

        // The generated sprite is transparent inside the original silhouette.
        // Matching sorting orders also preserves custom SpriteMask ranges.
        float pulse = pulseSpeed > 0f
            ? Mathf.Lerp(0.65f, 1f, (Mathf.Sin(Time.time * pulseSpeed) + 1f) * 0.5f)
            : 1f;
        Color color = glowColor;
        color.a *= pulse * sourceRenderer.color.a;
        glowRenderer.color = color;
    }

    private Sprite CreateGlow(Sprite source)
    {
        if (source.packed || !source.texture.isReadable)
        {
            Debug.LogError(
                "PickupGlow: use an unpacked sprite and enable Read/Write on " +
                source.name + "'s texture. Then restart Play mode.", this);
            return null;
        }

        if (sourceRenderer.drawMode != SpriteDrawMode.Simple)
        {
            Debug.LogError("PickupGlow supports Simple SpriteRenderer draw mode.", this);
            return null;
        }

        Color32[] originalPixels;
        try
        {
            originalPixels = source.texture.GetPixels32();
        }
        catch (UnityException exception)
        {
            Debug.LogError("Cannot read glow texture: " + exception.Message, this);
            return null;
        }

        int rim = Mathf.Clamp(outlinePixels, 1, 12);
        int halo = Mathf.Clamp(glowPixels, 1, 24);
        int padding = rim + halo + 1;
        Rect rect = source.rect;
        int originalWidth = Mathf.RoundToInt(rect.width);
        int originalHeight = Mathf.RoundToInt(rect.height);
        int startX = Mathf.RoundToInt(rect.x);
        int startY = Mathf.RoundToInt(rect.y);
        int width = originalWidth + padding * 2;
        int height = originalHeight + padding * 2;

        if (width > SystemInfo.maxTextureSize || height > SystemInfo.maxTextureSize)
        {
            Debug.LogError("Sprite is too large to generate a glow: " + source.name, this);
            return null;
        }

        float[] distances = new float[width * height];
        for (int i = 0; i < distances.Length; i++)
            distances[i] = 1000000f;

        // Visible pixels are the starting points for the outline.
        for (int y = 0; y < originalHeight; y++)
        {
            for (int x = 0; x < originalWidth; x++)
            {
                int sourceIndex = (startY + y) * source.texture.width + startX + x;
                if (originalPixels[sourceIndex].a > 25)
                    distances[(y + padding) * width + x + padding] = 0f;
            }
        }

        // Two-pass distance transform: approximate distance from opaque pixels.
        // Runs once per sprite, not every frame.
        for (int pass = 0; pass < 2; pass++)
        {
            int step = pass == 0 ? 1 : -1;
            int firstY = pass == 0 ? 0 : height - 1;
            int firstX = pass == 0 ? 0 : width - 1;

            for (int y = firstY; y >= 0 && y < height; y += step)
            {
                for (int x = firstX; x >= 0 && x < width; x += step)
                {
                    int index = y * width + x;
                    float d = distances[index];
                    d = Mathf.Min(d, ReadDistance(distances, width, height, x - step, y) + 1f);
                    d = Mathf.Min(d, ReadDistance(distances, width, height, x, y - step) + 1f);
                    d = Mathf.Min(d, ReadDistance(distances, width, height, x - 1, y - step) + 1.414214f);
                    d = Mathf.Min(d, ReadDistance(distances, width, height, x + 1, y - step) + 1.414214f);
                    distances[index] = d;
                }
            }
        }

        Color32[] glowPixelsData = new Color32[distances.Length];
        for (int i = 0; i < distances.Length; i++)
        {
            float d = distances[i];
            if (d <= 0f || d >= rim + halo)
                continue;

            float fade = Mathf.Clamp01(1f - (d - rim) / halo);
            float alpha = d <= rim ? 1f : fade * fade;
            glowPixelsData[i] = new Color32(255, 255, 255,
                (byte)Mathf.RoundToInt(alpha * 255f));
        }

        Texture2D texture = new Texture2D(width, height, TextureFormat.RGBA32, false);
        texture.name = source.name + "_PickupGlow";
        texture.filterMode = FilterMode.Bilinear;
        texture.wrapMode = TextureWrapMode.Clamp;
        texture.SetPixels32(glowPixelsData);
        texture.Apply(false, true);

        Vector2 pivot = new Vector2(
            (source.pivot.x + padding) / width,
            (source.pivot.y + padding) / height);

        Sprite result = Sprite.Create(texture, new Rect(0, 0, width, height),
            pivot, source.pixelsPerUnit, 0, SpriteMeshType.FullRect, Vector4.zero, false);
        result.name = texture.name;
        return result;
    }

    private static float ReadDistance(float[] values, int width, int height, int x, int y)
    {
        return x >= 0 && x < width && y >= 0 && y < height
            ? values[y * width + x] : 1000000f;
    }

    private void OnDisable()
    {
        if (glowRenderer != null)
            glowRenderer.enabled = false;
    }

    private void OnDestroy()
    {
        foreach (Sprite sprite in cachedGlows.Values)
        {
            if (sprite == null)
                continue;
            Destroy(sprite.texture);
            Destroy(sprite);
        }

        if (glowRenderer != null)
            Destroy(glowRenderer.gameObject);
    }
}
