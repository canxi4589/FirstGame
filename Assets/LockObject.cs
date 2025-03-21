using UnityEngine;

public class LockObject : MonoBehaviour
{
    private Animator animator;
    private bool isUnlocked = false;
    [SerializeField] private GameObject blocker; // Reference to the Blocker GameObject

    void Awake()
    {
        animator = GetComponent<Animator>();
        if (animator == null)
        {
            Debug.LogError("Animator component missing on LockObject: " + gameObject.name);
        }
    }

    public void Unlock()
    {
        if (isUnlocked) return;

        isUnlocked = true;
        animator.SetTrigger("Unlock");
        Debug.Log("LockObject: Unlock animation triggered on " + gameObject.name);

        // Disable the blocker's collider if assigned
        if (blocker != null)
        {
            Collider2D blockerCollider = blocker.GetComponent<Collider2D>();
            if (blockerCollider != null && !blockerCollider.isTrigger)
            {
                blockerCollider.enabled = false;
                Debug.Log($"LockObject: Disabled collider on {blockerCollider.gameObject.name}");
            }
            else
            {
                Debug.LogWarning("LockObject: No collider found on assigned Blocker!");
            }
        }
        else
        {
            Debug.LogWarning("LockObject: Blocker not assigned!");
        }

        StartCoroutine(DisableAfterAnimation());
    }

    private System.Collections.IEnumerator DisableAfterAnimation()
    {
        yield return new WaitForSeconds(animator.GetCurrentAnimatorStateInfo(0).length);
        Debug.Log("LockObject: Disabling lock " + gameObject.name);
        gameObject.SetActive(false);
    }
}