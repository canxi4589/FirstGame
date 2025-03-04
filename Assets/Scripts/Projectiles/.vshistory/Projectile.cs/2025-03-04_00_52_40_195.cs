using UnityEngine;

public class Projectile : MonoBehaviour
{
    public float lifetime = 1f;
    private Animator animator;
    private bool hasHit = false; // Prevent multiple triggers

    void Start()
    {
        animator = GetComponent<Animator>();
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

        // Delay destruction to allow animation to play
        Destroy(gameObject, 0.3f); // Adjust based on animation length
    }
}
