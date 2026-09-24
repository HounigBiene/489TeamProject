using UnityEngine;

public class DrawingEnabler : MonoBehaviour
{
    private Drawing drawingScript;
    private PlayerPencil playerPencil;

    void Start()
    {
        // Auto-find the Drawing component in the scene
        drawingScript = FindObjectOfType<Drawing>();
        
        // Auto-find the PlayerPencil component
        playerPencil = FindObjectOfType<PlayerPencil>();
        
        if (drawingScript == null)
        {
            Debug.LogError("No Drawing script found in scene!");
        }
        
        if (playerPencil == null)
        {
            Debug.LogError("No PlayerPencil found in scene!");
        }
        
        // Start with drawing disabled
        if (drawingScript != null)
        {
            drawingScript.enabled = false;
            Debug.Log("Drawing disabled at start");
        }
    }

    void Update()
    {
        if (drawingScript == null || playerPencil == null) return;
        
        // Enable drawing ONLY when holding the drawing pencil
        bool shouldDraw = playerPencil.isHoldingPencil && playerPencil.isDrawingPencil;
        
        if (drawingScript.enabled != shouldDraw)
        {
            drawingScript.enabled = shouldDraw;
            Debug.Log("Drawing " + (shouldDraw ? "ENABLED" : "DISABLED"));
        }
    }
}