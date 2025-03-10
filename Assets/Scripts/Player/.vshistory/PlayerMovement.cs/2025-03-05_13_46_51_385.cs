using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float dashSpeedMultiplier = 2f;
    [SerializeField] private float dashDuration = 0.1f;
    [SerializeField] private float dashCooldown = 1f;

    private Rigidbody2D rb;
    private Vector2 moveInput;
    private Animator animator;
    private SpriteRenderer spriteRenderer;
    private Camera mainCamera;

    private float lastInputX = 1;
    private float lastInputY = 0;
    private bool isDashing = false;
    private float dashEndTime = 0f;
    private float lastDashTime = -Mathf.Infinity;
    private Vector2 dashDirection;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        mainCamera = Camera.main;
    }

    void Update()
    {
        if (isDashing)
        {
            if (Time.time >= dashEndTime)
            {
                isDashing = false;
                rb.velocity = Vector2.zero;
            }
            return;
        }

        rb.velocity = moveInput * moveSpeed;

        // Update facing direction based on the cursor
        UpdateAimingDirection();

        // Flip sprite if cursor is to the left or right
        spriteRenderer.flipX = lastInputX < 0;
    }

    public void Move(InputAction.CallbackContext context)
    {
        if (isDashing) return; // Prevent movement during dash

        moveInput = context.ReadValue<Vector2>();

        animator.SetFloat("InputX", moveInput.x);
        animator.SetFloat("InputY", moveInput.y);

        // Walking Animation Logic
        if (moveInput.sqrMagnitude > 0.01f)
        {
            animator.SetBool("isWalking", true);
        }
        else
        {
            animator.SetBool("isWalking", false);
        }

        // Update LastInputX & LastInputY based on the cursor
        animator.SetFloat("LastInputX", lastInputX);
        animator.SetFloat("LastInputY", lastInputY);
    }

    public void UpdateAimingDirection(Vector2 aimDirection)
    {
        if (aimDirection.sqrMagnitude > 0.01f) // Ensure a valid direction
        {
            lastInputX = aimDirection.x;
            lastInputY = aimDirection.y;

            animator.SetFloat("LastInputX", lastInputX);
            animator.SetFloat("LastInputY", lastInputY);
        }
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
