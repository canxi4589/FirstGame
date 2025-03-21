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
    [SerializeField] private float footstepInterval = 0.4f;
    private float footstepTimer = 0f;

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
    private Vector3 lastSafePosition;
    [SerializeField] private float fallDuration = 0.4f;
    private bool isFalling = false;

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
    [SerializeField] private GameObject panelToToggle; // Reference to the panel to show/hide

    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI timeText;
    [SerializeField] private TextMeshProUGUI moneyText;
    [SerializeField] private TextMeshProUGUI killsText;
    [SerializeField] private Button quickRestartButton;
    [SerializeField] private Button returnToBreachButton;
    [SerializeField] private AudioClip defaultTransitionSound;
    [SerializeField] private AudioClip fallSound;
    [SerializeField] private AudioClip hurtSound;

    private Image[] heartImages;
    private Text ammoText;
    private TextMeshProUGUI coinText;
    private TextMeshProUGUI keyText;

    [SerializeField] private float invincibilityDuration = 1.5f;
    private float invincibilityTime = 0f;
    private bool isInvincible = false;

    private float lastSoundTime = -Mathf.Infinity;
    private const float soundDebounceTime = 0.1f;

    private bool isPanelActive = false; // Track panel state

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        mainCamera = Camera.main;
        currentHealth = maxHealth;

        var playerInput = GetComponent<PlayerInput>();
        if (playerInput != null)
        {
            playerInput.enabled = false;
            playerInput.enabled = true;
            Debug.Log("PlayerInput reinitialized");
        }

        heartImages = heartContainer.GetComponentsInChildren<Image>();
        coinText = coinContainer.transform.Find("CoinText")?.GetComponent<TextMeshProUGUI>();
        keyText = keyContainer.transform.Find("KeyText")?.GetComponent<TextMeshProUGUI>();

        if (heartImages == null || heartImages.Length == 0 || coinText == null || keyText == null || panelToToggle == null)
        {
            Debug.LogError("UI components or panel not found! Check container names, hierarchy, or panel assignment.");
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

        if (quickRestartButton != null)
        {
            quickRestartButton.onClick.AddListener(QuickRestart);
        }
        if (returnToBreachButton != null)
        {
            returnToBreachButton.onClick.AddListener(ReturnToBreach);
        }
        lastSafePosition = transform.position;

        // Ensure panel starts inactive
        if (panelToToggle != null)
        {
            panelToToggle.SetActive(false);
            CanvasGroup panelCanvasGroup = panelToToggle.GetComponent<CanvasGroup>();
            if (panelCanvasGroup == null) panelCanvasGroup = panelToToggle.AddComponent<CanvasGroup>();
            panelCanvasGroup.alpha = 0;
            panelCanvasGroup.interactable = false;
            panelCanvasGroup.blocksRaycasts = false;
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

        if (moveInput.sqrMagnitude > 0.01f && !isDashing)
        {
            footstepTimer -= Time.deltaTime;
            if (footstepTimer <= 0f && Time.time >= lastSoundTime + soundDebounceTime)
            {
                if (SoundManager.Instance != null)
                {
                    SoundManager.Instance.PlayFootstepSound();
                    lastSoundTime = Time.time;
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

    public void Dash(InputAction.CallbackContext context)
    {
        if (context.started && !isDashing && Time.time >= lastDashTime + dashCooldown)
        {
            isDashing = true;
            lastDashTime = Time.time;
            dashEndTime = Time.time + dashDuration;
            dashIFrameEndTime = Time.time + dashIFrameDuration;

            dashDirection = moveInput.sqrMagnitude > 0 ? moveInput.normalized : new Vector2(lastInputX, lastInputY);
            rb.velocity = dashDirection * moveSpeed * dashSpeedMultiplier;
            if (SoundManager.Instance != null && Time.time >= lastSoundTime + soundDebounceTime)
            {
                SoundManager.Instance.PlayDashSound();
                lastSoundTime = Time.time;
            }
            Invoke(nameof(EndDash), dashDuration);
        }
    }

    void EndDash()
    {
        isDashing = false;
        rb.velocity = moveInput.sqrMagnitude > 0 ? moveInput * moveSpeed : Vector2.zero;
    }

    public void TogglePanel(InputAction.CallbackContext context)
    {
        if (context.started && panelToToggle != null)
        {
            isPanelActive = !isPanelActive;
            CanvasGroup panelCanvasGroup = panelToToggle.GetComponent<CanvasGroup>();
            if (panelCanvasGroup == null) panelCanvasGroup = panelToToggle.AddComponent<CanvasGroup>();

            if (SoundManager.Instance != null && Time.time >= lastSoundTime + soundDebounceTime)
            {
                SoundManager.Instance.PlaySelectMenuSound();
                lastSoundTime = Time.time;
            }

            StartCoroutine(TogglePanelAnimation(panelCanvasGroup, isPanelActive));
        }
    }

    private System.Collections.IEnumerator TogglePanelAnimation(CanvasGroup canvasGroup, bool activate)
    {
        float duration = 0.5f;
        float elapsed = 0f;

        if (activate)
        {
            panelToToggle.SetActive(true);
            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                canvasGroup.alpha = Mathf.Lerp(0, 1, elapsed / duration);
                canvasGroup.interactable = true;
                canvasGroup.blocksRaycasts = true;
                yield return null;
            }
            canvasGroup.alpha = 1;
        }
        else
        {
            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                canvasGroup.alpha = Mathf.Lerp(1, 0, elapsed / duration);
                canvasGroup.interactable = false;
                canvasGroup.blocksRaycasts = false;
                yield return null;
            }
            canvasGroup.alpha = 0;
            panelToToggle.SetActive(false);
        }
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
            if (SoundManager.Instance != null && Time.time >= lastSoundTime + soundDebounceTime)
            {
                SoundManager.Instance.PlayCoinCollectSound();
                lastSoundTime = Time.time;
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

            if (SoundManager.Instance != null && Time.time >= lastSoundTime + soundDebounceTime)
            {
                SoundManager.Instance.PlayHurtSound();
                lastSoundTime = Time.time;
            }

            if (currentHealth == 0)
            {
                StartCoroutine(EndGameSequence(false));
            }
        }
    }

    public void TriggerVictory()
    {
        StartCoroutine(EndGameSequence(true));
    }

    private System.Collections.IEnumerator EndGameSequence(bool isVictory)
    {
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.StopAllEffects();
        }
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

    private void QuickRestart()
    {
        Time.timeScale = 1f;
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.StopAllEffects();
        }
        if (GameManager.Instance != null)
        {
            GameManager.Instance.ResetStats();
        }
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    private void ReturnToBreach()
    {
        Time.timeScale = 1f;
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.StopAllEffects();
        }
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

    public int GetAmmo() => ammo;
    public int GetHealth() => currentHealth;

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

    public AudioClip GetDefaultTransitionSound() => defaultTransitionSound;

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
        rb.velocity = Vector2.zero;
        animator.SetTrigger("Fall");

        if (SoundManager.Instance != null && Time.time >= lastSoundTime + soundDebounceTime)
        {
            SoundManager.Instance.PlayFallSound();
            lastSoundTime = Time.time;
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