using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(AudioSource))]
public class Drawing : MonoBehaviour
{
    [Header("Drawing")]
    [SerializeField] private Camera drawingCamera;
    [SerializeField] private Material lineMaterial;
    [SerializeField, Min(0.01f)] private float lineWidth = 0.15f;
    [SerializeField, Min(0.001f)] private float minimumPointDistance = 0.08f;
    [SerializeField] private float drawingPlaneZ = 0f;

    [Header("Rendering")]
    [SerializeField] private string sortingLayerName = "Default";
    [SerializeField] private int sortingOrder = 10;

    [Header("Paper Boundaries")]
    [SerializeField] private SpriteMask[] paperMasks = new SpriteMask[0];
    [SerializeField, Min(0f)] private float paperEdgeInset = 0.01f;

    [Header("Physics")]
    [SerializeField] private bool createCollider = false;
    [SerializeField] private PhysicsMaterial2D physicsMaterial;

    [Header("Drawing Sound")]
    [SerializeField] private AudioClip drawingSound;
    [SerializeField, Range(0f, 1f)] private float drawingVolume = 0.5f;
    [SerializeField, Range(0.1f, 3f)] private float drawingPitch = 1f;

    [Header("Eraser")]
    [SerializeField, Min(0.05f)] private float eraserRadius = 0.3f;
    [SerializeField] private bool eraseOnlyOnPaper = true;

    public bool IsDrawingOnPaper { get; private set; }
    public int StrokesCreated { get; private set; } = 0;

    private readonly List<Vector2> colliderPoints = new List<Vector2>();
    private LineRenderer currentLine;
    private EdgeCollider2D currentCollider;
    private SpriteMask currentPaper;
    private ColorPencil strokePencil;
    private AudioSource drawingAudioSource;
    private Material fallbackMaterial;
    private Vector3 lastWorldPoint;
    private int nextStrokeNumber = 1;

    private void Awake()
    {
        if (drawingCamera == null)
            drawingCamera = Camera.main;

        drawingAudioSource = GetComponent<AudioSource>();

        // Also handles objects that already had Drawing before RequireComponent.
        if (drawingAudioSource == null)
            drawingAudioSource = gameObject.AddComponent<AudioSource>();

        drawingAudioSource.playOnAwake = false;
        drawingAudioSource.loop = true;
        drawingAudioSource.spatialBlend = 0f;

        if (lineMaterial != null)
            return;

        Shader shader = Shader.Find("Sprites/Default");

        if (shader == null)
            shader = Shader.Find("Universal Render Pipeline/2D/Sprite-Unlit-Default");

        if (shader != null)
            fallbackMaterial = new Material(shader);
        else
            Debug.LogWarning("Assign a Line Material on the Drawing component.", this);
    }


    private void Update()
    {
        Mouse mouse = Mouse.current;
        PlayerPencil player = PlayerPencil.Instance;

        if (drawingCamera == null || !Application.isFocused || mouse == null)
        {
            EndStroke();
            return;
        }

        Vector2 screenPosition = mouse.position.ReadValue();

        if (screenPosition.x < 0f || screenPosition.y < 0f ||
            screenPosition.x >= Screen.width || screenPosition.y >= Screen.height)
        {
            EndStroke();
            return;
        }

        float depth = Mathf.Abs(
            drawingPlaneZ - drawingCamera.transform.position.z);

        Vector3 worldPosition = drawingCamera.ScreenToWorldPoint(
            new Vector3(screenPosition.x, screenPosition.y, depth));

        worldPosition.z = drawingPlaneZ;

        // =====================================
        // RIGHT CLICK: ERASE
        // Works even if pencil is broken
        // =====================================

        if (mouse.rightButton.isPressed)
        {
            EndStroke();
            EraseAtPosition(worldPosition);
            return;
        }

        // =====================================
        // LEFT CLICK: DRAW
        // Requires a working pencil
        // =====================================

        if (!mouse.leftButton.isPressed ||
            mouse.leftButton.wasReleasedThisFrame ||
            player == null ||
            !player.isHoldingPencil ||
            player.currentPencil == null ||
            player.currentPencil.isBroken)
        {
            EndStroke();
            return;
        }

        // Check if mouse is inside paper
        SpriteMask paper = GetPaperAtPosition(worldPosition);

        if (paper == null)
        {
            EndStroke();
            return;
        }

        // Separate strokes when switching papers or pencils
        if (currentLine != null &&
            (paper != currentPaper ||
             player.currentPencil != strokePencil))
        {
            EndStroke();
        }

        // Start a new stroke or continue drawing
        if (currentLine == null)
        {
            BeginStroke(worldPosition, paper, player);
        }
        else if (Vector2.Distance(
            worldPosition, lastWorldPoint) >= minimumPointDistance)
        {
            AddPoint(worldPosition);
        }

        IsDrawingOnPaper = currentLine != null;
    }


    private void BeginStroke(Vector3 worldPosition, SpriteMask paper, PlayerPencil player)
    {
        currentPaper = paper;
        strokePencil = player.currentPencil;

        GameObject stroke = new GameObject("Stroke_" + nextStrokeNumber++);
        stroke.transform.position = new Vector3(0f, 0f, drawingPlaneZ);
        stroke.transform.SetParent(transform, true);

        currentLine = stroke.AddComponent<LineRenderer>();
        currentLine.useWorldSpace = false;
        currentLine.positionCount = 0;
        currentLine.startWidth = lineWidth;
        currentLine.endWidth = lineWidth;
        currentLine.startColor = player.heldColor;
        currentLine.endColor = player.heldColor;
        currentLine.numCapVertices = 8;
        currentLine.numCornerVertices = 8;
        currentLine.alignment = LineAlignment.View;
        currentLine.sortingLayerName = sortingLayerName;
        currentLine.sortingOrder = sortingOrder;

        Material material = lineMaterial != null ? lineMaterial : fallbackMaterial;
        if (material != null)
            currentLine.sharedMaterial = material;

        // Colored strokes remain visible but have no physical collider.
        currentCollider = null;
        if (createCollider && player.heldColorName == "Black")
        {
            currentCollider = stroke.AddComponent<EdgeCollider2D>();
            currentCollider.enabled = false;
            currentCollider.isTrigger = false;
            currentCollider.edgeRadius = lineWidth * 0.5f;
            stroke.tag = "Platform";
            stroke.layer = LayerMask.NameToLayer("Ground");

            if (physicsMaterial != null)
                currentCollider.sharedMaterial = physicsMaterial;
        }

        colliderPoints.Clear();
        AddPoint(worldPosition);

        if (drawingAudioSource != null && drawingSound != null)
        {
            drawingAudioSource.clip = drawingSound;
            drawingAudioSource.volume = drawingVolume;
            drawingAudioSource.pitch = drawingPitch;

            if (!drawingAudioSource.isPlaying)
                drawingAudioSource.Play();
        }

        StrokesCreated++;
    }

    private void AddPoint(Vector3 worldPosition)
    {
        Vector3 localPosition = currentLine.transform.InverseTransformPoint(worldPosition);
        localPosition.z = 0f;

        int index = currentLine.positionCount;
        currentLine.positionCount = index + 1;
        currentLine.SetPosition(index, localPosition);

        if (currentCollider != null)
        {
            colliderPoints.Add(new Vector2(localPosition.x, localPosition.y));

            if (colliderPoints.Count >= 2)
            {
                currentCollider.SetPoints(colliderPoints);
                currentCollider.enabled = true;
            }
        }

        lastWorldPoint = worldPosition;
    }

    public void EndStroke()
    {
        IsDrawingOnPaper = false;

        if (drawingAudioSource != null && drawingAudioSource.isPlaying)
            drawingAudioSource.Stop();

        // Discard clicks that never became a complete line.
        if (currentLine != null && currentLine.positionCount < 2)
            Destroy(currentLine.gameObject);

        currentLine = null;
        currentCollider = null;
        currentPaper = null;
        strokePencil = null;
        colliderPoints.Clear();
    }

    private SpriteMask GetPaperAtPosition(Vector3 worldPosition)
    {
        // Prefer the existing paper when paper rectangles overlap.
        if (IsInsidePaper(currentPaper, worldPosition))
            return currentPaper;

        if (paperMasks != null)
        {
            foreach (SpriteMask paper in paperMasks)
            {
                if (IsInsidePaper(paper, worldPosition))
                    return paper;
            }
        }

        return null;
    }

    private bool IsInsidePaper(SpriteMask paper, Vector3 worldPosition)
    {
        if (paper == null || !paper.enabled ||
            !paper.gameObject.activeInHierarchy || paper.sprite == null)
        {
            return false;
        }

        Vector3 localPosition = paper.transform.InverseTransformPoint(worldPosition);
        Bounds bounds = paper.sprite.bounds;

        // Account for paper rotation/scale and keep line thickness inside its edges.
        float padding = lineWidth * 0.5f + paperEdgeInset;
        Matrix4x4 matrix = paper.transform.worldToLocalMatrix;
        float paddingX = padding * new Vector2(matrix.m00, matrix.m01).magnitude;
        float paddingY = padding * new Vector2(matrix.m10, matrix.m11).magnitude;

        return localPosition.x >= bounds.min.x + paddingX &&
               localPosition.x <= bounds.max.x - paddingX &&
               localPosition.y >= bounds.min.y + paddingY &&
               localPosition.y <= bounds.max.y - paddingY;
    }

    private void OnApplicationFocus(bool hasFocus)
    {
        if (!hasFocus)
            EndStroke();
    }

    private void OnDisable()
    {
        EndStroke();
    }

    private void OnDestroy()
    {
        if (fallbackMaterial != null)
            Destroy(fallbackMaterial);
    }

    private void EraseAtPosition(Vector3 worldPosition)
    {
        if (eraseOnlyOnPaper && GetPaperAtPosition(worldPosition) == null)
            return;

        List<GameObject> strokesToErase = new List<GameObject>();

        foreach (Transform child in transform)
        {
            LineRenderer line = child.GetComponent<LineRenderer>();

            if (line == null)
                continue;

            for (int i = 0; i < line.positionCount - 1; i++)
            {
                Vector3 pointA = line.useWorldSpace
                    ? line.GetPosition(i)
                    : line.transform.TransformPoint(line.GetPosition(i));

                Vector3 pointB = line.useWorldSpace
                    ? line.GetPosition(i + 1)
                    : line.transform.TransformPoint(line.GetPosition(i + 1));

                float distance = DistanceToSegment(
                    worldPosition, pointA, pointB);

                if (distance <= eraserRadius + lineWidth * 0.5f)
                {
                    strokesToErase.Add(child.gameObject);
                    break;
                }
            }
        }

        foreach (GameObject stroke in strokesToErase)
        {
            // Immediately disable collider before destroying
            EdgeCollider2D collider = stroke.GetComponent<EdgeCollider2D>();

            if (collider != null)
                collider.enabled = false;

            stroke.SetActive(false);
            Destroy(stroke);
        }
    }

    private float DistanceToSegment(
        Vector2 point, Vector2 start, Vector2 end)
    {
        Vector2 segment = end - start;

        if (segment.sqrMagnitude < 0.0001f)
            return Vector2.Distance(point, start);

        float t = Mathf.Clamp01(
            Vector2.Dot(point - start, segment) / segment.sqrMagnitude);

        Vector2 closestPoint = start + t * segment;

        return Vector2.Distance(point, closestPoint);
    }
}