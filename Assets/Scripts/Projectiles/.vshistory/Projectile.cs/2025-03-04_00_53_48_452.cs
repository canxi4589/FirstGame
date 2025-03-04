using UnityEngine;

public class Projectile : MonoBehaviour
{
    public float lifetime = 1f;
    private Animator animator;
    private bool hasHit = false; // Prevent multiple triggers

    void Start()
    {
        animator = GetComponent<Animator>();

        // Ensure the flying animation plays when the projectile spawns
        if (animator != null)
        {
            animator.Play("Flying"); // Replace with the actual name of your flying animation
        }

        Destroy(gameObject, lifetime); // Destroy after a while if it doesn't hit anything
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (hasHit) return; // Prevents multiple triggers
        hasHit = true;

        if (collision.CompareTag("Enemy"))
        {
            Destroy(collision.gameObject); // Destroy enemy
        }

        // Play the impact animation
        if (animator != null)
        {
            animator.SetTrigger("Explode"); // Ensure your animation controller has an "Explode" trigger
        }

        // Disable movement so the projectile stops
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.velocity = Vector2.zero; // Stop movement
        }

        // Disable the collider to prevent multiple triggers
        GetComponent<Collider2D>().enabled = false;

        // Delay destruction to allow the animation to play
        Destroy(gameObject, 0.3f); // Adjust based on animation length
    }
}
