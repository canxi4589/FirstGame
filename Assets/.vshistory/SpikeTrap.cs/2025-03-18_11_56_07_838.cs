using UnityEngine;

public class SpikeTrap : MonoBehaviour
{
    [SerializeField] private int damage = 1; // Damage dealt to the player
    [SerializeField] private float activationDelay = 0.2f; // Delay before the spikes activate
    [SerializeField] private float cooldown = 1.0f; // Cooldown before the trap can activate again

    private Animator animator;
    private bool canActivate = true;

    void Awake()
    {
        animator = GetComponent<Animator>();
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!canActivate) return;

        if (other.CompareTag("Player"))
        {
            StartCoroutine(ActivateTrap(other.GetComponent<PlayerMovement>()));
        }
    }

    private System.Collections.IEnumerator ActivateTrap(PlayerMovement player)
    {
        if (player == null) yield break;

        canActivate = false;

        // Optional delay before activation (for warning)
        yield return new WaitForSeconds(activationDelay);

        // Play the spike up animation
        animator.SetTrigger("Activate");

        // Play the spike trap sound
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlaySpikeTrapSound();
        }

        // Deal damage to the player
        player.TakeDamage(damage);

        // Wait for the cooldown before allowing the trap to activate again
        yield return new WaitForSeconds(cooldown);
        canActivate = true;
    }
}