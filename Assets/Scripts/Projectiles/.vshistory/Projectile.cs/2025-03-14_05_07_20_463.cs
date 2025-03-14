using UnityEngine;

public class Projectile : MonoBehaviour
{
    public float lifetime = 5f;
    private Animator animator;
    private bool hasHit = false; // Prevent multiple triggers
    private BulletPool1 bulletPool; // Reference to the pool

    void Awake()
    {
        animator = GetComponent<Animator>();
    }

    public void SetPool(BulletPool1 pool)
    {
        bulletPool = pool;
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (hasHit) return;
        hasHit = true;

        if (collision.CompareTag("PlayerProjectile") || collision.CompareTag("Player") || collision.CompareTag("TileMap") || collision.CompareTag("EnemyProjectile"))
            return;

        if (collision.CompareTag("Enemy") || collision.CompareTag("Decor"))
        {
            Destroy(collision.gameObject); 
        }

        if (animator != null)
        {
            animator.SetTrigger("Explode");
        }

        rb.velocity = Vector2.zero;
        col.enabled = true;

        Invoke(nameof(DisableProjectile), 1f);
    }

    void DisableProjectile()
    {
        gameObject.SetActive(false); // Instead of Destroying
    }
}
