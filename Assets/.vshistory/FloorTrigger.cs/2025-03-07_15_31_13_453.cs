using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FloorTrigger : MonoBehaviour
{
    public GameObject[] doors; // Assign both doors in Inspector

    private Animator animator;
    private Collider[] colliders;

    private void Start()
    {
        // Get Animator from the first door (both use the same animation)
        if (doors.Length > 0)
        {
            animator = doors[0].GetComponent<Animator>();
        }

        // Collect all colliders
        colliders = new Collider[doors.Length];
        for (int i = 0; i < doors.Length; i++)
        {
            colliders[i] = doors[i].GetComponent<Collider>();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            // Play the animation (assumes both doors share the same Animator Controller)
            if (animator != null)
            {
                animator.SetTrigger("Open");
            }

            // Disable colliders after a delay
            StartCoroutine(DisableCollidersAfterDelay(1.5f));
        }
    }

    private IEnumerator DisableCollidersAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        foreach (var col in colliders)
        {
            if (col != null) col.enabled = false;
        }
    }
}
