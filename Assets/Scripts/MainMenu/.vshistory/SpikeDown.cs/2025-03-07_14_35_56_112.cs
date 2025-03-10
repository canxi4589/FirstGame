using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpikeDown : MonoBehaviour
{
    private Animation anim;
    private BoxCollider2D spikeCollider;

    void Start()
    {
        anim = GetComponent<Animation>();
        spikeCollider = GetComponent<BoxCollider2D>();
    }

    public void HideSpikes()
    {
        anim.Play("New Animation"); // Play the animation clip
        Invoke("DisableSpikes", anim["New Animation"].length); // Wait for animation to finish
    }

    private void DisableSpikes()
    {
        spikeCollider.enabled = false;  // Disable collision
    }
}
