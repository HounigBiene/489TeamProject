using UnityEngine;
using UnityEngine.InputSystem;

public class PencilDurability : MonoBehaviour
{

    [Header("Durability")]
    public float isDullAfterSeconds = 30f;
    public float isBrokenAfterSeconds = 60f;

    [Header("Current State")]
    public float currentUseTime = 0f;
    public bool isDull = false;
    public bool isBroken = false;

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
    void Update()
    {
        if (playerPencil == null)
            return;

        if (!playerPencil.isHoldingPencil)
            return;

        if (isBroken)
            return;

        bool currentlyDrawing =
            drawingScript != null &&
            drawingScript.enabled &&
            Mouse.current != null &&
            Mouse.current.leftButton.isPressed;

        // Time starts when drawing is enabled and mouse is in use
        if (currentlyDrawing)
        {
            currentUseTime += Time.deltaTime;

            CheckDurability();
        }
    }

    void CheckDurability()
    {
        if(currentUseTime >= isBrokenAfterSeconds)
        {
            if(!isBroken)
            {
                isBroken = true;
                isDull = true;

                Debug.Log("Pencil broke!");

                UpdatePencilSprite();
            }

            return;
        }

        if(currentUseTime >= isDullAfterSeconds)
        {
            if(!isDull)
            {
                isDull = true;

                Debug.Log("Pencil is dull!");

                UpdatePencilSprite();
            }
        }

        return;
    }

    public void ResetDurability()
    {
        currentUseTime = 0f;
        isDull = false;
        isBroken = false;

        UpdatePencilSprite();
    }

    void UpdatePencilSprite()
    {
        if (playerPencil == null)
            return;

        if (playerPencil.pencilIndicator == null)
            return;

        if (isBroken)
        {
            playerPencil.pencilIndicator.sprite =
                playerPencil.currentPencil.brokenSprite;
        }
        else if (isDull)
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
       !isBroken;
    }
}
