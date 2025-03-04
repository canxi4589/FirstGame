using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    private float moveSpeed = 5f;
    private Rigidbody2D rb;
    private Vector2 moveInput;
    private Animator animator;
    private SpriteRenderer spriteRenderer;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        rb.velocity = moveInput * moveSpeed;

        // Set movement animation parameters
        animator.SetFloat("InputX", moveInput.x);
        animator.SetFloat("InputY", moveInput.y);
        animator.SetBool("isWalking", moveInput.sqrMagnitude > 0);

        // Store last movement direction for idle animations
        if (moveInput.sqrMagnitude > 0)
        {
            animator.SetFloat("LastInputX", moveInput.x);
            animator.SetFloat("LastInputY", moveInput.y);

            // Flip sprite for left/right movement
            if (moveInput.x < 0)
                spriteRenderer.flipX = true;
            else if (moveInput.x > 0)
                spriteRenderer.flipX = false;
        }

        // Handle Idle Animation When Stopping
        if (moveInput.sqrMagnitude == 0)
        {
            float lastX = animator.GetFloat("LastInputX");
            float lastY = animator.GetFloat("LastInputY");

            // Flip sprite based on last movement before stopping
            if (lastX < 0)
                spriteRenderer.flipX = true;
            else if (lastX > 0)
                spriteRenderer.flipX = false;

            // Choose the correct idle animation
            if (lastX == 1 && lastY == 0)
                animator.Play("IdleAnimation"); // Idle East
            else if (lastX == 1 && lastY == 1)
                animator.Play("IdleBackEastAnimation"); // Idle North-East
            else if (lastY == 1)
                animator.Play("IdleBackAnimation"); // Idle North
            else if (lastY == -1)
                animator.Play("Idle_South"); // Idle South
        }
    }

    public void Move(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>();
    }
}
