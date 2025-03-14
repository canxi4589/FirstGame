using UnityEngine;

public class Projectile : MonoBehaviour
{
    public float lifetime = 5f;
    private Animator animator;
    private BulletPool1 bulletPool; // Reference to the pool
    public float damage = 1f;

    void Awake()
    {
        animator = GetComponent<Animator>();
    }

    void OnEnable()
    {
        if (animator != null)
        {
            animator.Play("FireBullet"); 
        }

        Invoke(nameof(DisableProjectile), lifetime);
    }
    public void SetPool(BulletPool1 pool)
    {
        bulletPool = pool;
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {

        Debug.Log($"Bullet collided with: {collision.gameObject.name}, Tag: {collision.gameObject.tag}");
        if (collision.CompareTag("Decor") || collision.CompareTag("TileMap"))
        {
            animator.SetTrigger("Explode");
            DisableProjectile();
        }
        if (collision.CompareTag("Enemy"))
        {
            Yellowwizard enemy = collision.GetComponent<Yellowwizard>();
            if (enemy != null)
            {
                enemy.TakeDamage((int)damage);
            }
        }
        if (collision.CompareTag("PlayerProjectile") || collision.CompareTag("Player") || collision.CompareTag("Bound"))
            return;
        if (animator != null)
        {
            animator.SetTrigger("Explode");
        }
        Invoke(nameof(DisableProjectile), 1f);
    }
    void DisableProjectile()
    {
        gameObject.SetActive(false); // Instead of Destroying
    }

}

