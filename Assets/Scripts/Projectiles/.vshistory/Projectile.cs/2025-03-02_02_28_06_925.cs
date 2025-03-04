using UnityEngine;

public class Projectile : MonoBehaviour
{
    public float lifetime = 1f;

    void Start()
    {
        Destroy(gameObject, lifetime); // Destroy bullet after a few seconds
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Enemy")) // Change to match your enemy tag
        {
            Destroy(collision.gameObject); // Destroy enemy
        }
        Destroy(gameObject); // Destroy bullet on impact
    }
}
