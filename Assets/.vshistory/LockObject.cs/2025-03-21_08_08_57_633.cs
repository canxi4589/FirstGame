using UnityEngine;

public class LockObject : MonoBehaviour
{
    private Animator animator;
    private bool isUnlocked = false;

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

        // Start coroutine to disable the lock after the animation
        StartCoroutine(DisableAfterAnimation());
    }

    private System.Collections.IEnumerator DisableAfterAnimation()
    {
        // Wait for the animation to finish
        yield return new WaitForSeconds(animator.GetCurrentAnimatorStateInfo(0).length);
        Debug.Log("LockObject: Disabling lock " + gameObject.name);
        gameObject.SetActive(false);
    }
}