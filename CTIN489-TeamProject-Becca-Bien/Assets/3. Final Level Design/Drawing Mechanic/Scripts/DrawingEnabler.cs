using UnityEngine;

public class DrawingEnabler : MonoBehaviour
{

    [Header("Scripts")]
    public Drawing drawingScript;
    public PlayerPencil playerPencil;
    public PencilDurability pencilDurability;

    [Header("Drawing Status")]
    public bool shouldDraw = false;

    void Start()
    {

        if (drawingScript == null)
            drawingScript = Object.FindAnyObjectByType<Drawing>();

        if (playerPencil == null)
            playerPencil = Object.FindAnyObjectByType<PlayerPencil>();

        if (pencilDurability == null)
            pencilDurability = GetComponent<PencilDurability>();

        if (pencilDurability == null && playerPencil != null)
            pencilDurability =
                playerPencil.GetComponent<PencilDurability>();

        if (drawingScript == null)
        {
            Debug.LogError("No Drawing script found in scene!");
        }

        if(playerPencil == null)
        {
            Debug.LogError("No PlayerPencil found!");
        }
        
        if (pencilDurability == null)
        {
            Debug.LogError("No Pencil Durability found in scene!");
        }
        
        if (drawingScript != null)
        {
            drawingScript.enabled = false;
        }
    }

    void Update()
    {
        if (drawingScript == null || pencilDurability == null) 
            return;

        // Enable drawing ONLY when holding the drawing pencil
        shouldDraw = pencilDurability.CanDraw();
        
        if (drawingScript.enabled != shouldDraw)
        {
            drawingScript.enabled = shouldDraw;
            Debug.Log("Drawing " + (shouldDraw ? "ENABLED" : "DISABLED"));
        }
    }
}