using UnityEngine;
using System.Collections;

public class Projectile : MonoBehaviour
{
    public float projectileSpeed = 20f;  // Bullet speed
    public float lifetime = 5f;
    private Rigidbody2D rb;
    private Collider2D col;
    private bool hasHit = false;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<Collider2D>();
    }

    void OnEnable()
    {
        hasHit = false;
        col.enabled = true;
        rb.velocity = transform.right * projectileSpeed;  // Set velocity instantly

        // Auto-disable after `lifetime`
        Invoke(nameof(DisableProjectile), lifetime);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (hasHit) return;
        hasHit = true;

        Debug.Log($"Bullet hit: {collision.gameObject.name}, Tag: {collision.gameObject.tag}");

        if (collision.gameObject.CompareTag("Enemy"))
        {
            Destroy(collision.gameObject);  // Replace with damage logic if needed
        }

        // Stop movement and disable collider after impact
        rb.velocity = Vector2.zero;
        StartCoroutine(DisableWithDelay());
    }

    IEnumerator DisableWithDelay()
    {
        yield return new WaitForSeconds(0.1f);
        col.enabled = false;
        yield return new WaitForSeconds(0.3f);
        DisableProjectile();
    }

    void DisableProjectile()
    {
        gameObject.SetActive(false);
    }
}
