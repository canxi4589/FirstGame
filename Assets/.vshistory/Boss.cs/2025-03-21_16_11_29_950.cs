using UnityEngine;

public class Boss : MonoBehaviour
{
    // Health and UI
    public float maxHealth = 1500f;
    private float currentHealth;
    public UnityEngine.UI.Slider healthBar;

    // Phase tracking
    private int currentPhase = 1;
    private float phase2Threshold = 0.66f;
    private float phase3Threshold = 0.33f;

    // Bullet pattern variables
    public GameObject bulletPrefab;
    private float fireRate = 0.5f;
    private float nextFireTime;
    private GameObject player; // Cached player reference

    // Movement variables
    private Vector2 targetPosition;
    private float moveSpeed = 2f;
    private float minX = -4f, maxX = 4f, minY = 2f, maxY = 4f; // Movement boundaries

    // Audio/Visuals
    public ParticleSystem phaseChangeEffect;
    public AudioSource attackSound;

    // Detection range
    public float detectionRange = 10f; // Range within which the boss can detect the player

    private bool canFight = false;

    void Start()
    {
        currentHealth = maxHealth;
        if (healthBar != null)
        {
            healthBar.maxValue = maxHealth;
            healthBar.value = currentHealth;
        }
        else
        {
            Debug.LogWarning("Health bar not assigned in Inspector!");
        }

        targetPosition = transform.position;

        // Cache the player reference
        player = GameObject.FindGameObjectWithTag("Player");
        if (player == null)
        {
            Debug.LogWarning("Player not found! Ensure the player GameObject is tagged as 'Player'.");
        }
    }

    void Update()
    {
        if (!canFight) return;

        if (healthBar != null)
        {
            healthBar.value = currentHealth;
        }

        // Clamp the target position to keep the boss on-screen
        targetPosition.x = Mathf.Clamp(targetPosition.x, minX, maxX);
        targetPosition.y = Mathf.Clamp(targetPosition.y, minY, maxY);
        transform.position = Vector2.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);

        if (currentHealth <= maxHealth * phase3Threshold && currentPhase < 3)
        {
            TransitionToPhase(3);
        }
        else if (currentHealth <= maxHealth * phase2Threshold && currentPhase < 2)
        {
            TransitionToPhase(2);
        }

        // Only fire if the player is within detection range
        if (Time.time >= nextFireTime && IsPlayerInRange())
        {
            FirePattern();
            nextFireTime = Time.time + fireRate;
        }
    }

    public void StartFight()
    {
        canFight = true;
        Debug.Log("Boss fight started!");
    }

    void TakeDamage(float damage, bool isWeakPoint = false)
    {
        float actualDamage = isWeakPoint ? damage * 2 : damage;
        currentHealth -= actualDamage;
        if (currentHealth <= 0)
        {
            Destroy(gameObject);
        }
    }

    void TransitionToPhase(int phase)
    {
        currentPhase = phase;
        if (phaseChangeEffect != null)
        {
            phaseChangeEffect.Play();
        }
        else
        {
            Debug.LogWarning("Phase change effect not assigned in Inspector!");
        }

        switch (phase)
        {
            case 2:
                fireRate = 0.35f;
                moveSpeed = 3f;
                StartPhase2();
                break;
            case 3:
                fireRate = Mathf.Max(0.2f, 0.25f); // Cap the fire rate to avoid excessive spawning
                moveSpeed = 4f;
                StartPhase3();
                break;
        }
        Debug.Log($"Phase {phase} started!");
    }

    void FirePattern()
    {
        if (attackSound != null)
        {
            attackSound.Play();
        }
        else
        {
            Debug.LogWarning("Attack sound not assigned in Inspector!");
        }

        switch (currentPhase)
        {
            case 1:
                Phase1Pattern();
                break;
            case 2:
                Phase2Pattern();
                break;
            case 3:
                Phase3Pattern();
                break;
        }
    }

    // Phase 1: Radial Burst with Clustering
    void Phase1Pattern()
    {
        int bulletCount = 20;
        float angleStep = 360f / bulletCount;
        for (int i = 0; i < bulletCount; i++)
        {
            float angle = i * angleStep + Random.Range(-10f, 10f);
            Vector2 direction = new Vector2(Mathf.Cos(angle * Mathf.Deg2Rad), Mathf.Sin(angle * Mathf.Deg2Rad));
            Vector2 spawnOffset = new Vector2(Random.Range(-0.5f, 0.5f), Random.Range(-0.5f, 0.5f));
            Vector2 spawnPos = (Vector2)transform.position + spawnOffset;
            SpawnBullet(spawnPos, direction, 4f, true); // Bias downward
        }

        if (Random.value > 0.8f)
        {
            targetPosition = new Vector2(Random.Range(minX, maxX), Random.Range(minY, maxY));
        }
    }

    // Phase 2: Spiral + Targeted Shots
    void Phase2Pattern()
    {
        // Spiral pattern
        float angle = Time.time * 150f;
        for (int i = 0; i < 16; i++)
        {
            float currentAngle = angle + (i * 22.5f);
            Vector2 direction = new Vector2(Mathf.Cos(currentAngle * Mathf.Deg2Rad), Mathf.Sin(currentAngle * Mathf.Deg2Rad));
            SpawnBullet(transform.position, direction, 5f, false); // No downward bias for spiral
        }

        // Targeted shots at the player
        if (player != null)
        {
            Vector2 playerPos = player.transform.position;
            Vector2 directionToPlayer = (playerPos - (Vector2)transform.position).normalized;
            SpawnBullet(transform.position, directionToPlayer, 6f, true); // Bias downward
        }

        targetPosition = new Vector2(Mathf.Sin(Time.time * 2f) * (maxX - minX) / 2f, transform.position.y);
    }

    // Phase 3: Dense Waves + Rotating Arcs
    void Phase3Pattern()
    {
        // Dense waves
        for (float x = -6f; x <= 6f; x += 0.8f)
        {
            Vector2 pos = new Vector2(transform.position.x + x, transform.position.y);
            Vector2 dir1 = new Vector2(0.5f, -1f).normalized;
            Vector2 dir2 = new Vector2(-0.5f, -1f).normalized;
            SpawnBullet(pos, dir1, 6f, false);
            SpawnBullet(pos, dir2, 6f, false);
        }

        // Rotating arcs
        float arcAngle = Time.time * 100f;
        for (int i = 0; i < 3; i++)
        {
            float startAngle = arcAngle + (i * 120f);
            for (int j = -2; j <= 2; j++)
            {
                float currentAngle = startAngle + (j * 10f);
                Vector2 direction = new Vector2(Mathf.Cos(currentAngle * Mathf.Deg2Rad), Mathf.Sin(currentAngle * Mathf.Deg2Rad));
                SpawnBullet(transform.position, direction, 5f, false);
            }
        }

        targetPosition = new Vector2(Mathf.PingPong(Time.time * moveSpeed, maxX - minX) - (maxX - minX) / 2f, 4f);
    }

    void SpawnBullet(Vector2 position, Vector2 direction, float speed, bool biasDownward = false)
    {
        GameObject bullet = Instantiate(bulletPrefab, position, Quaternion.identity);
        Rigidbody2D rb = bullet.GetComponent<Rigidbody2D>();
        if (biasDownward)
        {
            direction = (direction + new Vector2(0, -1)).normalized;
        }
        rb.velocity = direction * speed;
    }

    bool IsPlayerInRange()
    {
        if (player == null) return false;

        float distanceToPlayer = Vector2.Distance(transform.position, player.transform.position);
        return distanceToPlayer <= detectionRange;
    }

    void StartPhase2() { }
    void StartPhase3() { }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("PlayerProjectile" +
            ""))
        {
            TakeDamage(10f);
            Destroy(collision.gameObject);
        }
    }

    // Optional: Visualize the detection range in the editor
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
    }
}