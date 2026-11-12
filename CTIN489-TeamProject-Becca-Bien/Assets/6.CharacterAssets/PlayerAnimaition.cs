using UnityEngine;

public class PlayerAnimation : MonoBehaviour
{
    private Animator anim;
    private Rigidbody2D rb;

    void Start()
    {
        anim = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        // 1. Check if the player is moving horizontally
        float horizontalVelocity = rb.linearVelocity.x;
        bool isMoving = Mathf.Abs(horizontalVelocity) > 0.1f;
        
        anim.SetBool("isWalking", isMoving);

        // 2. Set the facing direction based on velocity
        if (horizontalVelocity > 0.1f)
        {
            anim.SetBool("facingRight", true);
        }
        else if (horizontalVelocity < -0.1f)
        {
            anim.SetBool("facingRight", false);
        }

        // 3. Jumping Animation (Leaving this commented out for now)
        // anim.SetBool("isGrounded", isGrounded); 
    }
}