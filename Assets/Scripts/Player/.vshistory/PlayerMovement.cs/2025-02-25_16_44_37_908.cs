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

        //// Flip sprite based on movement direction
        //if (moveInput.x < 0)
        //    spriteRenderer.flipX = true;
        //else if (moveInput.x > 0)
        //    spriteRenderer.flipX = false;

    }

    public void Move(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>();

        // Update animation parameters
        animator.SetFloat("InputX", moveInput.x);
        animator.SetFloat("InputY", moveInput.y);

        if (moveInput.sqrMagnitude > 0)
        {
            animator.SetBool("isWalking", true);

            // Save last movement direction
            animator.SetFloat("LastInputX", moveInput.x);
            animator.SetFloat("LastInputY", moveInput.y);
        }
        else
        {
            animator.SetBool("isWalking", false);

            // When stopping, reset InputX & InputY to (0,0) to force idle state
            animator.SetFloat("InputX", 0);
            animator.SetFloat("InputY", 0);
        }
    }


}
