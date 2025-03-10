using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpikeDown : MonoBehaviour
{
    public Animator anim;
    private BoxCollider2D spikeCollider;

    void Start()
    {
        anim = GetComponent<Animator>(); // Use Animator
        spikeCollider = GetComponent<BoxCollider2D>();
    }

    public void HideSpikes()
    {
        anim.SetTrigger("Hide"); 
        Invoke("New Animation", 1.0f); // Adjust based on animation length
    }

    private void DisableSpikes()
    {
        spikeCollider.enabled = false;  // Disable collision
    }
}
