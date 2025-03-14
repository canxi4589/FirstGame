using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI; // For UI elements (Image, Text)
using TMPro; // For TextMeshPro

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

    // UI Variables
    [SerializeField] private int maxHealth = 5; // Max 5 hearts
    private int currentHealth;
    [SerializeField] private int ammo = 57;     // Initial ammo
    [SerializeField] private int coins = 1;     // Initial coins
    [SerializeField] private int keys = 0;      // Initial keys
    [SerializeField] private GameObject heartContainer; // Reference to HeartContainer
    [SerializeField] private GameObject coinContainer;  // Reference to CoinContainer
    [SerializeField] private GameObject keyContainer;   // Reference to KeyContainer

    private Image[] heartImages;
    private Text ammoText;
    private TextMeshProUGUI coinText;
    private Text keyText;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        mainCamera = Camera.main;
        currentHealth = maxHealth; // Initialize health

        // Initialize UI references from containers
        heartImages = heartContainer.GetComponentsInChildren<Image>();
        ammoText = coinContainer.transform.Find("AmmoText")?.GetComponent<Text>();
        coinText = coinContainer.transform.Find("CoinText")?.GetComponent<TextMeshProUGUI>();
        keyText = keyContainer.transform.Find("KeyText")?.GetComponent<Text>();

        if (heartImages == null || heartImages.Length == 0 || ammoText == null || coinText == null || keyText == null)
        {
            Debug.LogError("UI components not found! Check container names and hierarchy.");
        }

        UpdateHealthUI();
        UpdateAmmoUI();
        UpdateCoinUI();
        UpdateKeyUI();
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

        Vector3 mousePosition = mainCamera.ScreenToWorldPoint(Input.mousePosition);
        Vector2 aimDirection = (mousePosition - transform.position).normalized;
        UpdateAimingDirection(aimDirection);
        //spriteRenderer.flipX = lastInputX < 0;
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

    void Dash(InputAction.CallbackContext context)
    {
        if (context.started && !isDashing && Time.time >= lastDashTime + dashCooldown)
        {
            isDashing = true;
            lastDashTime = Time.time;
            dashEndTime = Time.time + dashDuration;

            dashDirection = moveInput.sqrMagnitude > 0 ? moveInput.normalized : new Vector2(lastInputX, lastInputY);
            rb.velocity = dashDirection * moveSpeed * dashSpeedMultiplier;

            Invoke(nameof(EndDash), dashDuration); // Automatically call EndDash after dashDuration
        }
    }

    void EndDash()
    {
        isDashing = false;
        rb.velocity = moveInput.sqrMagnitude > 0 ? moveInput * moveSpeed : Vector2.zero;
    }

    // UI Update Methods
    private void UpdateHealthUI()
    {
        for (int i = 0; i < heartImages.Length; i++)
        {
            heartImages[i].enabled = (i < currentHealth); // Enable hearts based on health
        }
    }

    private void UpdateAmmoUI()
    {
        if (ammoText != null)
        {
            ammoText.text = ammo.ToString();
        }
    }

    private void UpdateCoinUI()
    {
        if (coinText != null)
        {
            coinText.text = coins.ToString();
        }
    }

    private void UpdateKeyUI()
    {
        if (keyText != null)
        {
            keyText.text = keys.ToString();
        }
    }

    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
        currentHealth = Mathf.Max(0, currentHealth);
        UpdateHealthUI();
        if (currentHealth == 0)
        {
            Debug.Log("Player died!");
            // Add game over logic here
        }
    }
    public void UseAmmo(int amount)
    {
        ammo -= amount;
        ammo = Mathf.Max(0, ammo);
        UpdateAmmoUI();
    }

    public void AddCoins(int amount)
    {
        coins += amount;
        UpdateCoinUI();
    }

    public void AddKeys(int amount)
    {
        keys += amount;
        UpdateKeyUI();
    }

    public int GetAmmo()
    {
        return ammo;
    }
}