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
    public float dashSpeedMultiplier = 2f;
    public float dashDuration = 0.1f;
    public float dashCooldown = 1f;

    private bool isDashing = false;
    private float dashEndTime = 0f;
    private float lastDashTime = -Mathf.Infinity;
    private Vector2 dashDirection;


    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        if (isDashing)
        {
            if (Time.time >= dashEndTime) // Stop dashing when time is up
            {
                isDashing = false;
                rb.velocity = Vector2.zero;
            }
            return;
        }


        rb.velocity = moveInput * moveSpeed;

        // Flip sprite based on movement direction
        if (moveInput.x < 0)
            spriteRenderer.flipX = true;
        else if (moveInput.x > 0)
            spriteRenderer.flipX = false;
    }

    public void Move(InputAction.CallbackContext context)
    {
        if (isDashing) return; // Prevent movement during dash

        moveInput = context.ReadValue<Vector2>();

        // Update animation parameters
        animator.SetFloat("InputX", moveInput.x);
        animator.SetFloat("InputY", moveInput.y);

        if (moveInput.sqrMagnitude > 0.01f) // Player is moving
        {
            animator.SetBool("isWalking", true);

            // Only update lastInputX and lastInputY if the movement is significant
            lastInputX = moveInput.x;
            lastInputY = moveInput.y;
        }
        else 
        {
            animator.SetBool("isWalking", false);
        }

        // Ensure we keep the last direction by checking if movement stops
        animator.SetFloat("LastInputX", lastInputX);
        animator.SetFloat("LastInputY", lastInputY);
    }


    public void UpdateAimingDirection(Vector2 aimDirection)
    {
        lastInputX = aimDirection.x;
        lastInputY = aimDirection.y;

        animator.SetFloat("LastInputX", lastInputX);
        animator.SetFloat("LastInputY", lastInputY);
    }
    public void Dash(InputAction.CallbackContext context)
    {
        if (context.started && !isDashing && Time.time >= lastDashTime + dashCooldown)
        {
            isDashing = true;
            lastDashTime = Time.time;
            dashEndTime = Time.time + dashDuration;

            dashDirection = new Vector2(lastInputX, lastInputY).normalized;
            rb.velocity = dashDirection * moveSpeed * dashSpeedMultiplier;
        }
    }
}
