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
        rb.velocity = rb.velocity; // Maintain velocity

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

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (hasHit) return;
        hasHit = true;

        // Log the collision
        Debug.Log($"Bullet collided with: {collision.gameObject.name}, Tag: {collision.gameObject.tag}");

        if (collision.gameObject.CompareTag("Enemy") ||
            collision.gameObject.CompareTag("Decor") ||
            collision.gameObject.CompareTag("TileMap"))
        {
            animator.SetTrigger("Explode");
            rb.velocity = Vector2.zero;
            col.enabled = false;
            Invoke(nameof(DisableProjectile), 1f);
        }
    }

    void DisableProjectile()
    {
        gameObject.SetActive(false); // Instead of Destroying
    }
}
