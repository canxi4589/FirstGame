using UnityEngine;

public class Projectile : MonoBehaviour
{
    public float speed = 5f;
    public float lifetime1 = 3f;
    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        if (rb != null)
        {
            rb.velocity = transform.right * speed; // Moves the projectile forward
        }

        Destroy(gameObject, lifetime1); // Destroy after a set time
    }

    public void SetSprite(Sprite newSprite)
    {
        if (spriteRenderer != null && newSprite != null)
        {
            spriteRenderer.sprite = newSprite;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Enemy")) // Only destroy if it hits an enemy
        {
            Destroy(collision.gameObject); // Destroy the enemy
        }

        Destroy(gameObject); // Destroy the projectile on impact
    }
}
