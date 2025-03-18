using UnityEngine;

public class SpikeTrap : MonoBehaviour
{
    [SerializeField] private int damage = 1; // Damage dealt to the player
    [SerializeField] private float activationDelay = 0.2f; // Delay before the spikes activate
    [SerializeField] private float cooldown = 1.0f; // Cooldown before the trap can activate again

    private Animator animator;
    private bool canActivate = true;
    private bool isSpikesUp = false; // Tracks if the spikes are in the "up" state
    private PlayerMovement playerInTrigger; // Tracks the player while in the trigger area
    private float lastDamageTime = -Mathf.Infinity; // Tracks the last time damage was dealt

    void Awake()
    {
        animator = GetComponent<Animator>();
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!canActivate) return;

        if (other.CompareTag("Player"))
        {
            playerInTrigger = other.GetComponent<PlayerMovement>();
            StartCoroutine(ActivateTrap());
        }
    }

    void OnTriggerStay2D(Collider2D other)
    {
        if (other.CompareTag("Player") && isSpikesUp && Time.time >= lastDamageTime + cooldown)
        {
            if (playerInTrigger != null)
            {
                playerInTrigger.TakeDamage(damage);
                lastDamageTime = Time.time;
            }
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInTrigger = null;
        }
    }

    private System.Collections.IEnumerator ActivateTrap()
    {
        canActivate = false;

        // Play the armed sound during the delay (warning sound)
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlaySpikeTrapArmedSound();
        }

        // Delay before activation (gives player time to react)
        yield return new WaitForSeconds(activationDelay);

        // Play the spike up animation
        Debug.Log("Setting Activate trigger");
        animator.SetTrigger("Activate "); // Ensure this line is correct
        isSpikesUp = true;

        // Play the shoot sound when the spikes activate
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlaySpikeTrapShootSound();
        }

        // Wait for the SpikeUp animation to finish
        yield return new WaitForSeconds(animator.GetCurrentAnimatorStateInfo(0).length);

        // Wait for the SpikeDown animation to finish (SpikeDown -> SpikeIdle transition)
        yield return new WaitForSeconds(animator.GetCurrentAnimatorStateInfo(0).length);

        isSpikesUp = false; // Reset the spikes state after the animation completes

        // Wait for the remaining cooldown before allowing the trap to activate again
        float totalAnimationTime = animator.GetCurrentAnimatorStateInfo(0).length * 2; // SpikeUp + SpikeDown
        float remainingCooldown = cooldown - totalAnimationTime;
        if (remainingCooldown > 0)
        {
            yield return new WaitForSeconds(remainingCooldown);
        }

        canActivate = true;
    }
}