using UnityEngine;
using System.Collections;

public class Yellowwizard : MonoBehaviour
{
    public float moveSpeed = 3f;         // Speed of the enemy
    public float chaseRange = 5f;        // Distance to start chasing the player
    public float fireRange = 4f;         // Distance to start shooting bullets
    public float attackRange = 1f;       // Distance to start melee attacking the player
    public float fireRate = 1f;          // Time between shots (in seconds)
    public float attackRate = 1f;        // Time between melee attacks (in seconds)
    public BulletPattern bulletPattern = BulletPattern.Straight; // Default bullet pattern

    [SerializeField] private int bulletsAmount = 10;     // Number of bullets in spread (from FireBullet)
    [SerializeField] private float angleSpread = 30f;    // Spread angle for bullets (from FireBullet)

    private Animator animator;           // Reference to the Animator
    private Transform player;            // Reference to the player's transform
    private bool isAttacking = false;    // Track if melee attacking
    private bool isShooting = false;     // Track if shooting bullets
    private float shootTimer = 0f;       // Timer for shooting cooldown
    private float attackTimer = 0f;      // Timer for melee attack cooldown
    private int attackDirection = 0;     // 0 for Left, 1 for Right (for AttackLeft/AttackRight)

    // Enum for bullet patterns
    public enum BulletPattern
    {
        Straight,    // Single bullet straight toward the player
        Spread,      // Spread of bullets (like FireBullet, default 10 bullets in 30-degree spread)
        Burst,       // Three rapid bullets in quick succession
        Spiral       // Bullets spiraling outward
    }

    void Start()
    {
        animator = GetComponent<Animator>();
        player = GameObject.FindGameObjectWithTag("Player").transform; // Find the player

    }

    void Update()
    {
        if (player != null)
        {
            // Calculate direction to player
            Vector2 direction = (player.position - transform.position).normalized;
            float horizontal = direction.x;
            float vertical = direction.y;

            // Check distance to player
            float distanceToPlayer = Vector2.Distance(transform.position, player.position);

            // Determine enemy behavior based on ranges
            if (distanceToPlayer > chaseRange)
            {
                // Player is too far—stay idle
                isAttacking = false;
                isShooting = false;
                ResetTimers();
            }
            else if (distanceToPlayer > fireRange)
            {
                // Player is within chase range but outside fire range—chase the player
                ChasePlayer(direction, distanceToPlayer);
                isAttacking = false;
                isShooting = false;
            }
            else if (distanceToPlayer > attackRange)
            {
                // Player is within fire range but outside attack range—shoot bullets
                ChasePlayer(direction, distanceToPlayer);
                isAttacking = false;
                ShootBullet(direction, distanceToPlayer);
            }
            else
            {
                // Player is within attack range—melee attack
                ChasePlayer(direction, distanceToPlayer);
                MeleeAttack(direction, distanceToPlayer);
            }

            // Update animator parameters (ensure isAttacking is false when out of attack range)
            animator.SetBool("IsMoving", distanceToPlayer <= chaseRange && distanceToPlayer > fireRange);
            animator.SetFloat("Horizontal", horizontal);
            animator.SetFloat("Vertical", vertical);
            animator.SetBool("IsAttacking", isAttacking && distanceToPlayer <= attackRange); // Only attack if in range
            animator.SetInteger("AttackDirection", attackDirection); // Set attack direction for AttackLeft/AttackRight

            // Decrease timers
            UpdateTimers();
        }
    }

    private void ChasePlayer(Vector2 direction, float distance)
    {
        if (distance > attackRange) // Only move if not in attack range
        {
            transform.position = Vector2.MoveTowards(transform.position, player.position, moveSpeed * Time.deltaTime);
        }
    }

    private void ShootBullet(Vector2 direction, float distanceToPlayer)
    {
        if (shootTimer <= 0 && !isAttacking)
        {
            switch (bulletPattern)
            {
                case BulletPattern.Straight:
                    SpawnBullet(direction, 0f); // Single bullet straight toward the player
                    break;

                case BulletPattern.Spread:
                    FireSpread(direction); // Spread of bullets (like FireBullet)
                    break;

                case BulletPattern.Burst:
                    StartCoroutine(BurstFire(direction)); // Rapid succession of three bullets
                    break;

                case BulletPattern.Spiral:
                    StartCoroutine(SpiralFire(direction)); // Simplified spiral pattern
                    break;
            }
            shootTimer = fireRate;  // Reset shoot timer after firing
        }
    }

    private void FireSpread(Vector2 direction)
    {
        // Calculate the direction to the player
        Vector2 directionToPlayer = direction.normalized;
        float angleToPlayer = Mathf.Atan2(directionToPlayer.y, directionToPlayer.x) * Mathf.Rad2Deg;

        // Adjust the start and end angles based on the angle to the player and spread
        float startAngle = angleToPlayer - angleSpread / 2;
        float endAngle = angleToPlayer + angleSpread / 2;

        float angleStep = (endAngle - startAngle) / bulletsAmount;
        float angle = startAngle;

        for (int i = 0; i < bulletsAmount; i++)
        {
            Vector2 bulDir = RotateDirection(directionToPlayer, angle);
            SpawnBullet(bulDir, 0f);
            angle += angleStep;
        }
    }

    private void SpawnBullet(Vector2 baseDirection, float angleOffset)
    {
        GameObject bullet = BulletPool.instance.GetBullet();
        if (bullet != null)
        {
            bullet.transform.position = transform.position;
            Vector2 finalDirection = RotateDirection(baseDirection, angleOffset);
            bullet.GetComponent<Bullet>().SetMoveDirection(finalDirection);
            bullet.SetActive(true);
            isShooting = true;
        }
    }

    private Vector2 RotateDirection(Vector2 direction, float angleDegrees)
    {
        float angleRad = angleDegrees * Mathf.Deg2Rad;
        float cos = Mathf.Cos(angleRad);
        float sin = Mathf.Sin(angleRad);
        return new Vector2(
            direction.x * cos - direction.y * sin,
            direction.x * sin + direction.y * cos
        );
    }

    private System.Collections.IEnumerator BurstFire(Vector2 direction)
    {
        for (int i = 0; i < 3; i++)
        {
            SpawnBullet(direction, 0f); // Straight shots in rapid succession
            yield return new WaitForSeconds(0.1f); // Small delay between shots (0.1 seconds)
        }
    }

    private System.Collections.IEnumerator SpiralFire(Vector2 direction)
    {
        float angle = 0f;
        for (int i = 0; i < 8; i++) // Fire 8 bullets in a spiral
        {
            Vector2 spiralDirection = RotateDirection(direction, angle);
            SpawnBullet(spiralDirection, 0f);
            angle += 45f; // 45-degree increment for a simple spiral
            yield return new WaitForSeconds(0.1f); // Small delay between shots
        }
    }

    private void MeleeAttack(Vector2 direction, float distanceToPlayer)
    {
        if (attackTimer <= 0 && !isShooting && distanceToPlayer <= attackRange)
        {
            isAttacking = true;
            // Determine attack direction based on player position relative to enemy
            attackDirection = (player.position.x > transform.position.x) ? 1 : 0; // Right = 1, Left = 0
            attackTimer = attackRate;  // Reset attack timer
        }
    }

    private void ResetTimers()
    {
        shootTimer = 0f;
        attackTimer = 0f;
    }

    private void UpdateTimers()
    {
        if (shootTimer > 0)
        {
            shootTimer -= Time.deltaTime;
            if (shootTimer <= 0)
            {
                isShooting = false;
            }
        }
        if (attackTimer > 0)
        {
            attackTimer -= Time.deltaTime;
            if (attackTimer <= 0)
            {
                isAttacking = false;
            }
        }
    }

    // Optional: Add a method to handle damage during attacks (triggered via Animation Events)
    //void DealDamage()
    //{
    //    if (player != null)
    //    {
    //        PlayerHealth playerHealth = player.GetComponent<PlayerHealth>();
    //        if (playerHealth != null)
    //        {
    //            playerHealth.TakeDamage(10); // Adjust damage value as needed
    //        }
    //    }
    //}

    // Optional: Method to sync animation duration with cooldown (called via Animation Event if needed)
    void EndAttack()
    {
        isAttacking = false;
    }
}