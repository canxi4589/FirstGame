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

        // Normalize Input to avoid floating-point precision issues
        Vector2 normalizedInput = moveInput.sqrMagnitude > 0 ? moveInput.normalized : moveInput;

        // Set movement animation parameters
        animator.SetFloat("InputX", normalizedInput.x);
        animator.SetFloat("InputY", normalizedInput.y);
        animator.SetBool("isWalking", moveInput.sqrMagnitude > 0);

        // Store last movement direction when moving
        if (moveInput.sqrMagnitude > 0)
        {
            animator.SetFloat("LastInputX", normalizedInput.x);
            animator.SetFloat("LastInputY", normalizedInput.y);

            // Flip sprite for left/right movement
            spriteRenderer.flipX = normalizedInput.x < 0;
        }

        // Handle Idle Animation When Stopping
        if (moveInput.sqrMagnitude == 0)
        {
            animator.SetBool("isWalking", false);

            float lastX = animator.GetFloat("LastInputX");
            float lastY = animator.GetFloat("LastInputY");

            Debug.Log($"LastX: {lastX}, LastY: {lastY}"); // Debugging

            // Flip sprite only for left/right idle
            if (Mathf.Abs(lastX) > 0.1f)
                spriteRenderer.flipX = lastX < 0;

            // Set blend parameters instead of Play()
            animator.SetFloat("BlendX", lastX);
            animator.SetFloat("BlendY", lastY);
        }
    }


    public void Move(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>();
    }
}
