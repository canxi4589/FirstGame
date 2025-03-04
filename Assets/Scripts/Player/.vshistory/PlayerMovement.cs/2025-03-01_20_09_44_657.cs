using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    private float moveSpeed = 5f;
    private Rigidbody2D rb;
    private Vector2 moveInput;
    private Animator animator;
    private SpriteRenderer spriteRenderer;

    private float lastInputX = 0;
    private float lastInputY = -1; // Default facing downward

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        rb.velocity = moveInput * moveSpeed;

        // Flip sprite based on movement direction
        if (moveInput.x < 0)
            spriteRenderer.flipX = true;
        else if (moveInput.x > 0)
            spriteRenderer.flipX = false;
    }

    public void Move(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>();

        // Update animation parameters
        animator.SetFloat("InputX", moveInput.x);
        animator.SetFloat("InputY", moveInput.y);

        if (moveInput.sqrMagnitude > 0) // Player is moving
        {
            animator.SetBool("isWalking", true);

            // Update last input direction only when the player is moving
            lastInputX = moveInput.x;
            lastInputY = moveInput.y;
        }
        else // Player stopped moving
        {
            animator.SetBool("isWalking", false);
        }

        // Set last input direction in Animator
        animator.SetFloat("LastInputX", lastInputX);
        animator.SetFloat("LastInputY", lastInputY);
    }
}
