using System.Collections;
using UnityEngine;

public class FloorTrigger : MonoBehaviour
{
    public GameObject[] doors; // Assign both doors in Inspector
    public string animationClipName = "New Animation"; // Set animation clip name in Inspector

    private Animation[] animations;
    private Collider[] colliders;

    private void Start()
    {
        // Initialize arrays
        animations = new Animation[doors.Length];
        colliders = new Collider[doors.Length];

        for (int i = 0; i < doors.Length; i++)
        {
            if (doors[i] != null)
            {
                animations[i] = doors[i].GetComponent<Animation>();
                colliders[i] = doors[i].GetComponent<Collider>();

                // Ensure the Animation component has the clip
                if (animations[i] != null && animations[i][animationClipName] == null)
                {
                    Debug.LogError($"Animation clip '{animationClipName}' not found on {doors[i].name}");
                }
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            foreach (var anim in animations)
            {
                if (anim != null)
                {
                    anim.Play(animationClipName); // Play the assigned animation clip
                }
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
