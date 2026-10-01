using UnityEngine;

public class PlayerAnimator : MonoBehaviour
{
    [Header("Sprites")]
    public Sprite frontSprite;   // Facing forward/idle
    public Sprite sideSprite;    // Facing left/right (side view)
    public Sprite backSprite;    // Facing away (optional)
    
    [Header("Settings")]
    public bool flipSideSprite = false; // If side sprite faces left, flip it for right
    
    private SpriteRenderer spriteRenderer;
    private Vector2 lastDirection = Vector2.down; // Start facing down (front)
    private float moveX;
    private float moveY;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        
        // Start with front sprite
        if (frontSprite != null)
            spriteRenderer.sprite = frontSprite;
    }

    void Update()
    {
        // Get input direction
        float inputX = 0f;
        float inputY = 0f;
        
        if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))
            inputX = -1f;
        if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow))
            inputX = 1f;
        if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow))
            inputY = 1f;
        if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow))
            inputY = -1f;
        
        Vector2 direction = new Vector2(inputX, inputY);
        
        // Only update if player is moving
        if (direction.magnitude > 0.1f)
        {
            direction.Normalize();
            
            // Determine which sprite to show
            if (Mathf.Abs(inputX) > 0.1f)
            {
                // Moving horizontally → side view
                ShowSideSprite(inputX);
            }
            else if (inputY > 0.1f)
            {
                // Moving up → back view (or side)
                ShowBackSprite();
            }
            else if (inputY < -0.1f)
            {
                // Moving down → front view
                ShowFrontSprite();
            }
            
            lastDirection = direction;
        }
        else
        {
            // Idle - show last direction
            if (Mathf.Abs(lastDirection.x) > 0.1f)
                ShowSideSprite(lastDirection.x);
            else if (lastDirection.y > 0)
                ShowBackSprite();
            else
                ShowFrontSprite();
        }
    }

    void ShowFrontSprite()
    {
        if (frontSprite != null && spriteRenderer.sprite != frontSprite)
        {
            spriteRenderer.sprite = frontSprite;
            spriteRenderer.flipX = false;
        }
    }

    void ShowSideSprite(float directionX)
    {
        if (sideSprite != null)
        {
            spriteRenderer.sprite = sideSprite;
            
            // Flip based on direction
            if (flipSideSprite)
                spriteRenderer.flipX = directionX > 0;
            else
                spriteRenderer.flipX = directionX < 0;
        }
    }

    void ShowBackSprite()
    {
        if (backSprite != null)
        {
            spriteRenderer.sprite = backSprite;
            spriteRenderer.flipX = false;
        }
        else
        {
            // No back sprite - use side view
            ShowSideSprite(lastDirection.x);
        }
    }
}