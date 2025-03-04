using UnityEngine;

public class PlayerProjectiles : MonoBehaviour
{
    public float speed = 5f;
    public float lifetime = 3f;
    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        // Set initial movement direction
        rb.velocity = transform.right * speed;

        // Destroy projectile after some time
        Destroy(gameObject, lifetime);
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
        // Add damage logic here (e.g., check if it hits an enemy)
        Destroy(gameObject);
    }
}
