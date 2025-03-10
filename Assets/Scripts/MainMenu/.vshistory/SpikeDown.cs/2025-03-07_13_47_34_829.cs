using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpikeDown : MonoBehaviour
{
    public Transform downPosition; // Set a Transform for where spikes should go when lowering
    public float moveSpeed = 2f;

    private BoxCollider2D spikeCollider;
    private bool hasMoved = false; // Prevents multiple activations

    void Start()
    {
        spikeCollider = GetComponent<BoxCollider2D>();
    }

    public void LowerSpikes()
    {
        if (!hasMoved) // Only allow lowering once
        {
            hasMoved = true;
            StartCoroutine(MoveSpikes(downPosition.position));
            spikeCollider.enabled = false; // Disable collider so player can pass
        }
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
