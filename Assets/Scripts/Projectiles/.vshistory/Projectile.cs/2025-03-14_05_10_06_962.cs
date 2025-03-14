using UnityEngine;
using UnityEngine.Tilemaps;

public class Projectile : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 5f;  // Bullet speed (adjustable)
    [SerializeField] private float fireRange = 10f; // Max distance before deactivation
    private Vector2 startPosition;
    private Vector2 moveDirection;

    private Animator animator;
    private bool hasHit = false;
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
        hasHit = false;
        col.enabled = true;

        startPosition = transform.position;  // Store initial position
        rb.velocity = moveDirection * moveSpeed; // Set bullet movement

        if (animator != null)
        {
            animator.Play("FireBullet"); // Play shooting animation
        }

        Invoke(nameof(DisableProjectile), 2f); // Auto-disable after lifetime
    }

    public void SetMoveDirection(Vector2 dir)
    {
        moveDirection = dir.normalized;
    }

    private void Update()
    {
        // If the bullet exceeds max distance, deactivate it
        if (Vector2.Distance(startPosition, transform.position) > fireRange)
        {
            DisableProjectile();
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (hasHit) return;
        hasHit = true;
        Debug.Log($"Bullet collided with: {collision.gameObject.name}, Tag: {collision.gameObject.tag}, Layer: {LayerMask.LayerToName(collision.gameObject.layer)}, Is Tilemap: {collision is TilemapCollider2D}");

        if (collision.CompareTag("PlayerProjectile") || collision.CompareTag("Player") ||
            collision.CompareTag("TileMap") || collision.CompareTag("EnemyProjectile"))
            return;

        if (collision.CompareTag("Enemy") || collision.CompareTag("Decor"))
        {
            Destroy(collision.gameObject);
        }

        if (animator != null)
        {
            animator.SetTrigger("Explode"); // Trigger explosion animation
        }

        rb.velocity = Vector2.zero; // Stop movement
        col.enabled = false; // Disable collider

        Invoke(nameof(DisableProjectile), 0.5f); // Wait for explosion animation
    }
    public void SetPool(BulletPool1 pool)
    {
        bulletPool = pool;
    }

    void DisableProjectile()
    {
        gameObject.SetActive(false);
    }
}
