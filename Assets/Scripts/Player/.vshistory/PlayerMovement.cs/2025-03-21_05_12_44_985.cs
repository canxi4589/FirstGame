using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float dashSpeedMultiplier = 2f;
    [SerializeField] private float dashDuration = 0.1f;
    [SerializeField] private float dashCooldown = 1f;
    [SerializeField] private float dashIFrameDuration = 0.08f;

    private Rigidbody2D rb;
    private Vector2 moveInput;
    private Animator animator;
    private SpriteRenderer spriteRenderer;
    private Camera mainCamera;

    private float lastInputX = 1;
    private float lastInputY = 0;
    private bool isDashing = false;
    private float dashEndTime = 0f;
    private float dashIFrameEndTime = 0f;
    private float lastDashTime = -Mathf.Infinity;
    private Vector2 dashDirection;

    [SerializeField] private float fireRate = 0.5f;
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private float bulletSpeed = 10f;
    [SerializeField] private int bulletDamage = 1;
    private float shootTimer = 0f;

    [SerializeField] private float footstepInterval = 0.4f;
    private float footstepTimer = 0f;

    [SerializeField] private int maxHealth = 5;
    private int currentHealth;
    [SerializeField] private int ammo = 57;
    [SerializeField] private int coins = 1;
    [SerializeField] private int keys = 0;
    [SerializeField] private GameObject heartContainer;
    [SerializeField] private GameObject coinContainer;
    [SerializeField] private GameObject keyContainer;
    [SerializeField] private Image fadePanel;
    [SerializeField] private GameObject gameOverPanel;

    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI timeText;
    [SerializeField] private TextMeshProUGUI moneyText;
    [SerializeField] private TextMeshProUGUI killsText;
    [SerializeField] private Button quickRestartButton;
    [SerializeField] private Button returnToBreachButton;

    private Image[] heartImages;
    private Text ammoText;
    private TextMeshProUGUI coinText;
    private Text keyText;

    [SerializeField] private float invincibilityDuration = 1.5f;
    private float invincibilityTime = 0f;
    private bool isInvincible = false;

    [SerializeField] private GameObject tablePrefab;
    private GameObject currentTable;

    [SerializeField] private AudioClip defaultTransitionSound;
    [SerializeField] private AudioClip spikeHitSound;
    [SerializeField] private AudioClip trapHoleSound;

    private bool isInSpikeArea = false;
    private float lastSpikeDamageTime = -Mathf.Infinity;
    [SerializeField] private float spikeDamageCooldown = 0.5f;

    private bool isInTrapHoleArea = false;
    private bool isInTrapHoleFallZone = false;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        mainCamera = Camera.main;
        currentHealth = maxHealth;

        heartImages = heartContainer.GetComponentsInChildren<Image>();
        ammoText = coinContainer.transform.Find("AmmoText")?.GetComponent<Text>();
        coinText = coinContainer.transform.Find("CoinText")?.GetComponent<TextMeshProUGUI>();
        keyText = keyContainer.transform.Find("KeyText")?.GetComponent<Text>();

        if (heartImages == null || heartImages.Length == 0 || ammoText == null || coinText == null || keyText == null)
        {
            Debug.LogError("UI components not found!");
        }

        UpdateHealthUI();
        UpdateAmmoUI();
        UpdateCoinUI();
        UpdateKeyUI();

        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(false);
            CanvasGroup canvasGroup = gameOverPanel.GetComponent<CanvasGroup>();
            if (canvasGroup == null) canvasGroup = gameOverPanel.AddComponent<CanvasGroup>();
            canvasGroup.alpha = 0;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
        }

        if (fadePanel != null)
        {
            fadePanel.color = new Color(0, 0, 0, 0);
        }

        if (quickRestartButton != null)
        {
            quickRestartButton.onClick.AddListener(QuickRestart);
        }
        if (returnToBreachButton != null)
        {
            returnToBreachButton.onClick.AddListener(ReturnToBreach);
        }
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

        if (shootTimer > 0)
        {
            shootTimer -= Time.deltaTime;
        }

        if (moveInput.sqrMagnitude > 0.01f && !isDashing && Time.time > 0.1f)
        {
            footstepTimer -= Time.deltaTime;
            if (footstepTimer <= 0f)
            {
                if (SoundManager.Instance != null)
                {
                    SoundManager.Instance.PlayFootstepSound();
                }
                footstepTimer = footstepInterval;
            }
        }
        else
        {
            footstepTimer = footstepInterval;
        }

        Vector3 mousePosition = mainCamera.ScreenToWorldPoint(Input.mousePosition);
        Vector2 aimDirection = (mousePosition - transform.position).normalized;
        UpdateAimingDirection(aimDirection);

        if (isInvincible)
        {
            spriteRenderer.enabled = Mathf.Sin(Time.time * 10) > 0;
            if (Time.time >= invincibilityTime)
            {
                isInvincible = false;
                spriteRenderer.enabled = true;
            }
        }

        if (Input.GetKeyDown(KeyCode.E) && currentTable != null)
        {
            FlipTable();
        }

        // Handle spike damage
        if (isInSpikeArea && Time.time >= lastSpikeDamageTime + spikeDamageCooldown)
        {
            TakeDamage(1);
            lastSpikeDamageTime = Time.time;
        }

        // Handle trap hole fall with animation
        if (isInTrapHoleFallZone)
        {
            bool isDashInvincible = isDashing && Time.time <= dashIFrameEndTime;
            if (!isDashInvincible)
            {
                if (SoundManager.Instance != null && trapHoleSound != null)
                {
                    SoundManager.Instance.PlaySound(trapHoleSound);
                    Debug.Log("Played trap hole sound");
                }
                animator.SetTrigger("Fall"); // Trigger fall animation
                StartCoroutine(DelayedDeath());
            }
        }
    }

    public void Move(InputAction.CallbackContext context)
    {
        if (isDashing) return;

        moveInput = context.ReadValue<Vector2>();

        animator.SetFloat("InputX", moveInput.x);
        animator.SetFloat("InputY", moveInput.y);

        if (moveInput.sqrMagnitude > 0.01f)
        {
            animator.SetBool("isWalking", true);
        }
        else
        {
            animator.SetBool("isWalking", false);
        }

        animator.SetFloat("LastInputX", lastInputX);
        animator.SetFloat("LastInputY", lastInputY);
    }

    public void UpdateAimingDirection(Vector2 aimDirection)
    {
        if (aimDirection.sqrMagnitude > 0.01f)
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
            dashIFrameEndTime = Time.time + dashIFrameDuration;

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
        rb.velocity = moveInput.sqrMagnitude > 0 ? moveInput * moveSpeed : Vector2.zero;
    }

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
                GameManager.Instance.AddMoney(coins);
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

            if (currentHealth == 0)
            {
                StartCoroutine(EndGameSequence(false));
            }
            else if (SoundManager.Instance != null && spikeHitSound != null)
            {
                SoundManager.Instance.PlaySound(spikeHitSound);
                Debug.Log("Played spike hit sound");
            }
        }
    }

    public void AddCoins(int amount)
    {
        coins += amount;
        UpdateCoinUI();

        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlayCoinCollectSound();
        }
    }

    public void AddKeys(int amount)
    {
        keys += amount;
        UpdateKeyUI();
    }

    public void TriggerVictory()
    {
        StartCoroutine(EndGameSequence(true));
    }

    private System.Collections.IEnumerator EndGameSequence(bool isVictory)
    {
        Time.timeScale = 0f;
        enabled = false;
        rb.velocity = Vector2.zero;

        if (fadePanel != null)
        {
            float fadeDuration = 0.5f;
            float elapsed = 0f;
            Color startColor = fadePanel.color;
            Color endColor = new Color(0, 0, 0, 0.7f);

            while (elapsed < fadeDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                fadePanel.color = Color.Lerp(startColor, endColor, elapsed / fadeDuration);
                yield return null;
            }
            fadePanel.color = endColor;
        }

        animator.SetTrigger(isVictory ? "Victory" : "Die");

        yield return new WaitForSecondsRealtime(animator.GetCurrentAnimatorStateInfo(0).length);
        Debug.Log("Animation completed, duration: " + animator.GetCurrentAnimatorStateInfo(0).length);

        if (GameManager.Instance != null)
        {
            if (titleText != null) titleText.text = isVictory ? "YOU WIN" : "YOU DIED";
            if (timeText != null) timeText.text = GameManager.Instance.GetTimeFormatted();
            if (moneyText != null) moneyText.text = GameManager.Instance.GetMoney().ToString();
            if (killsText != null) killsText.text = GameManager.Instance.GetKills().ToString();
        }

        if (gameOverPanel != null)
        {
            CanvasGroup canvasGroup = gameOverPanel.GetComponent<CanvasGroup>();
            if (canvasGroup == null) canvasGroup = gameOverPanel.AddComponent<CanvasGroup>();
            gameOverPanel.SetActive(true);
            canvasGroup.interactable = true;
            canvasGroup.blocksRaycasts = true;
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

    private System.Collections.IEnumerator DelayedDeath()
    {
        yield return new WaitForSeconds(animator.GetCurrentAnimatorStateInfo(0).length); // Wait for fall animation to finish
        currentHealth = 0;
        UpdateHealthUI();
        StartCoroutine(EndGameSequence(false));
    }

    private void QuickRestart()
    {
        Time.timeScale = 1f;
        if (GameManager.Instance != null)
        {
            GameManager.Instance.ResetStats();
        }
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    private void ReturnToBreach()
    {
        Time.timeScale = 1f;
        if (GameManager.Instance != null)
        {
            GameManager.Instance.ResetStats();
        }
        SceneManager.LoadScene("Breach");
    }

    public void UseAmmo(int amount)
    {
        ammo -= amount;
        ammo = Mathf.Max(0, ammo);
        UpdateAmmoUI();
    }

    public int GetAmmo()
    {
        return ammo;
    }

    public int GetHealth()
    {
        return currentHealth;
    }

    public AudioClip GetDefaultTransitionSound()
    {
        return defaultTransitionSound;
    }

    public void TeleportToPosition(Vector3 position)
    {
        transform.position = position;
        Debug.Log("Teleported to " + position);
    }

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

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Spike"))
        {
            isInSpikeArea = true;
            if (Time.time >= lastSpikeDamageTime + spikeDamageCooldown)
            {
                TakeDamage(1);
                lastSpikeDamageTime = Time.time;
            }
        }
        else if (collision.CompareTag("TrapHole"))
        {
            isInTrapHoleArea = true;
        }
        else if (collision.CompareTag("TrapHoleFall"))
        {
            isInTrapHoleFallZone = true;
        }
        else if (collision.CompareTag("Table"))
        {
            currentTable = collision.gameObject;
        }
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.CompareTag("Spike"))
        {
            isInSpikeArea = true;
        }
        else if (collision.CompareTag("TrapHole"))
        {
            isInTrapHoleArea = true;
        }
        else if (collision.CompareTag("TrapHoleFall"))
        {
            isInTrapHoleFallZone = true;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Spike"))
        {
            isInSpikeArea = false;
        }
        else if (collision.CompareTag("TrapHole"))
        {
            isInTrapHoleArea = false;
        }
        else if (collision.CompareTag("TrapHoleFall"))
        {
            isInTrapHoleFallZone = false;
        }
        else if (collision.CompareTag("Table"))
        {
            currentTable = null;
        }
    }

    private void FlipTable()
    {
        if (currentTable != null)
        {
            currentTable.transform.Rotate(0, 0, 90);
            currentTable.AddComponent<BoxCollider2D>();
            Destroy(currentTable, 5f);
        }
    }
}