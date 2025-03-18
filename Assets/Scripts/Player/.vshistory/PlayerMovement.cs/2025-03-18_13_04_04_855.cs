using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI; // For UI elements (Image, Text)
using TMPro; // For TextMeshPro
using UnityEngine.SceneManagement; // For scene management

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float dashSpeedMultiplier = 2f;
    [SerializeField] private float dashDuration = 0.1f;
    [SerializeField] private float dashCooldown = 1f;
    [SerializeField] private float dashIFrameDuration = 0.08f; // I-frames during dash
    [SerializeField] private float footstepInterval = 0.4f;  // Time between footstep sounds (in seconds)
    private float footstepTimer = 0f;                        // Timer for footstep delay

    private Rigidbody2D rb;
    private Vector2 moveInput;
    private Animator animator;
    private SpriteRenderer spriteRenderer;
    private Camera mainCamera;

    private float lastInputX = 1;
    private float lastInputY = 0;
    private bool isDashing = false;
    private float dashEndTime = 0f;
    private float dashIFrameEndTime = 0f; // When i-frames end during dash
    private float lastDashTime = -Mathf.Infinity;
    private Vector2 dashDirection;
    private Vector3 lastSafePosition; // Store the last safe position
    [SerializeField] private float fallDuration = 0.4f; // Duration of the fall animation
    private bool isFalling = false;

    // UI Variables
    [SerializeField] private int maxHealth = 5; // Max 5 hearts
    private int currentHealth;
    [SerializeField] private int ammo = 57;     // Initial ammo
    [SerializeField] private int coins = 1;     // Initial coins
    [SerializeField] private int keys = 0;      // Initial keys
    [SerializeField] private GameObject heartContainer; // Reference to HeartContainer
    [SerializeField] private GameObject coinContainer;  // Reference to CoinContainer
    [SerializeField] private GameObject keyContainer;   // Reference to KeyContainer
    [SerializeField] private Image fadePanel;           // Fade panel for death sequence
    [SerializeField] private GameObject gameOverPanel;  // Game over/victory panel

    // Game over/victory UI elements
    [SerializeField] private TextMeshProUGUI titleText;    // "YOU DIED" or "YOU WIN"
    [SerializeField] private TextMeshProUGUI timeText;     // Time value
    [SerializeField] private TextMeshProUGUI moneyText;    // Money value
    [SerializeField] private TextMeshProUGUI killsText;    // Kills value
    [SerializeField] private Button quickRestartButton;
    [SerializeField] private Button returnToBreachButton;
    [SerializeField] private AudioClip defaultTransitionSound; // Assign in Inspector
    [SerializeField] private AudioClip fallSound; // Assign fall sound in Inspector
    [SerializeField] private AudioClip hurtSound; // Assign hurt sound in Inspector

    private Image[] heartImages;
    private Text ammoText;
    private TextMeshProUGUI coinText;
    private Text keyText;

    // Invincibility Variables
    [SerializeField] private float invincibilityDuration = 1.5f;
    private float invincibilityTime = 0f; 
    private bool isInvincible = false;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        mainCamera = Camera.main;
        currentHealth = maxHealth; // Initialize health
        var playerInput = GetComponent<PlayerInput>();
        if (playerInput != null)
        {
            playerInput.enabled = false;
            playerInput.enabled = true;
            Debug.Log("PlayerInput reinitialized");
        }

        // Initialize UI references from containers
        heartImages = heartContainer.GetComponentsInChildren<Image>();
        coinText = coinContainer.transform.Find("CoinText")?.GetComponent<TextMeshProUGUI>();
        keyText = keyContainer.transform.Find("KeyText")?.GetComponent<Text>();

        if (heartImages == null || heartImages.Length == 0 || coinText == null || keyText == null)
        {
            Debug.LogError("UI components not found! Check container names and hierarchy.");
        }

        UpdateHealthUI();
        UpdateAmmoUI();
        UpdateCoinUI();
        UpdateKeyUI();

        // Initialize game over/victory panel
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(false);
            CanvasGroup canvasGroup = gameOverPanel.GetComponent<CanvasGroup>();
            if (canvasGroup == null) canvasGroup = gameOverPanel.AddComponent<CanvasGroup>();
            canvasGroup.alpha = 0; // Start fully transparent
            canvasGroup.interactable = false; // Prevent blocking input during animation
            canvasGroup.blocksRaycasts = false; // Allow raycasts to pass through until fully active
        }

        // Set up buttons
        if (quickRestartButton != null)
        {
            quickRestartButton.onClick.AddListener(QuickRestart);
        }
        if (returnToBreachButton != null)
        {
            returnToBreachButton.onClick.AddListener(ReturnToBreach);
        }
        lastSafePosition = transform.position;

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
        if (moveInput.sqrMagnitude > 0.01f && !isDashing) // Only play footsteps when moving and not dashing
        {
            footstepTimer -= Time.deltaTime;
            if (footstepTimer <= 0f)
            {
                if (SoundManager.Instance != null)
                {
                    SoundManager.Instance.PlayFootstepSound();
                }
                footstepTimer = footstepInterval; // Reset timer
            }
        }
        else
        {
            footstepTimer = footstepInterval; // Reset timer when not moving
        }

        Vector3 mousePosition = mainCamera.ScreenToWorldPoint(Input.mousePosition);
        Vector2 aimDirection = (mousePosition - transform.position).normalized;
        UpdateAimingDirection(aimDirection);

        // Handle invincibility blinking
        if (isInvincible)
        {
            spriteRenderer.enabled = Mathf.Sin(Time.time * 10) > 0; // Blink effect
            if (Time.time >= invincibilityTime)
            {
                isInvincible = false;
                spriteRenderer.enabled = true; // Ensure visible when invincibility ends
            }
        }
        if (!isFalling && !isDashing && moveInput.sqrMagnitude > 0.01f)
        {
            lastSafePosition = transform.position;
        }


    }

    public void Move(InputAction.CallbackContext context)
    {
        if (isDashing) return; 

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
            dashIFrameEndTime = Time.time + dashIFrameDuration; // Set i-frame duration

            dashDirection = moveInput.sqrMagnitude > 0 ? moveInput.normalized : new Vector2(lastInputX, lastInputY);
            rb.velocity = dashDirection * moveSpeed * dashSpeedMultiplier;
            if (SoundManager.Instance != null)
            {
                SoundManager.Instance.PlayDashSound();
            }
            Invoke(nameof(EndDash), dashDuration); 
        }
    }

    void EndDash()
    {
        isDashing = false;
        // Ensure velocity is zero if no input, otherwise use current move input
        rb.velocity = moveInput.sqrMagnitude > 0 ? moveInput * moveSpeed : Vector2.zero;
    }

    // UI Update Methods
    private void UpdateHealthUI()
    {
        for (int i = 0; i < heartImages.Length; i++)
        {
            heartImages[i].enabled = (i < currentHealth);
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
            if (GameManager.Instance != null)
            {
                GameManager.Instance.AddMoney(coins); // Sync with GameManager
            }
            if (SoundManager.Instance != null)
            {
                SoundManager.Instance.PlayCoinCollectSound();
            }
        }
    }

    private void UpdateKeyUI()
    {
        if (keyText != null)
        {
            keyText.text = keys.ToString();
        }
    }

    // Public Methods to Modify Values
    public void TakeDamage(int damage)
    {
        bool isDashInvincible = isDashing && Time.time <= dashIFrameEndTime;
        if (!isInvincible && !isDashInvincible)
        {
            currentHealth -= damage;
            currentHealth = Mathf.Max(0, currentHealth);
            UpdateHealthUI();
            StartCoroutine(FlashRed());
            StartCoroutine(ScreenShake(0.2f, 0.1f));
            isInvincible = true;
            invincibilityTime = Time.time + invincibilityDuration;

            if (SoundManager.Instance != null)
            {
                SoundManager.Instance.PlayHurtSound();
            }

            if (currentHealth == 0)
            {
                StartCoroutine(EndGameSequence(false));
            }
        }
    }

    // Method to trigger victory (call this when the player wins, e.g., defeats a boss)
    public void TriggerVictory()
    {
        StartCoroutine(EndGameSequence(true)); // Trigger victory sequence
    }

    private System.Collections.IEnumerator EndGameSequence(bool isVictory)
    {
        // Freeze time (except for animations)
        Time.timeScale = 0f;
        enabled = false; // Disable movement and other scripts
        rb.velocity = Vector2.zero; // Stop movement

        // Fade the screen to black, keeping player visible
        if (fadePanel != null)
        {
            float fadeDuration = 0.5f;
            float elapsed = 0f;
            Color startColor = fadePanel.color;
            Color endColor = new Color(0, 0, 0, 0.7f); // Semi-transparent black to ensure player visibility

            while (elapsed < fadeDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                fadePanel.color = Color.Lerp(startColor, endColor, elapsed / fadeDuration);
                yield return null;
            }
            fadePanel.color = endColor;
        }

        // Play animation (death for game over, victory animation if available)
        animator.SetTrigger(isVictory ? "Victory" : "Die");

        // Wait for the animation to complete (using unscaled time)
        yield return new WaitForSecondsRealtime(animator.GetCurrentAnimatorStateInfo(0).length);
        Debug.Log("Animation completed, duration: " + animator.GetCurrentAnimatorStateInfo(0).length); // Debug timing

        // Populate game over/victory panel with stats
        if (GameManager.Instance != null)
        {
            if (titleText != null) titleText.text = isVictory ? "YOU WIN" : "YOU DIED";
            if (timeText != null) timeText.text = GameManager.Instance.GetTimeFormatted();
            if (moneyText != null) moneyText.text = GameManager.Instance.GetMoney().ToString();
            if (killsText != null) killsText.text = GameManager.Instance.GetKills().ToString();
        }

        // Fade in the game over/victory panel
        if (gameOverPanel != null)
        {
            CanvasGroup canvasGroup = gameOverPanel.GetComponent<CanvasGroup>();
            if (canvasGroup == null) canvasGroup = gameOverPanel.AddComponent<CanvasGroup>();
            gameOverPanel.SetActive(true);
            canvasGroup.interactable = true; // Enable interaction
            canvasGroup.blocksRaycasts = true; // Allow blocking raycasts only after animation
            float fadeInDuration = 0.5f;
            float elapsed = 0f;
            while (elapsed < fadeInDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                canvasGroup.alpha = elapsed / fadeInDuration;
                yield return null;
            }
            canvasGroup.alpha = 1;
        }
    }

    private void QuickRestart()
    {
        Time.timeScale = 1f; // Restore time
        if (GameManager.Instance != null)
        {
            GameManager.Instance.ResetStats(); // Reset stats for a new run
        }
        SceneManager.LoadScene(SceneManager.GetActiveScene().name); // Reload current scene
    }

    private void ReturnToBreach()
    {
        Time.timeScale = 1f; // Restore time
        if (GameManager.Instance != null)
        {
            GameManager.Instance.ResetStats(); // Reset stats for a new run
        }
        SceneManager.LoadScene("Breach"); // Load hub scene (create this scene)
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

    // Getter methods
    public int GetAmmo()
    {
        return ammo;
    }

    public int GetHealth()
    {
        return currentHealth;
    }

    // Coroutines for Effects
    private System.Collections.IEnumerator FlashRed()
    {
        spriteRenderer.color = Color.red;
        yield return new WaitForSeconds(0.2f);
        spriteRenderer.color = Color.white;
    }

    private System.Collections.IEnumerator ScreenShake(float duration, float magnitude)
    {
        Vector3 originalPos = mainCamera.transform.position;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            float xOffset = Random.Range(-1f, 1f) * magnitude;
            float yOffset = Random.Range(-1f, 1f) * magnitude;

            mainCamera.transform.position = new Vector3(originalPos.x + xOffset, originalPos.y + yOffset, originalPos.z);
            elapsed += Time.deltaTime;
            yield return null;
        }

        mainCamera.transform.position = originalPos; 
    }
    public AudioClip GetDefaultTransitionSound()
    {
        return defaultTransitionSound; // Returns the default sound if no specific sound is set
    }

    public void TeleportToPosition(Vector3 position)
    {
        transform.position = position;
        Debug.Log("Teleported to " + position);

        if (mainCamera != null)
        {
            Vector3 cameraPosition = new Vector3(position.x, position.y, mainCamera.transform.position.z);
            mainCamera.transform.position = cameraPosition;
            Debug.Log("Camera moved to " + cameraPosition);
        }
        else
        {
            Debug.LogWarning("Main camera is not assigned!");
        }
    }
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Hole") && !isDashing)
        {
            StartCoroutine(FallIntoHole());
        }
    }

    private System.Collections.IEnumerator FallIntoHole()
    {
        if (isFalling) yield break;

        isFalling = true;
        rb.velocity = Vector2.zero; // Immediately stop movement when falling starts
        animator.SetTrigger("Fall");

        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlayFallSound();
        }

        yield return new WaitForSeconds(fallDuration);

        TakeDamage(1);

        if (currentHealth > 0)
        {
            TeleportToPosition(lastSafePosition);
            isFalling = false;
            animator.ResetTrigger("Fall");
        }
    }



}