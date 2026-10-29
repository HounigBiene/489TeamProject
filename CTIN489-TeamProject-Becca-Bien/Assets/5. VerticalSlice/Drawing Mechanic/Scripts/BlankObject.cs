using UnityEngine;

public class BlankObject : MonoBehaviour
{
    public string requiredColor = "Green";
    public bool isColored = false;
    
    [Header("Scribble Settings")]
    public float fillDuration = 1.5f; // Total seconds needed to color it
    
    [Header("Visuals")]
    public Sprite blankSprite;     
    public Sprite coloredSprite;   

    private SpriteRenderer spriteRenderer;
    private Collider2D objectCollider;
    private float fillProgress = 0f; // accumulates over time
    
    private PlayerPencil cachedPlayerPencil;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        objectCollider = GetComponent<Collider2D>();
        
        if (objectCollider != null)
            objectCollider.isTrigger = true;
        
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
                
                // ADD to the progress instead of resetting it
                fillProgress += Time.deltaTime;
                
                // Check if enough TOTAL time has passed
                if (fillProgress >= fillDuration)
                {
                    ColorObject(cachedPlayerPencil.heldColor);
                }
            }
        }
    }

    public void ColorObject(Color color)
    {
        if (isColored) return;
        
        isColored = true;
        fillProgress = 0f; 
        
        if (coloredSprite != null && spriteRenderer != null)
        {
            spriteRenderer.sprite = coloredSprite;
            spriteRenderer.color = Color.white;
        }
        else if (spriteRenderer != null)
        {
            spriteRenderer.color = color;
        }
        
        if (objectCollider != null)
            objectCollider.isTrigger = false;
        
        gameObject.tag = "PaintedPlatform";
        
        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayColoringSound();
        
        Debug.Log("Colored " + gameObject.name + " with " + requiredColor);
    }
}