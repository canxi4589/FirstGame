using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    private float moveSpeed = 5f;
    private Rigidbody2D rb;
    private Vector2 moveInput;
    private Animator animator;
    private SpriteRenderer spriteRenderer;

    private float lastInputX = 1;
    private float lastInputY = 0; // Default to facing downward

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

            if (moveInput.x != 0 || moveInput.y != 0)
            {
                lastInputX = moveInput.x;
                lastInputY = moveInput.y;
            }
        }
        else // Player stopped moving
        {
            animator.SetBool("isWalking", false);
        }

        animator.SetFloat("LastInputX", lastInputX);
        animator.SetFloat("LastInputY", lastInputY);
    }

    // ?? New Method to Update Facing Direction When Shooting
    public void UpdateAimingDirection(Vector2 aimDirection)
    {
        lastInputX = aimDirection.x;
        lastInputY = aimDirection.y;

        animator.SetFloat("LastInputX", lastInputX);
        animator.SetFloat("LastInputY", lastInputY);
    }
}
