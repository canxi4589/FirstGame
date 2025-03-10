using System.Collections;
using UnityEngine;

public class SpikeTrap : MonoBehaviour
{
    public Transform upPosition;  // Set a Transform where spikes should move when up
    public Transform downPosition; // Set a Transform where spikes should move when down
    public float moveSpeed = 2f;
    private bool isUp = false;

    private BoxCollider2D spikeCollider;

    void Start()
    {
        spikeCollider = GetComponent<BoxCollider2D>();
        transform.position = downPosition.position; // Start in down position
    }

    public void ToggleSpikes()
    {
        isUp = !isUp;
        StartCoroutine(MoveSpikes(isUp ? upPosition.position : downPosition.position));
        spikeCollider.enabled = isUp; // Enable collider when up, disable when down
    }

    private IEnumerator MoveSpikes(Vector3 targetPosition)
    {
        while (Vector3.Distance(transform.position, targetPosition) > 0.01f)
        {
            transform.position = Vector3.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);
            yield return null;
        }
    }
}
