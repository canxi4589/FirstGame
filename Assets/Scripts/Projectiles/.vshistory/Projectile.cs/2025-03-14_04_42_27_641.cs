using UnityEngine;

public class Projectile : MonoBehaviour
{
    public float lifetime = 30f;
    public float projectileSpeed = 5f; // Match the speed from WeaponHolder
    private Animator animator;
    private bool hasHit = false;
    private Collider2D col;
    private BulletPool1 bulletPool;
    private Vector2 direction;
    private Vector2 startPosition;

    void Awake()
    {
        animator = GetComponent<Animator>();
        col = GetComponent<Collider2D>();
        // Remove Rigidbody2D dependency since we’ll handle movement manually
    }

    void OnEnable()
    {
        hasHit = false;
        col.enabled = true;
        if (animator != null)
        {
            animator.Play("FireBullet");
        }
        startPosition = transform.position; // Record starting position
        Invoke(nameof(DisableProjectile), lifetime);
        Debug.Log($"Bullet enabled at position: {transform.position}");
    }

    public void SetPool(BulletPool1 pool)
    {
        bulletPool = pool;
    }

    // Called by WeaponHolder to set initial direction
    public void SetDirection(Vector2 dir)
    {
        direction = dir.normalized;
    }

    void Update()
    {
        if (!hasHit)
        {
            // Move the bullet manually using unscaled time
            float moveDistance = projectileSpeed * Time.unscaledDeltaTime;
            transform.position += (Vector3)(direction * moveDistance);

            // Check range
            if (Vector2.Distance(startPosition, transform.position) > 10f) // Match enemy’s fireRange
            {
                DisableProjectile();
                return;
            }

            // Raycast to detect collisions
            RaycastHit2D hit = Physics2D.Raycast(transform.position, direction, moveDistance, LayerMask.GetMask("Default"));
            if (hit.collider != null)
            {
                Debug.Log($"Raycast hit: {hit.collider.gameObject.name}, Tag: {hit.collider.gameObject.tag}, Position: {transform.position}");
                OnTriggerEnter2D(hit.collider); // Manually trigger collision
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (hasHit) return;
        hasHit = true;

        Debug.Log($"Trigger entered with: {collision.gameObject.name}, Tag: {collision.gameObject.tag}, Layer: {LayerMask.LayerToName(collision.gameObject.layer)}, Position: {transform.position}");
        if (collision.CompareTag("Enemy") || collision.CompareTag("Decor"))
        {
            animator.SetTrigger("Explode");
            col.enabled = false;
            Invoke(nameof(DisableProjectile), 1f);
        }

        if (collision.CompareTag("PlayerProjectile") || collision.CompareTag("EnemyProjectile") || collision.CompareTag("Bound") || collision.CompareTag("Player"))
            return;

        if (animator != null)
        {
            animator.SetTrigger("Explode");
        }

        col.enabled = false;
        Invoke(nameof(DisableProjectile), 1f);
    }

    void DisableProjectile()
    {
        Debug.Log($"Disabling bullet at position: {transform.position}, Active: {gameObject.activeSelf}");
        gameObject.SetActive(false);
    }
}