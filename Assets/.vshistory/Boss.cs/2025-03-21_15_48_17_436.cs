using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Playables;

public class Boss : MonoBehaviour
{
    [Header("Boss Stats")]
    [SerializeField] private string bossName = "Dark Overlord"; // Name of the boss
    [SerializeField] private int maxHealth = 100;
    [SerializeField] private float phase2Threshold = 0.66f;
    [SerializeField] private float phase3Threshold = 0.33f;

    [Header("Difficulty Scaling")]
    [SerializeField] private float healthPerKey = 10f; // Additional health per key the player has
    [SerializeField] private float speedPerKey = 0.2f; // Additional speed per key the player has

    [Header("Movement and Attack Settings")]
    [SerializeField] private float chargeSpeed = 5f;
    [SerializeField] private float chargeCooldown = 3f;
    [SerializeField] private float projectileSpeed = 5f;
    [SerializeField] private float projectileCooldown = 2f;
    [SerializeField] private float phase3SpeedMultiplier = 1.5f;
    [SerializeField] private GameObject shockwavePrefab; // Prefab for the shockwave (ground slam)

    [Header("Rage Mode (Phase 3)")]
    [SerializeField] private float rageHealthThreshold = 0.2f; // Health percentage to enter rage mode
    [SerializeField] private float rageSpeedMultiplier = 1.5f; // Speed multiplier in rage mode
    [SerializeField] private float rageCooldownReduction = 0.5f; // Reduce attack cooldowns in rage mode
    [SerializeField] private ParticleSystem rageEffect; // Visual effect for rage mode

    [Header("Visual and Audio Feedback")]
    [SerializeField] private AudioClip chargeSound;
    [SerializeField] private AudioClip shootSound;
    [SerializeField] private AudioClip slamSound;
    [SerializeField] private ParticleSystem chargeEffect;
    [SerializeField] private ParticleSystem shootEffect;
    [SerializeField] private ParticleSystem slamEffect;

    [Header("Health Pickups")]
    [SerializeField] private GameObject healthPickupPrefab; // Prefab for the health pickup

    [Header("Environmental Hazards")]
    [SerializeField] private GameObject[] firePits; // Fire pits to activate in Phase 2

    [Header("Atmosphere")]
    [SerializeField] private AudioClip phase1Music;
    [SerializeField] private AudioClip phase2Music;
    [SerializeField] private AudioClip phase3Music;
    [SerializeField] private AudioClip rageMusic;
    [SerializeField] private UnityEngine.Rendering.Universal.Light2D globalLight;
    [SerializeField] private Color phase1LightColor = Color.white;
    [SerializeField] private Color phase2LightColor = new Color(1f, 0.8f, 0.8f);
    [SerializeField] private Color phase3LightColor = new Color(1f, 0.5f, 0.5f);
    [SerializeField] private Color rageLightColor = new Color(1f, 0.2f, 0.2f);

    [Header("References")]
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private Transform projectileSpawnPoint;
    [SerializeField] private Sprite bossDialogSprite;
    [SerializeField] private Sprite player1;


    [Header("Health Bar")]
    [SerializeField] private GameObject healthBarPanel;
    [SerializeField] private UnityEngine.UI.Slider healthBarSlider;
    [SerializeField] private TMPro.TextMeshProUGUI healthBarLabel;
    [SerializeField] private UnityEngine.UI.Image healthBarFill;

    [Header("Boss Area")]
    [SerializeField] private GameObject[] barriers; // Barriers to lock the player in the boss area

    [Header("Reward")]
    [SerializeField] private GameObject keyPrefab; // Key to drop when the boss is defeated
    [SerializeField] private ParticleSystem defeatEffect; // Particle effect for boss defeat

    [Header("Game Progression")]
    [SerializeField] private GameObject progressionBarrier; // Barrier to unlock after defeating the boss

    [Header("Cutscene")]
    [SerializeField] private Transform cutsceneFocusPoint; // Point to focus the camera on during the cutscene

    private int currentHealth;
    private int currentPhase = 1;
    private Transform player;
    private bool isCharging = false;
    private bool canAttack = true;
    private bool isInvincible = false;
    private bool hasFightStarted = false;
    private SpriteRenderer spriteRenderer;
    private Color originalColor;
    private bool isVulnerable = false;
    private bool isInRageMode = false;
    private bool droppedHealthAt50 = false;
    private bool droppedHealthAt25 = false;
    private float targetHealthPercentage; // For smooth health bar transition
    private bool isUpdatingHealthBar = false;

    private void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
        if (player == null)
        {
            Debug.LogWarning("Player not found for Boss! Disabling boss.");
            gameObject.SetActive(false);
            return;
        }

        // Scale difficulty based on player's key count
        PlayerMovement playerMovement = player.GetComponent<PlayerMovement>();
        if (playerMovement != null)
        {
            int playerKeys =0;
            maxHealth += Mathf.RoundToInt(healthPerKey * playerKeys);
            chargeSpeed += speedPerKey * playerKeys;
            projectileSpeed += speedPerKey * playerKeys;
        }

        currentHealth = maxHealth;
        targetHealthPercentage = 1f;
        UpdateHealthBar();

        spriteRenderer = GetComponent<SpriteRenderer>();
        originalColor = spriteRenderer.color;

        if (healthBarPanel != null)
        {
            healthBarPanel.SetActive(false);
        }
    }

    private void Update()
    {
        if (player == null || !hasFightStarted) return;

        switch (currentPhase)
        {
            case 1:
                Phase1Behavior();
                break;
            case 2:
                Phase2Behavior();
                break;
            case 3:
                Phase3Behavior();
                break;
        }
    }

    private void Phase1Behavior()
    {
        if (canAttack && !isCharging)
        {
            StartCoroutine(ChargeAtPlayer());
        }
    }

    private void Phase2Behavior()
    {
        if (canAttack)
        {
            StartCoroutine(ShootProjectile());
        }
    }

    private void Phase3Behavior()
    {
        if (canAttack)
        {
            float random = Random.value;
            if (random < 0.33f)
            {
                StartCoroutine(ChargeAtPlayer(phase3SpeedMultiplier * (isInRageMode ? rageSpeedMultiplier : 1f)));
            }
            else if (random < 0.66f)
            {
                StartCoroutine(ShootProjectile());
            }
            else
            {
                StartCoroutine(GroundSlam());
            }
        }
    }

    public void StartFight()
    {
        if (!hasFightStarted)
        {
            hasFightStarted = true;
            foreach (GameObject barrier in barriers)
            {
                if (barrier != null)
                {
                    barrier.SetActive(true);
                }
            }
            StartCoroutine(StartBossFight());
        }
    }

    private IEnumerator StartBossFight()
    {
        spriteRenderer.enabled = false;
        if (DialogManager.Instance != null)
        {
            List<DialogManager.DialogLine> dialogLines = new List<DialogManager.DialogLine>
            {
                new DialogManager.DialogLine { message = "Well, well, well... You dare to face me?", speakerSprite = bossDialogSprite },
                new DialogManager.DialogLine { message = "Prepare to meet your doom!", speakerSprite = bossDialogSprite }
            };
            DialogManager.Instance.ShowDialogSequence(dialogLines, true);
        }

        while (DialogManager.Instance != null && DialogManager.Instance.IsDialogActive())
        {
            yield return null;
        }

        spriteRenderer.enabled = true;
        Color startColor = new Color(1f, 1f, 1f, 0f);
        Color endColor = Color.white;
        float duration = 1f;
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            spriteRenderer.color = Color.Lerp(startColor, endColor, t);
            yield return null;
        }

        if (SoundManager.Instance != null && phase1Music != null)
        {
        }
        if (globalLight != null)
        {
            globalLight.color = phase1LightColor;
        }

        if (healthBarPanel != null)
        {
            healthBarPanel.SetActive(true);
        }
    }

    private IEnumerator ChargeAtPlayer(float speedMultiplier = 1f)
    {
        canAttack = false;
        isCharging = true;
        isVulnerable = true;

        Vector3 originalScale = transform.localScale;
        Color originalColor = spriteRenderer.color;
        spriteRenderer.color = Color.red;
        yield return StartCoroutine(ScaleAnimation(originalScale, originalScale * 1.2f, 0.5f));

        if (SoundManager.Instance != null && chargeSound != null)
        {
            SoundManager.Instance.PlaySound(chargeSound, 1f);
        }
        if (chargeEffect != null)
        {
            chargeEffect.Play();
        }

        Vector2 direction = (player.position - transform.position).normalized;
        float chargeDuration = 1f;
        float elapsed = 0f;

        while (elapsed < chargeDuration)
        {
            transform.position += (Vector3)direction * chargeSpeed * speedMultiplier * Time.deltaTime;
            elapsed += Time.deltaTime;
            yield return null;
        }

        yield return StartCoroutine(ScaleAnimation(transform.localScale, originalScale, 0.3f));
        spriteRenderer.color = isInRageMode ? Color.red : originalColor;

        isCharging = false;
        isVulnerable = false;
        yield return new WaitForSeconds(chargeCooldown / (isInRageMode ? rageSpeedMultiplier : 1f));
        canAttack = true;
    }

    private IEnumerator ShootProjectile()
    {
        canAttack = false;

        if (spriteRenderer != null)
        {
            spriteRenderer.color = Color.yellow;
            yield return new WaitForSeconds(0.3f);
            spriteRenderer.color = isInRageMode ? Color.red : Color.white;
        }

        if (SoundManager.Instance != null && shootSound != null)
        {
            SoundManager.Instance.PlaySound(shootSound, 1f);
        }
        if (shootEffect != null)
        {
            shootEffect.Play();
        }

        if (projectilePrefab != null && projectileSpawnPoint != null)
        {
            GameObject projectile = Instantiate(projectilePrefab, projectileSpawnPoint.position, Quaternion.identity);
            Vector2 direction = (player.position - projectileSpawnPoint.position).normalized;
            Rigidbody2D rb = projectile.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.velocity = direction * projectileSpeed * (isInRageMode ? rageSpeedMultiplier : 1f);
            }
        }

        yield return new WaitForSeconds(projectileCooldown / (isInRageMode ? rageSpeedMultiplier : 1f));
        canAttack = true;
    }

    private IEnumerator GroundSlam()
    {
        canAttack = false;

        GameObject telegraph = new GameObject("SlamTelegraph");
        telegraph.transform.position = transform.position;
        SpriteRenderer telegraphRenderer = telegraph.AddComponent<SpriteRenderer>();
        telegraphRenderer.sprite = shockwavePrefab.GetComponent<SpriteRenderer>().sprite;
        telegraphRenderer.color = new Color(1f, 0f, 0f, 0.5f);
        float telegraphDuration = 1f;
        float elapsed = 0f;
        while (elapsed < telegraphDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / telegraphDuration;
            telegraphRenderer.transform.localScale = Vector3.Lerp(Vector3.one, Vector3.one * 5f, t);
            yield return null;
        }
        Destroy(telegraph);

        Vector3 originalScale = transform.localScale;
        yield return StartCoroutine(ScaleAnimation(originalScale, originalScale * 1.5f, 0.5f));

        if (SoundManager.Instance != null && slamSound != null)
        {
            SoundManager.Instance.PlaySound(slamSound, 1f);
        }
        if (slamEffect != null)
        {
            slamEffect.Play();
        }

        if (shockwavePrefab != null)
        {
            Instantiate(shockwavePrefab, transform.position, Quaternion.identity);
        }

        yield return StartCoroutine(ScaleAnimation(transform.localScale, originalScale, 0.3f));

        yield return new WaitForSeconds(chargeCooldown / (isInRageMode ? rageSpeedMultiplier : 1f));
        canAttack = true;
    }

    private IEnumerator ScaleAnimation(Vector3 startScale, Vector3 endScale, float duration)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            transform.localScale = Vector3.Lerp(startScale, endScale, t);
            yield return null;
        }
        transform.localScale = endScale;
    }

    public void TakeDamage(int damage)
    {
        if (isInvincible) return;

        int actualDamage = isVulnerable ? damage * 2 : damage;
        currentHealth -= actualDamage;
        currentHealth = Mathf.Max(0, currentHealth);
        UpdateHealthBar();

        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlayHurtSound();
        }

        float healthPercentage = (float)currentHealth / maxHealth;
        if (!droppedHealthAt50 && healthPercentage <= 0.5f)
        {
            droppedHealthAt50 = true;
            DropHealthPickup();
        }
        else if (!droppedHealthAt25 && healthPercentage <= 0.25f)
        {
            droppedHealthAt25 = true;
            DropHealthPickup();
        }

        if (currentPhase == 1 && healthPercentage <= phase2Threshold)
        {
            StartCoroutine(TransitionToPhase(2));
        }
        else if (currentPhase == 2 && healthPercentage <= phase3Threshold)
        {
            StartCoroutine(TransitionToPhase(3));
        }
        else if (currentPhase == 3 && !isInRageMode && healthPercentage <= rageHealthThreshold)
        {
            StartCoroutine(EnterRageMode());
        }

        if (currentHealth <= 0)
        {
            StartCoroutine(DefeatBoss());
        }
    }

    private void DropHealthPickup()
    {
        if (healthPickupPrefab != null)
        {
            Instantiate(healthPickupPrefab, transform.position, Quaternion.identity);
        }
    }

    private void UpdateHealthBar()
    {
        targetHealthPercentage = (float)currentHealth / maxHealth;
        if (!isUpdatingHealthBar)
        {
            StartCoroutine(SmoothHealthBarUpdate());
        }

        if (healthBarFill != null)
        {
            float healthPercentage = (float)currentHealth / maxHealth;
            healthBarFill.color = Color.Lerp(Color.red, Color.green, healthPercentage);
        }

        if (healthBarLabel != null)
        {
            healthBarLabel.text = $"{bossName}: {currentHealth}/{maxHealth}";
        }
    }

    private IEnumerator SmoothHealthBarUpdate()
    {
        isUpdatingHealthBar = true;
        float currentValue = healthBarSlider.value;
        float duration = 0.5f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            healthBarSlider.value = Mathf.Lerp(currentValue, targetHealthPercentage, t);
            yield return null;
        }

        healthBarSlider.value = targetHealthPercentage;
        isUpdatingHealthBar = false;
    }

    private IEnumerator TransitionToPhase(int newPhase)
    {
        isInvincible = true;
        canAttack = false;

        if (healthBarFill != null)
        {
            StartCoroutine(FlashHealthBar());
        }

        string dialogMessage = newPhase == 2 ? "You think you can defeat me? Try this!" : "Now you’ve made me angry!";
        if (DialogManager.Instance != null)
        {
            DialogManager.Instance.ShowDialog(dialogMessage, bossDialogSprite, 2f);
        }

        while (DialogManager.Instance != null && DialogManager.Instance.IsDialogActive())
        {
            yield return null;
        }

        if (newPhase == 2)
        {
            foreach (GameObject firePit in firePits)
            {
                if (firePit != null)
                {
                    firePit.SetActive(true);
                }
            }
        }

        if (SoundManager.Instance != null)
        {
            if (newPhase == 2 && phase2Music != null)
            {
                //SoundManager.Instance.PlayMusic(phase2Music);
            }
            else if (newPhase == 3 && phase3Music != null)
            {
                //SoundManager.Instance.PlayMusic(phase3Music);
            }
        }
        if (globalLight != null)
        {
            globalLight.color = newPhase == 2 ? phase2LightColor : phase3LightColor;
        }

        currentPhase = newPhase;
        isInvincible = false;
        canAttack = true;
    }

    private IEnumerator EnterRageMode()
    {
        isInRageMode = true;
        isInvincible = true;
        canAttack = false;

        spriteRenderer.color = Color.red;
        if (rageEffect != null)
        {
            rageEffect.Play();
        }

        if (DialogManager.Instance != null)
        {
            DialogManager.Instance.ShowDialog("You’ll pay for this! RAAAH!", bossDialogSprite, 2f);
        }

        while (DialogManager.Instance != null && DialogManager.Instance.IsDialogActive())
        {
            yield return null;
        }

        chargeCooldown *= rageCooldownReduction;
        projectileCooldown *= rageCooldownReduction;

        if (SoundManager.Instance != null && rageMusic != null)
        {
            //SoundManager.Instance.PlayMusic(rageMusic);
        }
        if (globalLight != null)
        {
            globalLight.color = rageLightColor;
        }

        isInvincible = false;
        canAttack = true;
    }

    private IEnumerator FlashHealthBar()
    {
        float flashDuration = 0.1f;
        int flashCount = 3;
        Color originalColor = healthBarFill.color;

        for (int i = 0; i < flashCount; i++)
        {
            healthBarFill.color = Color.white;
            yield return new WaitForSeconds(flashDuration);
            healthBarFill.color = originalColor;
            yield return new WaitForSeconds(flashDuration);
        }
    }

    private IEnumerator DefeatBoss()
    {
        spriteRenderer.color = originalColor;

        if (DialogManager.Instance != null)
        {
            DialogManager.Instance.ShowDialog("Nooo! How could I be defeated?!", bossDialogSprite, 3f);
        }

        while (DialogManager.Instance != null && DialogManager.Instance.IsDialogActive())
        {
            yield return null;
        }

        PlayerMovement playerMovement = player.GetComponent<PlayerMovement>();
        if (playerMovement != null && DialogManager.Instance != null)
        {
            Sprite playerDialogSprite = player1;
            DialogManager.Instance.ShowDialog("I did it! The Dark Overlord is defeated!", playerDialogSprite, 3f);
        }

        while (DialogManager.Instance != null && DialogManager.Instance.IsDialogActive())
        {
            yield return null;
        }

        if (defeatEffect != null)
        {
            defeatEffect.Play();
            yield return new WaitForSeconds(defeatEffect.main.duration);
        }

        if (keyPrefab != null)
        {
            Instantiate(keyPrefab, transform.position, Quaternion.identity);
        }

        foreach (GameObject barrier in barriers)
        {
            if (barrier != null)
            {
                barrier.SetActive(false);
            }
        }

        foreach (GameObject firePit in firePits)
        {
            if (firePit != null)
            {
                firePit.SetActive(false);
            }
        }

        if (progressionBarrier != null)
        {
            progressionBarrier.SetActive(false);
        }

        if (healthBarPanel != null)
        {
            healthBarPanel.SetActive(false);
        }
        Destroy(gameObject);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            PlayerMovement playerMovement = collision.gameObject.GetComponent<PlayerMovement>();
            if (playerMovement != null)
            {
                playerMovement.TakeDamage(1);
            }
        }
    }
}