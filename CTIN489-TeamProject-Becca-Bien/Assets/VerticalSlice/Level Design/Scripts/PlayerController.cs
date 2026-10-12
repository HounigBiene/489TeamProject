using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 7f;
    public float jumpForce = 12f;
    
    [Header("Ground Check")]
    public Transform groundCheck;        
    public float groundCheckRadius = 0.2f;
    public LayerMask groundLayer;        // Which layers count as ground
    
    [Header("Jump Feel")]
    public float fallMultiplier = 2.5f;  // Faster falling
    public float lowJumpMultiplier = 2f; // Short tap = short jump
    public float coyoteTime = 0.15f;     // Grace period after leaving ground
    public float jumpBufferTime = 0.15f; // Grace period for early jump press
    
    private Rigidbody2D rb;
    private bool isGrounded;
    private float moveX;
    private float coyoteTimer;
    private float jumpBufferTimer;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;
    }

    void Update()
    {
        // Movement input
        moveX = 0f;
        if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))
            moveX = -1f;
        if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow))
            moveX = 1f;

        // Ground check with OverlapCircle
        if (groundCheck != null)
        {
            isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
        }

        // Coyote time - allow jump shortly after leaving ground
        if (isGrounded)
            coyoteTimer = coyoteTime;
        else
            coyoteTimer -= Time.deltaTime;

        // Jump buffer - allow jump pressed slightly before landing
        if (Input.GetKeyDown(KeyCode.Space))
            jumpBufferTimer = jumpBufferTime;
        else
            jumpBufferTimer -= Time.deltaTime;

        // Jump - works if grounded OR within coyote time, AND within jump buffer
        if (jumpBufferTimer > 0f && coyoteTimer > 0f)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
            jumpBufferTimer = 0f;
            coyoteTimer = 0f;
        }

        // Reset
        if (Input.GetKeyDown(KeyCode.R))
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
    }

    void FixedUpdate()
    {
        // Horizontal movement
        rb.linearVelocity = new Vector2(moveX * moveSpeed, rb.linearVelocity.y);

        // Better jump feel - faster falling, shorter tap = shorter jump
        if (rb.linearVelocity.y < 0)
        {
            // Falling - apply fall multiplier
            rb.linearVelocity += Vector2.up * Physics2D.gravity.y * (fallMultiplier - 1) * Time.fixedDeltaTime;
        }
        else if (rb.linearVelocity.y > 0 && !Input.GetKey(KeyCode.Space))
        {
            // Rising but not holding jump - apply low jump multiplier
            rb.linearVelocity += Vector2.up * Physics2D.gravity.y * (lowJumpMultiplier - 1) * Time.fixedDeltaTime;
        }
    }

    void OnDrawGizmosSelected()
    {
        if (groundCheck != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
        }
    }
}