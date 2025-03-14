using UnityEngine;

public class Projectile : MonoBehaviour
{
    public float lifetime = 5f;
    private Animator animator;
    private bool hasHit = false; // Prevent multiple triggers
    private Rigidbody2D rb;
    private Collider2D col;
    private BulletPool1 bulletPool; // Reference to the pool

    void Awake()
    {
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<Collider2D>();
    }

    void OnEnable()
    {
        hasHit = false; // Reset hit state
        col.enabled = true; // Enable collider
        if (animator != null)
        {
            animator.Play("FireBullet"); // Start flying animation
        }

        // Automatically return to pool after `lifetime`
        Invoke(nameof(DisableProjectile), lifetime);
    }
    public void SetPool(BulletPool1 pool)
    {
        bulletPool = pool;
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (hasHit) return;
        hasHit = true;

        // Ignore other bullets, player, and tilemap
        if (collision.CompareTag("PlayerProjectile") || collision.CompareTag("Player") || collision.CompareTag("TileMap") || collision.CompareTag("EnemyProjectile"))
            return;

        if (collision.CompareTag("Enemy"))
        {
            Destroy(collision.gameObject); // Destroy enemy (Can replace with damage logic)
        }

        // Play impact animation
        if (animator != null)
        {
            animator.SetTrigger("Explode");
        }

        // Stop movement and disable collider
        rb.velocity = Vector2.zero;
        col.enabled = false;

        // Return to pool after animation
        Invoke(nameof(DisableProjectile), 1f);
    }

    void DisableProjectile()
    {
        gameObject.SetActive(false); // Instead of Destroying
    }
}
