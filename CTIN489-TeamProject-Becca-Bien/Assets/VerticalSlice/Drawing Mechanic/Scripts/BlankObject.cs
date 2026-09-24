using UnityEngine;

/**
*
* For colorable objects in the painitngs. When they are clicked on with the correct color, 
* we should switch to using the colored version of sprite, and users can be able to walk on it
*
**/
public class BlankObject : MonoBehaviour
{
    public string requiredColor = "Green";
    public bool isColored = false;
    
    [Header("Visuals")]
    public Sprite coloredSprite;  // The colored version 

    private SpriteRenderer spriteRenderer;
    private Collider2D objectCollider;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        objectCollider = GetComponent<Collider2D>();
        
        // Start as trigger (not walkable)
        if (objectCollider != null)
            objectCollider.isTrigger = true;
    }

    void OnMouseDown()
    {
        // Player clicked this blank object
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null) return;
        
        PlayerPencil playerPencil = player.GetComponent<PlayerPencil>();
        if (playerPencil == null) return;
        
        // Check if player is holding a pencil
        if (!playerPencil.isHoldingPencil)
        {
            Debug.Log("No pencil! Pick one up first.");
            return;
        }
        
        // Check if it's the right color
        if (playerPencil.heldColorName != requiredColor)
        {
            Debug.Log("Wrong color. Needs: " + requiredColor + " (You have: " + playerPencil.heldColorName + ")");
            return;
        }
        
        // Color the object!
        ColorObject(playerPencil.heldColor);
    }

    public void ColorObject(Color color)
    {
        if (isColored) return;
        
        isColored = true;
        
        // Change sprite to colored version
        if (coloredSprite != null)
        {
            spriteRenderer.sprite = coloredSprite;
            spriteRenderer.color = Color.white; // Use sprite's own colors
        }
        else
        {
            spriteRenderer.color = color; // Tint the sprite
        }
        
        // Make it walkable (remove trigger)
        if (objectCollider != null)
            objectCollider.isTrigger = false;
        
        // Change tag
        gameObject.tag = "PaintedPlatform";
        
        // Play color sound
        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayColoringSound();
        
        Debug.Log("Colored " + gameObject.name + " with " + requiredColor);
    }
}