using UnityEngine;

/**
*
* For colorable objects in the painitngs. When the player scribbles 
* on the object for longer than 1.5 seconds with the correct color, 
* we should switch to using the colored version of sprite, and users can be able to walk on it
*
**/

public class BlankObject : MonoBehaviour
{
    public string requiredColor = "Green";
    public bool isColored = false;
    
    [Header("Scribble Settings")]
    private float fillDuration = 0.5f; // How long to scribble (seconds)
    
    [Header("Visuals")]
    public Sprite blankSprite;     // The uncolored/outline sprite
    public Sprite coloredSprite;   // The fully colored sprite

    private SpriteRenderer spriteRenderer;
    private Collider2D objectCollider;
    private float scribbleTime = 0f;
    
    private PlayerPencil cachedPlayerPencil;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        objectCollider = GetComponent<Collider2D>();
        
        if (objectCollider != null)
            objectCollider.isTrigger = true;
        
        // Set blank sprite
        if (blankSprite != null && spriteRenderer != null)
            spriteRenderer.sprite = blankSprite;
    }

    void Update()
    {
        if (isColored) return;
        
        if (cachedPlayerPencil == null)
            cachedPlayerPencil = FindObjectOfType<PlayerPencil>();
        
        if (Input.GetMouseButton(0))
        {
            Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            RaycastHit2D hit = Physics2D.Raycast(mousePos, Vector2.zero);
            
            if (hit.collider != null && hit.collider.gameObject == gameObject)
            {
                if (cachedPlayerPencil == null || !cachedPlayerPencil.isHoldingPencil) return;
                if (cachedPlayerPencil.heldColorName != requiredColor) return;
                
                // Accumulate scribble time
                scribbleTime += Time.deltaTime;
                
                Debug.Log("Scribbling... " + scribbleTime.ToString("F1") + "s / " + fillDuration + "s");
                
                // Check if enough time has passed
                if (scribbleTime >= fillDuration)
                {
                    ColorObject(cachedPlayerPencil.heldColor);
                }
            }
        }
        else
        {
            // Mouse released - reset scribble timer
            if (!isColored && scribbleTime > 0f)
            {
                scribbleTime = 0f;
                Debug.Log("Scribble cancelled");
            }
        }
    }

    public void ColorObject(Color color)
    {
        if (isColored) return;
        
        isColored = true;
        scribbleTime = 0f;
        
        // Swap to colored sprite
        if (coloredSprite != null && spriteRenderer != null)
        {
            spriteRenderer.sprite = coloredSprite;
            spriteRenderer.color = Color.white;
        }
        else if (spriteRenderer != null)
        {
            spriteRenderer.color = color;
        }
        
        // Make it walkable
        if (objectCollider != null)
            objectCollider.isTrigger = false;
        
        gameObject.tag = "PaintedPlatform";
        
        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayColoringSound();
        
        Debug.Log("Colored " + gameObject.name + " with " + requiredColor);
    }
}