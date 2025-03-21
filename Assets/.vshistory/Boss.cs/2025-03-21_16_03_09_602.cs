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

    // Movement variables
    private Vector2 targetPosition;
    private float moveSpeed = 2f;

    // Audio/Visuals
    public ParticleSystem phaseChangeEffect;
    public AudioSource attackSound;

    private bool canFight = false;

    void Start()
    {
        currentHealth = maxHealth;
        healthBar.maxValue = maxHealth;
        healthBar.value = currentHealth;
        targetPosition = transform.position;
    }

    void Update()
    {
        if (!canFight) return;

        healthBar.value = currentHealth;
        transform.position = Vector2.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);

        if (currentHealth <= maxHealth * phase3Threshold && currentPhase < 3)
        {
            TransitionToPhase(3);
        }
        else if (currentHealth <= maxHealth * phase2Threshold && currentPhase < 2)
        {
            TransitionToPhase(2);
        }

        if (Time.time >= nextFireTime)
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
        phaseChangeEffect.Play();
        switch (phase)
        {
            case 2:
                fireRate = 0.35f;
                moveSpeed = 3f;
                StartPhase2();
                break;
            case 3:
                fireRate = 0.25f;
                moveSpeed = 4f;
                StartPhase3();
                break;
        }
        Debug.Log($"Phase {phase} started!");
    }

    void FirePattern()
    {
        attackSound.Play();
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

    // Phase 1: Radial Burst with Clustering (from the first image)
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
            SpawnBullet(spawnPos, direction, 4f);
        }

        if (Random.value > 0.8f)
        {
            targetPosition = new Vector2(Random.Range(-5f, 5f), Random.Range(2f, 5f));
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
            SpawnBullet(transform.position, direction, 5f);
        }

        // Targeted shots at the player
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            Vector2 playerPos = player.transform.position;
            Vector2 directionToPlayer = (playerPos - (Vector2)transform.position).normalized;
            SpawnBullet(transform.position, directionToPlayer, 6f);
        }

        targetPosition = new Vector2(Mathf.Sin(Time.time * 2f) * 4f, transform.position.y);
    }

    // Phase 3: Dense Waves + Rotating Arcs
    void Phase3Pattern()
    {
        // Dense waves (inspired by the crisscross pattern)
        for (float x = -6f; x <= 6f; x += 0.8f)
        {
            Vector2 pos = new Vector2(transform.position.x + x, transform.position.y);
            Vector2 dir1 = new Vector2(0.5f, -1f).normalized;
            Vector2 dir2 = new Vector2(-0.5f, -1f).normalized;
            SpawnBullet(pos, dir1, 6f);
            SpawnBullet(pos, dir2, 6f);
        }

        // Rotating arcs
        float arcAngle = Time.time * 100f;
        for (int i = 0; i < 3; i++) // 3 arcs
        {
            float startAngle = arcAngle + (i * 120f); // 120 degrees apart
            for (int j = -2; j <= 2; j++) // 5 bullets per arc
            {
                float currentAngle = startAngle + (j * 10f); // 10-degree spread
                Vector2 direction = new Vector2(Mathf.Cos(currentAngle * Mathf.Deg2Rad), Mathf.Sin(currentAngle * Mathf.Deg2Rad));
                SpawnBullet(transform.position, direction, 5f);
            }
        }

        targetPosition = new Vector2(Mathf.PingPong(Time.time * moveSpeed, 6f) - 3f, 4f);
    }

    void SpawnBullet(Vector2 position, Vector2 direction, float speed)
    {
        GameObject bullet = Instantiate(bulletPrefab, position, Quaternion.identity);
        Rigidbody2D rb = bullet.GetComponent<Rigidbody2D>();
        direction = (direction + new Vector2(0, -1)).normalized; // Bias downward
        rb.velocity = direction * speed;
    }

    void StartPhase2() { }
    void StartPhase3() { }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("PlayerBullet"))
        {
            TakeDamage(10f);
            Destroy(collision.gameObject);
        }
    }
}