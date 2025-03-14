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

        if (collision.CompareTag("Enemy") || collision.CompareTag("Decor"))
        {
            Destroy(collision.gameObject); 
        }

        // Play impact animation
        if (animator != null)
        {
            animator.SetTrigger("Explode");
        }

        rb.velocity = Vector2.zero;
        col.enabled = false;

        Invoke(nameof(DisableProjectile), 1f);
    }

    void DisableProjectile()
    {
        gameObject.SetActive(false); // Instead of Destroying
    }
}
