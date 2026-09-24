using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Drawing : MonoBehaviour
{

    [Header("Drawing")]
    [SerializeField] private Camera drawingCamera;
    [SerializeField] private Material lineMaterial;
    [SerializeField] private Color lineColor = Color.black;

    [Tooltip("Thickness of the drawn line in world units.")]
    [SerializeField, Min(0.01f)]
    private float lineWidth = 0.15f;

    [Tooltip("How far the mouse must move before another point is added.")]
    [SerializeField, Min(0.001f)]
    private float minimumPointDistance = 0.08f;

    [Tooltip("The Z position where lines are drawn.")]
    [SerializeField]
    private float drawingPlaneZ = 0f;

    [Header("Rendering")]
    [SerializeField] private string sortingLayerName = "Default";
    [SerializeField] private int sortingOrder = 10;

    [Header("Optional Physics")]
    [Tooltip("Enable this to let the player stand and walk on drawings.")]
    [SerializeField]
    private bool createCollider = true;

    [SerializeField]
    private PhysicsMaterial2D physicsMaterial;

    [Header("Drawing Sound")]
    [Tooltip("Sound that plays while the mouse is drawing.")]
    [SerializeField]
    private AudioClip drawingSound;

    [Range(0f, 1f)]
    [SerializeField]
    private float drawingVolume = 0.5f;

    [Range(0.1f, 3f)]
    [SerializeField]
    private float drawingPitch = 1f;

    private readonly List<Vector3> linePoints =
        new List<Vector3>();

    private readonly List<Vector2> colliderPoints =
        new List<Vector2>();

    private readonly List<GameObject> strokes =
        new List<GameObject>();

    private LineRenderer currentLine;
    private EdgeCollider2D currentCollider;

    private AudioSource drawingAudioSource;

    private Vector3 lastWorldPoint;
    private Material fallbackMaterial;

    private int nextStrokeNumber = 1;

    private void Awake()
    {
        if (drawingCamera == null)
        {
            drawingCamera = Camera.main;
        }

        SetupAudioSource();
        CreateFallbackMaterial();
    }

    private void Update()
    {
        /*
         * Check for mouse release first so the sound stops even if
         * the mouse is released outside of the Game window.
         */
        if (LeftMouseReleasedThisFrame())
        {
            EndStroke();
        }

        if (drawingCamera == null)
        {
            return;
        }

        if (!TryGetMouseScreenPosition(out Vector2 screenPosition))
        {
            return;
        }

        Vector3 worldPosition =
            ScreenToDrawingPlane(screenPosition);

        if (LeftMousePressedThisFrame())
        {
            BeginStroke(worldPosition);
        }
        else if (LeftMouseIsPressed() && currentLine != null)
        {
            ContinueStroke(worldPosition);
        }
    }

    private void SetupAudioSource()
    {
        drawingAudioSource = GetComponent<AudioSource>();

        drawingAudioSource.playOnAwake = false;
        drawingAudioSource.loop = true;

        // A value of zero makes this a 2D sound.
        drawingAudioSource.spatialBlend = 0f;

        drawingAudioSource.volume = drawingVolume;
        drawingAudioSource.pitch = drawingPitch;
        drawingAudioSource.clip = drawingSound;

    }

    private void CreateFallbackMaterial()
    {
        if (lineMaterial != null)
        {
            return;
        }

        Shader spriteShader = Shader.Find("Sprites/Default");

        if (spriteShader == null)
        {
            spriteShader = Shader.Find(
                "Universal Render Pipeline/2D/Sprite-Unlit-Default"
            );
        }

        if (spriteShader != null)
        {
            fallbackMaterial = new Material(spriteShader);
        }
        else
        {
            Debug.LogWarning(
                "MouseDrawer2D could not create a line material. " +
                "Assign a material to Line Material in the Inspector."
            );
        }
    }

    private void BeginStroke(Vector3 worldPosition)
    {
        /*
         * Finish any existing stroke in case the previous mouse
         * release was missed.
         */
        if (currentLine != null)
        {
            EndStroke();
        }

        GameObject stroke = new GameObject(
            "Stroke_" + nextStrokeNumber
        );
        stroke.tag = "Platform";

        nextStrokeNumber++;

        /*
         * Keep the strokes underneath the DrawingManager object
         * in the Hierarchy.
         */
        stroke.transform.position =
            new Vector3(0f, 0f, drawingPlaneZ);

        stroke.transform.rotation = Quaternion.identity;
        stroke.transform.localScale = Vector3.one;

        stroke.transform.SetParent(transform, true);

        currentLine = stroke.AddComponent<LineRenderer>();

        ConfigureLineRenderer(currentLine);

        if (createCollider)
        {
            currentCollider =
                stroke.AddComponent<EdgeCollider2D>();

            /*
             * An EdgeCollider2D needs at least two points, so it
             * starts disabled.
             */
            currentCollider.enabled = false;
            currentCollider.isTrigger = false;
            currentCollider.edgeRadius = lineWidth * 0.5f;

            if (physicsMaterial != null)
            {
                currentCollider.sharedMaterial = physicsMaterial;
            }
        }
        else
        {
            currentCollider = null;
        }

        linePoints.Clear();
        colliderPoints.Clear();

        strokes.Add(stroke);

        AddPoint(worldPosition);
        StartDrawingSound();
    }

    private void ConfigureLineRenderer(LineRenderer line)
    {
        line.useWorldSpace = false;
        line.positionCount = 0;

        line.startWidth = lineWidth;
        line.endWidth = lineWidth;

        line.startColor = lineColor;
        line.endColor = lineColor;

        // Creates rounded ends and corners.
        line.numCapVertices = 8;
        line.numCornerVertices = 8;

        line.alignment = LineAlignment.View;

        line.sortingLayerName = sortingLayerName;
        line.sortingOrder = sortingOrder;

        Material materialToUse =
            lineMaterial != null
                ? lineMaterial
                : fallbackMaterial;

        if (materialToUse != null)
        {
            line.sharedMaterial = materialToUse;
        }
    }

    private void ContinueStroke(Vector3 worldPosition)
    {
        float distanceFromPreviousPoint =
            Vector2.Distance(
                new Vector2(
                    worldPosition.x,
                    worldPosition.y
                ),
                new Vector2(
                    lastWorldPoint.x,
                    lastWorldPoint.y
                )
            );

        if (distanceFromPreviousPoint <
            minimumPointDistance)
        {
            return;
        }

        AddPoint(worldPosition);
    }

    private void AddPoint(Vector3 worldPosition)
    {
        if (currentLine == null)
        {
            return;
        }

        /*
         * LineRenderer and EdgeCollider2D store their points
         * relative to the stroke GameObject.
         */
        Vector3 localPosition =
            currentLine.transform.InverseTransformPoint(
                worldPosition
            );

        localPosition.z = 0f;

        linePoints.Add(localPosition);

        currentLine.positionCount = linePoints.Count;

        currentLine.SetPosition(
            linePoints.Count - 1,
            localPosition
        );

        if (currentCollider != null)
        {
            colliderPoints.Add(
                new Vector2(
                    localPosition.x,
                    localPosition.y
                )
            );

            if (colliderPoints.Count >= 2)
            {
                currentCollider.SetPoints(colliderPoints);
                currentCollider.enabled = true;
            }
        }

        lastWorldPoint = worldPosition;
    }

    private void EndStroke()
    {
        StopDrawingSound();

        if (currentLine == null)
        {
            return;
        }

        /*
         * Delete the stroke if the player only clicked and did
         * not drag far enough to create a complete line.
         */
        if (currentLine.positionCount < 2)
        {
            GameObject incompleteStroke =
                currentLine.gameObject;

            strokes.Remove(incompleteStroke);
            Destroy(incompleteStroke);
        }

        ResetCurrentStroke();
    }

    private void ResetCurrentStroke()
    {
        currentLine = null;
        currentCollider = null;

        linePoints.Clear();
        colliderPoints.Clear();
    }

    private void StartDrawingSound()
    {
        if (drawingAudioSource == null ||
            drawingSound == null)
        {
            return;
        }

        drawingAudioSource.clip = drawingSound;
        drawingAudioSource.volume = drawingVolume;
        drawingAudioSource.pitch = drawingPitch;
        drawingAudioSource.loop = true;

        if (!drawingAudioSource.isPlaying)
        {
            drawingAudioSource.Play();
        }
    }

    private void StopDrawingSound()
    {
        if (drawingAudioSource != null &&
            drawingAudioSource.isPlaying)
        {
            drawingAudioSource.Stop();
        }
    }

    private Vector3 ScreenToDrawingPlane(
        Vector2 screenPosition
    )
    {
        /*
         * Example:
         * Camera Z = -10
         * Drawing plane Z = 0
         * Distance from camera = 10
         */
        float distanceFromCamera = Mathf.Abs(
            drawingPlaneZ -
            drawingCamera.transform.position.z
        );

        Vector3 worldPosition =
            drawingCamera.ScreenToWorldPoint(
                new Vector3(
                    screenPosition.x,
                    screenPosition.y,
                    distanceFromCamera
                )
            );

        worldPosition.z = drawingPlaneZ;

        return worldPosition;
    }

    private bool TryGetMouseScreenPosition(
        out Vector2 screenPosition
    )
    {
        if (Mouse.current == null)
        {
            screenPosition = Vector2.zero;
            return false;
        }

        screenPosition =
            Mouse.current.position.ReadValue();

        /*
         * Prevent drawing while the mouse is outside of the
         * Game window.
         */
        return screenPosition.x >= 0f &&
               screenPosition.y >= 0f &&
               screenPosition.x < Screen.width &&
               screenPosition.y < Screen.height;
    }

    private bool LeftMousePressedThisFrame()
    {
        return Mouse.current != null &&
               Mouse.current.leftButton
                   .wasPressedThisFrame;
    }

    private bool LeftMouseIsPressed()
    {
        return Mouse.current != null &&
               Mouse.current.leftButton.isPressed;
    }

    private bool LeftMouseReleasedThisFrame()
    {
        return Mouse.current != null &&
               Mouse.current.leftButton
                   .wasReleasedThisFrame;
    }

    /// <summary>
    /// Deletes the most recently drawn stroke.
    /// This method can be connected to a UI Button.
    /// </summary>
    //public void UndoLastStroke()
    //{
    //    if (strokes.Count == 0)
    //    {
    //        return;
    //    }

    //    int lastIndex = strokes.Count - 1;

    //    GameObject lastStroke =
    //        strokes[lastIndex];

    //    if (currentLine != null &&
    //        currentLine.gameObject == lastStroke)
    //    {
    //        StopDrawingSound();
    //        ResetCurrentStroke();
    //    }

    //    strokes.RemoveAt(lastIndex);

    //    if (lastStroke != null)
    //    {
    //        Destroy(lastStroke);
    //    }
    //}

    /// <summary>
    /// Deletes every drawn stroke.
    /// This method can be connected to a UI Button.
    /// </summary>
    //public void ClearAllStrokes()
    //{
    //    StopDrawingSound();
    //    ResetCurrentStroke();

    //    foreach (GameObject stroke in strokes)
    //    {
    //        if (stroke != null)
    //        {
    //            Destroy(stroke);
    //        }
    //    }

    //    strokes.Clear();
    //}

    private void OnApplicationFocus(bool hasFocus)
    {
        /*
         * Finish the stroke and stop the sound when the player
         * clicks outside of the Unity game.
         */
        if (!hasFocus)
        {
            EndStroke();
        }
    }

    private void OnDisable()
    {
        StopDrawingSound();
    }

    private void OnDestroy()
    {
        if (fallbackMaterial != null)
        {
            Destroy(fallbackMaterial);
        }
    }
}