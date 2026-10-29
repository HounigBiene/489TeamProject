using UnityEngine;
using UnityEngine.InputSystem;

public class PencilDurability : MonoBehaviour
{

    [Header("Durability")]
    public float isDullAfterSeconds = 30f;
    public float isBrokenAfterSeconds = 60f;

    private PlayerPencil playerPencil;
    private Drawing drawingScript;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        playerPencil = GetComponent<PlayerPencil>();
        drawingScript = Object.FindAnyObjectByType<Drawing>();

        UpdatePencilSprite();
    }

    // Update is called once per frame
    void LateUpdate()
    {
        if (playerPencil == null)
            return;

        if (!playerPencil.isHoldingPencil)
            return;

        ColorPencil pencil =
            playerPencil.currentPencil;

        if (pencil.isBroken)
            return;

        bool currentlyDrawing =
            drawingScript != null &&
            drawingScript.isActiveAndEnabled &&
            drawingScript.IsDrawingOnPaper;

        // Time starts when drawing is enabled and mouse is in use
        if (currentlyDrawing)
        {
            pencil.currentUseTime += Time.deltaTime;

            CheckDurability(pencil);
        }
    }

    private void CheckDurability(ColorPencil pencil)
    {
        if (pencil.currentUseTime >= isBrokenAfterSeconds)
        {
            pencil.currentUseTime = isBrokenAfterSeconds;

            // Only record the transition from usable to broken.
            if (!pencil.isBroken)
            {
                pencil.isBroken = true;
                pencil.isDull = true;

                UpdatePencilSprite();

                if (PencilBreakTracker.Instance != null)
                {
                    PencilBreakTracker.Instance.RecordPencilBreak(
                        pencil.colorName,
                        playerPencil.transform.position
                    );
                }
                else
                {
                    Debug.LogError(
                        "PencilBreakTracker is missing from the scene!",
                        this
                    );
                }
            }

            return;
        }

        if (pencil.currentUseTime >= isDullAfterSeconds)
        {
            if (!pencil.isDull)
            {
                pencil.isDull = true;

                UpdatePencilSprite();

                Debug.Log(pencil.colorName + " pencil is dull!");
            }
        }
    }

    public void UpdatePencilSprite()
    {
        if (playerPencil == null)
            return;

        if (playerPencil.pencilIndicator == null)
            return;

        if (playerPencil.currentPencil == null)
            return;

        ColorPencil pencil =
            playerPencil.currentPencil;

        if (pencil.isBroken)
        {
            playerPencil.pencilIndicator.sprite =
                playerPencil.currentPencil.brokenSprite;
        }
        else if (pencil.isDull)
        {
            playerPencil.pencilIndicator.sprite =
                playerPencil.currentPencil.dullSprite;
        }
        else
        {
            playerPencil.pencilIndicator.sprite =
                playerPencil.currentPencil.sharpSprite;
        }

        playerPencil.pencilIndicator.color = Color.white;
    }

    public bool CanDraw()
    {
        return playerPencil != null &&
       playerPencil.isHoldingPencil &&
       !playerPencil.currentPencil.isBroken;
    }
}
